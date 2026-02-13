using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class NPCRoamPlanner : MonoBehaviour
{
    // ==========================
    // Config
    // ==========================
    [Header("Roam Destination Selection")]
    [SerializeField] bool roamUsesBfsVisibleTargets = true;

    [Tooltip("If true, roam targets must also be inside the cone (not just BFS reachable).")]
    [SerializeField] bool roamTargetsMustBeInCone = false;

    [Tooltip("How many times to try to pick a valid roam destination before falling back.")]
    [SerializeField] int roamPickAttempts = 10;

    [Tooltip("Don't pick destinations closer than this many BFS steps.")]
    [SerializeField] int roamMinDestinationDistanceNodes = 3;

    [Tooltip("Once a roam destination is chosen, keep it for this many NPC turns (moves/turns), unless reached/invalid.")]
    [SerializeField] int roamKeepDestinationForTurns = 4;

    // ==========================
    // Runtime
    // ==========================
    public GridNode CurrentDestination => currentRoamDestination;
    public int TurnsRemaining => roamDestinationTurnsRemaining;

    GridNode currentRoamDestination;
    int roamDestinationTurnsRemaining;

    // Reused buffer to avoid GC
    readonly List<GridNode> candidates = new List<GridNode>(256);

    // ==========================
    // Public API
    // ==========================
    public void CancelDestination()
    {
        currentRoamDestination = null;
        roamDestinationTurnsRemaining = 0;
    }

    public void ConsumeTurn()
    {
        if (roamDestinationTurnsRemaining > 0)
            roamDestinationTurnsRemaining--;
    }

    public void InvalidateIfBlocked()
    {
        if (currentRoamDestination != null && !IsValidDestination(currentRoamDestination))
            CancelDestination();
    }

    public GridNode GetRoamTarget(GridNode currentNode, GridNode fallbackAdjacent, NPCVisionSensor vision)
    {
        if (!roamUsesBfsVisibleTargets || vision == null || currentNode == null)
            return fallbackAdjacent;

        bool reached = (currentRoamDestination != null && currentNode == currentRoamDestination);
        bool expired = (roamDestinationTurnsRemaining <= 0);
        bool invalid = (currentRoamDestination != null && !IsValidDestination(currentRoamDestination));

        if (currentRoamDestination == null || reached || expired || invalid)
        {
            currentRoamDestination = null;

            // ✅ refresh vision ONCE here
            vision.Refresh(currentNode);

            for (int i = 0; i < roamPickAttempts; i++)
            {
                GridNode pick = PickRandomVisibleDestination(currentNode, vision);
                if (pick != null)
                {
                    currentRoamDestination = pick;
                    roamDestinationTurnsRemaining = roamKeepDestinationForTurns;
                    break;
                }
            }
        }

        return currentRoamDestination ?? fallbackAdjacent;
    }

    // ==========================
    // Internals
    // ==========================
    GridNode PickRandomVisibleDestination(GridNode start, NPCVisionSensor vision)
    {
        var bfsNodes = vision.LastBfsNodes;
        if (bfsNodes == null || bfsNodes.Count <= 1) return null;

        candidates.Clear();

        if (!roamTargetsMustBeInCone)
        {
            // BFS-only candidates
            for (int i = 0; i < bfsNodes.Count; i++)
            {
                GridNode node = bfsNodes[i];
                if (node == null || node == start) continue;

                int d = vision.GetLastBfsDistanceSteps(node);
                if (d < roamMinDestinationDistanceNodes) continue;

                if (!IsValidDestination(node)) continue;

                candidates.Add(node);
            }
        }
        else
        {
            // Cone-filtered candidates
            var coneNodes = vision.LastConeNodes;
            if (coneNodes == null || coneNodes.Count == 0) return null;

            for (int i = 0; i < bfsNodes.Count; i++)
            {
                GridNode node = bfsNodes[i];
                if (node == null || node == start) continue;

                int d = vision.GetLastBfsDistanceSteps(node);
                if (d < roamMinDestinationDistanceNodes) continue;

                if (!IsValidDestination(node)) continue;
                if (!ContainsNode(coneNodes, node)) continue;

                candidates.Add(node);
            }
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    bool ContainsNode(IReadOnlyList<GridNode> list, GridNode node)
    {
        if (list == null || node == null) return false;
        for (int i = 0; i < list.Count; i++)
            if (list[i] == node)
                return true;
        return false;
    }

    bool IsValidDestination(GridNode node)
    {
        if (node == null) return false;
        if (node.GetIsVoid()) return false;
        if (node.nodeData == null || !node.nodeData.isWalkable) return false;
        if (node.currentOccupant == null) return false;

        var occ = node.currentOccupant.occupantType;
        return (occ == GridNodeOccupantType.None || occ == GridNodeOccupantType.PressurePlate);
    }
}
