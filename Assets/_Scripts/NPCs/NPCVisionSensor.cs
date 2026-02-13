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
    [SerializeField] bool debugHighlightNodes = false;

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
    // Runtime caches (lists reused)
    // ==========================
    readonly List<GridNode> lastBfsNodes = new List<GridNode>(256);
    readonly List<GridNode> lastConeNodes = new List<GridNode>(128);

    readonly List<GridNode> lastHighlightedNodes = new List<GridNode>(256);
    Coroutine clearHighlightRoutine;

    // ==========================
    // Allocation-free BFS buffers
    // ==========================
    int[] dist;
    int[] stamp;
    int currentStamp = 1;

    int[] queue;
    int qHead, qTail;

    // ==========================
    // Public API
    // ==========================
    public bool Enabled => enablePlayerDetection;
    public IReadOnlyList<GridNode> LastBfsNodes => lastBfsNodes;
    public IReadOnlyList<GridNode> LastConeNodes => lastConeNodes;

    public void SetOrientation(Transform newOrientation) => orientation = newOrientation;

    public void ClearHighlights()
    {
        if (clearHighlightRoutine != null && debugHighlightDuration <= 0f)
        {
            StopCoroutine(clearHighlightRoutine);
            clearHighlightRoutine = null;
        }

        for (int i = 0; i < lastHighlightedNodes.Count; i++)
        {
            var n = lastHighlightedNodes[i];
            if (n != null) n.UnhighlightCell();
        }
        lastHighlightedNodes.Clear();
    }

    public void Refresh(GridNode npcNode)
    {
        if (!enablePlayerDetection) return;
        if (npcNode == null) return;
        if (orientation == null) return;

        EnsureBuffersSized();

        RunBfs(npcNode, detectRangeNodes, bfsIncludeDiagonals);
        BuildConeFromBfsNodes();

        if (debugHighlightNodes) ApplyVisionHighlights();
        else ClearHighlights();
    }

    public bool IsPlayerDetected(GridNode npcNode, GridNode playerNode)
    {
        if (!enablePlayerDetection) return false;
        if (npcNode == null || playerNode == null) return false;
        if (orientation == null) return false;

        EnsureBuffersSized();

        RunBfs(npcNode, detectRangeNodes, bfsIncludeDiagonals);

        if (!IsVisited(playerNode.NodeIndex))
        {
            lastConeNodes.Clear();
            if (debugHighlightNodes) ApplyVisionHighlights(); else ClearHighlights();
            return false;
        }

        bool playerInCone = IsNodeInCone(playerNode);

        // Keep caches up to date for roam planner / debug
        BuildConeFromBfsNodes();
        if (debugHighlightNodes) ApplyVisionHighlights(); else ClearHighlights();

        return playerInCone;
    }

    public int GetLastBfsDistanceSteps(GridNode node)
    {
        if (node == null) return int.MaxValue;

        int idx = node.NodeIndex;
        if (stamp == null || dist == null) return int.MaxValue;
        if (idx < 0 || idx >= stamp.Length) return int.MaxValue;

        if (stamp[idx] != currentStamp) return int.MaxValue;

        return dist[idx];
    }

    // ==========================
    // Buffer sizing
    // ==========================
    void EnsureBuffersSized()
    {
        // ✅ must match NodeIndex range for current level
        int nodeCount = GridController.Instance.CurrentNodeCount;
        if (nodeCount <= 0) return;

        if (dist == null || dist.Length != nodeCount)
        {
            dist = new int[nodeCount];
            stamp = new int[nodeCount];
            queue = new int[nodeCount];

            currentStamp = 1;
        }
    }

    bool IsVisited(int nodeIndex)
    {
        if (stamp == null) return false;
        if (nodeIndex < 0 || nodeIndex >= stamp.Length) return false;
        return stamp[nodeIndex] == currentStamp;
    }

    // ==========================
    // BFS (allocation-free)
    // ==========================
    void RunBfs(GridNode startNode, int maxSteps, bool includeDiagonals)
    {
        currentStamp++;
        if (currentStamp == int.MaxValue)
        {
            currentStamp = 1;
            System.Array.Clear(stamp, 0, stamp.Length);
        }

        lastBfsNodes.Clear();
        lastConeNodes.Clear();

        qHead = 0;
        qTail = 0;

        int startIdx = startNode.NodeIndex;
        if (startIdx < 0 || startIdx >= stamp.Length) return;

        stamp[startIdx] = currentStamp;
        dist[startIdx] = 0;

        Enqueue(startIdx);
        lastBfsNodes.Add(startNode);

        while (qHead != qTail)
        {
            int curIdx = Dequeue();
            int d = dist[curIdx];

            if (d >= maxSteps)
                continue;

            GridNode curNode = GetNodeByIndex(curIdx);
            if (curNode == null) continue;

            var neigh = includeDiagonals ? curNode.allNeighbouringNodes : curNode.neighbouringNodes;
            if (neigh == null) continue;

            for (int i = 0; i < neigh.Count; i++)
            {
                GridNode next = neigh[i];
                if (next == null) continue;

                int nextIdx = next.NodeIndex;
                if (nextIdx < 0 || nextIdx >= stamp.Length) continue;

                if (stamp[nextIdx] == currentStamp) continue;
                if (!IsNodeTraversableForVision(next)) continue;

                stamp[nextIdx] = currentStamp;
                dist[nextIdx] = d + 1;

                Enqueue(nextIdx);
                lastBfsNodes.Add(next);
            }
        }
    }

    void Enqueue(int idx)
    {
        queue[qTail] = idx;
        qTail++;
        if (qTail >= queue.Length) qTail = 0;
    }

    int Dequeue()
    {
        int idx = queue[qHead];
        qHead++;
        if (qHead >= queue.Length) qHead = 0;
        return idx;
    }

    GridNode GetNodeByIndex(int nodeIndex)
    {
        // ✅ O(1) lookup (requires GridController to swap nodesByIndex for active level)
        return GridController.Instance.GetNodeByIndex(nodeIndex);
    }

    // ==========================
    // Cone filtering
    // ==========================
    void BuildConeFromBfsNodes()
    {
        lastConeNodes.Clear();

        Vector3 origin = orientation.position;
        Vector3 forward = orientation.forward;
        float half = detectConeAngle * 0.5f;

        for (int i = 0; i < lastBfsNodes.Count; i++)
        {
            GridNode node = lastBfsNodes[i];
            if (node == null) continue;

            // ✅ skip self (first BFS node is start)
            if (i == 0) continue;

            Vector3 targetPos = GetNodeWorldPos(node);

            Vector3 to = targetPos - origin;
            to.y = 0f;

            if (to.sqrMagnitude < 0.0001f)
                continue;

            float angle = Vector3.Angle(forward, to.normalized);
            if (angle <= half)
                lastConeNodes.Add(node);
        }
    }

    bool IsNodeInCone(GridNode node)
    {
        Vector3 origin = orientation.position;
        Vector3 forward = orientation.forward;
        float half = detectConeAngle * 0.5f;

        Vector3 to = GetNodeWorldPos(node) - origin;
        to.y = 0f;

        if (to.sqrMagnitude < 0.0001f) return true;

        float angle = Vector3.Angle(forward, to.normalized);
        return angle <= half;
    }

    Vector3 GetNodeWorldPos(GridNode node)
    {
        if (node == null) return Vector3.zero;
        if (node.moveToTransform != null) return node.moveToTransform.position;
        return node.transform.position;
    }

    // ==========================
    // Vision flood rules
    // ==========================
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

    // ==========================
    // Debug: Highlight tiles
    // ==========================
    void ApplyVisionHighlights()
    {
        ClearHighlights();

        if (debugHighlightBfsReachable)
        {
            for (int i = 0; i < lastBfsNodes.Count; i++)
            {
                var n = lastBfsNodes[i];
                if (n == null) continue;
                n.HighlightCellClosed();
                lastHighlightedNodes.Add(n);
            }
        }

        for (int i = 0; i < lastConeNodes.Count; i++)
        {
            var n = lastConeNodes[i];
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
                var n = lastBfsNodes[i];
                if (n == null) continue;
                Gizmos.DrawSphere(GetNodeWorldPos(n), debugBfsSphereRadius);
            }
        }

        if (lastConeNodes != null && lastConeNodes.Count > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.55f);
            for (int i = 0; i < lastConeNodes.Count; i++)
            {
                var n = lastConeNodes[i];
                if (n == null) continue;
                Gizmos.DrawSphere(GetNodeWorldPos(n), debugConeSphereRadius);
            }
        }
    }
}
