using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NPCVisionSensor : MonoBehaviour
{
    // ==========================
    // Config
    // ==========================
    [Header("Vision (BFS + Cone)")]
    [SerializeField] bool enablePlayerDetection = true;

    [Tooltip("How many BFS steps (nodes) out from the NPC to consider.")]
    [SerializeField] int detectRangeNodes = 6;

    [Tooltip("Total cone angle in degrees. 90 = 45 degrees left/right.")]
    [SerializeField] float detectConeAngle = 90f;

    [Tooltip("If true, BFS expands using diagonal neighbours as well.")]
    [SerializeField] bool bfsIncludeDiagonals = false;

    [Tooltip("Which transform represents the NPC's facing direction (forward/right).")]
    [SerializeField] Transform orientation;

    // ==========================
    // Debug
    // ==========================
    [Header("Debug (Gizmos + Tile Highlight)")]
    [SerializeField] bool debugDrawConeGizmos = true;

    [Tooltip("Highlights cone nodes (and optionally BFS nodes) using GridNode highlight materials.")]
    [SerializeField] bool debugHighlightNodes = true;

    [Tooltip("Also highlight BFS-reachable nodes (not just the cone nodes).")]
    [SerializeField] bool debugHighlightBfsReachable = false;

    [Tooltip("Auto-clear highlights after this many seconds. 0 = clear on next update/disable.")]
    [SerializeField] float debugHighlightDuration = 0f;

    [Tooltip("Draw spheres over BFS nodes and cone nodes.")]
    [SerializeField] bool debugDrawNodeSpheres = true;

    [SerializeField] float debugBfsSphereRadius = 0.12f;
    [SerializeField] float debugConeSphereRadius = 0.18f;

    [Tooltip("World units per node (for gizmo range only).")]
    [SerializeField] int gridSizeWorldUnits = 3;

    // ==========================
    // Runtime caches
    // ==========================
    Dictionary<GridNode, int> lastBfsDistances = new Dictionary<GridNode, int>();
    readonly List<GridNode> lastBfsNodes = new List<GridNode>();
    readonly List<GridNode> lastConeNodes = new List<GridNode>();

    readonly List<GridNode> lastHighlightedNodes = new List<GridNode>();
    Coroutine clearHighlightRoutine;

    // ==========================
    // Public API
    // ==========================
    public bool Enabled => enablePlayerDetection;
    public int DetectRangeNodes => detectRangeNodes;
    public float DetectConeAngle => detectConeAngle;
    public bool IncludeDiagonals => bfsIncludeDiagonals;

    public IReadOnlyDictionary<GridNode, int> LastDistances => lastBfsDistances;
    public IReadOnlyList<GridNode> LastBfsNodes => lastBfsNodes;
    public IReadOnlyList<GridNode> LastConeNodes => lastConeNodes;

    public void SetOrientation(Transform newOrientation) => orientation = newOrientation;

    public void Refresh(GridNode npcNode)
    {
        if (!enablePlayerDetection) return;
        if (npcNode == null) return;
        if (orientation == null) return;

        // 1) BFS
        lastBfsDistances = BFS_Distances(npcNode, detectRangeNodes, bfsIncludeDiagonals);

        // 2) Cache BFS nodes
        lastBfsNodes.Clear();
        foreach (var kvp in lastBfsDistances)
            lastBfsNodes.Add(kvp.Key);

        // 3) Cone filter
        lastConeNodes.Clear();
        var cone = FilterNodesByCone(lastBfsNodes, orientation, detectConeAngle);
        for (int i = 0; i < cone.Count; i++)
            lastConeNodes.Add(cone[i]);

        // 4) Debug highlight
        if (debugHighlightNodes)
            ApplyVisionHighlights(lastBfsNodes, lastConeNodes);
        else
            ClearHighlights();
    }

    public bool IsPlayerDetected(GridNode npcNode, GridNode playerNode)
    {
        if (!enablePlayerDetection) return false;
        if (npcNode == null || playerNode == null) return false;
        if (orientation == null) return false;

        // Keep cache in sync with detection
        Refresh(npcNode);

        if (!lastBfsDistances.ContainsKey(playerNode))
            return false;

        for (int i = 0; i < lastConeNodes.Count; i++)
            if (lastConeNodes[i] == playerNode)
                return true;

        return false;
    }

    // ==========================
    // Core: BFS + Cone
    // ==========================
    Dictionary<GridNode, int> BFS_Distances(GridNode start, int maxSteps, bool includeDiagonals)
    {
        var dist = new Dictionary<GridNode, int>(128);
        var q = new Queue<GridNode>();

        dist[start] = 0;
        q.Enqueue(start);

        while (q.Count > 0)
        {
            GridNode cur = q.Dequeue();
            int d = dist[cur];

            if (d >= maxSteps)
                continue;

            List<GridNode> neighbours = cur.GetNeighbouringNodes(includeDiagonals);

            for (int i = 0; i < neighbours.Count; i++)
            {
                GridNode next = neighbours[i];
                if (next == null) continue;
                if (dist.ContainsKey(next)) continue;

                if (!IsNodeTraversableForVision(next)) continue;

                dist[next] = d + 1;
                q.Enqueue(next);
            }
        }

        return dist;
    }

    List<GridNode> FilterNodesByCone(List<GridNode> nodes, Transform facing, float coneAngleDeg)
    {
        var result = new List<GridNode>(nodes.Count);
        Vector3 origin = facing.position;
        Vector3 forward = facing.forward;

        float half = coneAngleDeg * 0.5f;

        for (int i = 0; i < nodes.Count; i++)
        {
            GridNode node = nodes[i];
            if (node == null) continue;

            Vector3 targetPos = GetNodeWorldPos(node);

            Vector3 to = targetPos - origin;
            to.y = 0f;

            if (to.sqrMagnitude < 0.0001f)
                continue;

            float angle = Vector3.Angle(forward, to.normalized);
            if (angle <= half)
                result.Add(node);
        }

        return result;
    }

    bool IsNodeTraversableForVision(GridNode node)
    {
        if (node == null) return false;
        if (node.GetIsVoid()) return false;

        if (node.nodeData == null || !node.nodeData.isWalkable) return false;

        switch (node.GetOccupantType())
        {
            case GridNodeOccupantType.Obstacle:
            case GridNodeOccupantType.NPCInaccessible:
                return false;

            default:
                return true;
        }
    }

    Vector3 GetNodeWorldPos(GridNode node)
    {
        if (node == null) return Vector3.zero;
        if (node.moveToTransform != null) return node.moveToTransform.position;
        return node.transform.position;
    }

    // ==========================
    // Debug: Highlight tiles
    // ==========================
    void ApplyVisionHighlights(List<GridNode> bfsNodes, List<GridNode> coneNodes)
    {
        ClearHighlights();

        if (debugHighlightBfsReachable)
        {
            for (int i = 0; i < bfsNodes.Count; i++)
            {
                GridNode n = bfsNodes[i];
                if (n == null) continue;
                n.HighlightCellClosed();
                lastHighlightedNodes.Add(n);
            }
        }

        for (int i = 0; i < coneNodes.Count; i++)
        {
            GridNode n = coneNodes[i];
            if (n == null) continue;
            n.HighlightCellClosed();
            if (!lastHighlightedNodes.Contains(n))
                lastHighlightedNodes.Add(n);
        }

        if (debugHighlightDuration > 0f)
        {
            if (clearHighlightRoutine != null)
                StopCoroutine(clearHighlightRoutine);
            clearHighlightRoutine = StartCoroutine(ClearHighlightsAfterDelay(debugHighlightDuration));
        }
    }

    IEnumerator ClearHighlightsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearHighlights();
        clearHighlightRoutine = null;
    }

    public void ClearHighlights()
    {
        if (clearHighlightRoutine != null && debugHighlightDuration <= 0f)
        {
            StopCoroutine(clearHighlightRoutine);
            clearHighlightRoutine = null;
        }

        for (int i = 0; i < lastHighlightedNodes.Count; i++)
        {
            GridNode n = lastHighlightedNodes[i];
            if (n != null)
                n.UnhighlightCell();
        }
        lastHighlightedNodes.Clear();
    }

    // ==========================
    // Debug: Gizmos
    // ==========================
    private void OnDrawGizmosSelected()
    {
        if (!debugDrawConeGizmos) return;
        if (orientation == null) return;

        float worldRange = detectRangeNodes * gridSizeWorldUnits;
        Vector3 origin = orientation.position;
        Vector3 forward = orientation.forward;

        Quaternion leftRot = Quaternion.AngleAxis(-detectConeAngle * 0.5f, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(detectConeAngle * 0.5f, Vector3.up);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + forward * worldRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + leftDir * worldRange);
        Gizmos.DrawLine(origin, origin + rightDir * worldRange);

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        const int arcSegments = 16;
        Vector3 prev = origin + leftDir * worldRange;
        for (int i = 1; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            float ang = Mathf.Lerp(-detectConeAngle * 0.5f, detectConeAngle * 0.5f, t);
            Vector3 dir = Quaternion.AngleAxis(ang, Vector3.up) * forward;
            Vector3 p = origin + dir * worldRange;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }

        if (!debugDrawNodeSpheres) return;

        if (lastBfsNodes != null && lastBfsNodes.Count > 0)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
            for (int i = 0; i < lastBfsNodes.Count; i++)
            {
                GridNode n = lastBfsNodes[i];
                if (n == null) continue;
                Gizmos.DrawSphere(GetNodeWorldPos(n), debugBfsSphereRadius);
            }
        }

        if (lastConeNodes != null && lastConeNodes.Count > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.55f);
            for (int i = 0; i < lastConeNodes.Count; i++)
            {
                GridNode n = lastConeNodes[i];
                if (n == null) continue;
                Gizmos.DrawSphere(GetNodeWorldPos(n), debugConeSphereRadius);
            }
        }
    }
}
