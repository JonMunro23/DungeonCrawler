using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class RadiationEmitter : MonoBehaviour
{
    [Header("Radiation")]
    [SerializeField] int maxDistanceNodes = 5;
    [SerializeField] bool includeDiagonals = false;

    [Header("Rebuild Triggers")]
    [SerializeField] bool rebuildIfEmitterMovesBetweenNodes = true;

    // Cache of nodes this emitter irradiated last time (so we can undo cleanly)
    readonly List<GridNode> previousIrradiated = new List<GridNode>(256);

    // BFS buffers (reused, no allocations after resize)
    int[] dist;
    int[] stamp;
    int currentStamp = 1;

    int[] queue;
    int qHead, qTail;

    int cachedNodeCount = -1;

    GridNode cachedSourceNode;

    void OnEnable()
    {
        GridController.onLevelFinishedGenerating += OnLevelFinishedGenerating;
    }

    void OnDisable()
    {
        GridController.onLevelFinishedGenerating -= OnLevelFinishedGenerating;

        // Remove ONLY this emitter's contribution
        ClearPreviousContribution();
    }

    void OnLevelFinishedGenerating()
    {
        EnsureBuffersSized();
        RebuildRadiation();
    }

    public void Init(GridNode spawnNode, int maxDistanceNodes)
    {
        cachedSourceNode = spawnNode;
        this.maxDistanceNodes = maxDistanceNodes;
    }

    void Update()
    {
        if (!rebuildIfEmitterMovesBetweenNodes) return;
        if (GridController.Instance == null) return;

        GridNode now = GridController.Instance.GetNodeFromWorldPos(transform.position);
        if (now != cachedSourceNode)
        {
            cachedSourceNode = now;
            RebuildRadiation();
        }
    }

    void OnValidate()
    {
        maxDistanceNodes = Mathf.Max(0, maxDistanceNodes);

        if (!Application.isPlaying) return;
        RebuildRadiation();
    }

    // -------------------------
    // Core
    // -------------------------
    public void RebuildRadiation()
    {
        if (GridController.Instance == null) return;

        EnsureBuffersSized();

        // 1) Undo our previous irradiation safely (ref-count decrement)
        ClearPreviousContribution();

        // 2) Find source node
        cachedSourceNode = GridController.Instance.GetNodeFromWorldPos(transform.position);
        if (cachedSourceNode == null) return;

        // If the emitter is sitting on a wall/void, do nothing
        if (!IsRadiationPassable(cachedSourceNode))
            return;

        // 3) BFS and apply new irradiation (ref-count increment)
        RunBfsAndApply(cachedSourceNode);
    }

    void ClearPreviousContribution()
    {
        for (int i = 0; i < previousIrradiated.Count; i++)
        {
            var node = previousIrradiated[i];
            if (node != null)
                node.RemoveIrradiationSource();
        }
        previousIrradiated.Clear();
    }

    void RunBfsAndApply(GridNode startNode)
    {
        int maxSteps = maxDistanceNodes;

        // Stamp trick for visited
        currentStamp++;
        if (currentStamp == int.MaxValue)
        {
            currentStamp = 1;
            System.Array.Clear(stamp, 0, stamp.Length);
        }

        qHead = 0;
        qTail = 0;

        int startIdx = startNode.NodeIndex;
        if (startIdx < 0 || startIdx >= stamp.Length) return;

        stamp[startIdx] = currentStamp;
        dist[startIdx] = 0;
        Enqueue(startIdx);

        ApplyNode(startNode);

        while (qHead != qTail)
        {
            int curIdx = Dequeue();
            int d = dist[curIdx];

            if (d >= maxSteps)
                continue;

            GridNode curNode = GridController.Instance.GetNodeByIndex(curIdx);
            if (curNode == null) continue;

            var neighbours = includeDiagonals ? curNode.allNeighbouringNodes : curNode.neighbouringNodes;
            if (neighbours == null) continue;

            for (int i = 0; i < neighbours.Count; i++)
            {
                GridNode next = neighbours[i];
                if (next == null) continue;

                // ✅ NEW: can't irradiate or traverse walls/void/unwalkable nodes
                if (!IsRadiationPassable(next))
                    continue;

                int nextIdx = next.NodeIndex;
                if (nextIdx < 0 || nextIdx >= stamp.Length) continue;

                if (stamp[nextIdx] == currentStamp)
                    continue;

                stamp[nextIdx] = currentStamp;
                dist[nextIdx] = d + 1;

                ApplyNode(next);
                Enqueue(nextIdx);
            }
        }
    }

    void ApplyNode(GridNode node)
    {
        // Safety (in case called elsewhere)
        if (!IsRadiationPassable(node))
            return;

        node.AddIrradiationSource();
        previousIrradiated.Add(node);
    }

    // ✅ NEW: One place to define "wall" / "passable"
    bool IsRadiationPassable(GridNode node)
    {
        if (node == null) return false;
        //if (node.GetIsVoid()) return false;
        if (node.nodeData == null) return false;

        // Walls in your project are simply non-walkable nodes
        if (!node.nodeData.isWalkable) return false;

        return true;
    }

    // -------------------------
    // Buffers
    // -------------------------
    void EnsureBuffersSized()
    {
        if (GridController.Instance == null) return;

        int count = GridController.Instance.CurrentNodeCount;
        if (count <= 0) return;

        if (count != cachedNodeCount || queue == null)
        {
            cachedNodeCount = count;

            dist = new int[count];
            stamp = new int[count];
            queue = new int[count];

            currentStamp = 1;
        }
    }

    void Enqueue(int idx)
    {
        queue[qTail++] = idx;
        if (qTail >= queue.Length) qTail = 0;
    }

    int Dequeue()
    {
        int idx = queue[qHead++];
        if (qHead >= queue.Length) qHead = 0;
        return idx;
    }
}
