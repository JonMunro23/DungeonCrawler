using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding_Custom
{       
    public static List<GridNode> FindPath(GridNode startNode, GridNode targetNode) {
        List<GridNode> toSearch = new List<GridNode>() { startNode };
        List<GridNode> processed = new List<GridNode>();

        while (toSearch.Any()) {
            GridNode current = toSearch[0];
            foreach (var t in toSearch) 
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;

            processed.Add(current);
            toSearch.Remove(current);

            current.HighlightCellClosed();

            if (current == targetNode) {
                GridNode currentPathTile = targetNode;
                List<GridNode> path = new List<GridNode>();
                int count = 100;
                while (currentPathTile != startNode) {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                    count--;
                }
                    
                foreach (GridNode node in path) node.HighlightCellPath();
                startNode.HighlightCellPath();
                return path;
            }

            foreach (GridNode neighbor in current.neighbouringNodes.Where(t => t.nodeData.isWalkable && t.currentOccupant.occupantType != GridNodeOccupantType.NPC && t.currentOccupant.occupantType != GridNodeOccupantType.Obstacle && !processed.Contains(t))) {
                bool inSearch = toSearch.Contains(neighbor);

                float costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G) {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch) {
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Add(neighbor);
                        neighbor.HighlightCellOpen();
                    }
                }
            }
        }
        return null;
    }
 }