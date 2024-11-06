using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Tiles;
using UnityEngine;

public static class Pathfinding_Custom
{
    private static readonly Color PathColor = new Color(0.65f, 0.35f, 0.35f);
    private static readonly Color OpenColor = new Color(.4f, .6f, .4f);
    private static readonly Color ClosedColor = new Color(0.35f, 0.4f, 0.5f);
        
    public static List<GridNode> FindPath(GridNode startNode, GridNode targetNode) {
        var toSearch = new List<GridNode>() { startNode };
        var processed = new List<GridNode>();

        while (toSearch.Any()) {
            var current = toSearch[0];
            foreach (var t in toSearch) 
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;

            processed.Add(current);
            toSearch.Remove(current);

            current.HighlightCellClosed();

            if (current == targetNode) {
                var currentPathTile = targetNode;
                var path = new List<GridNode>();
                var count = 100;
                while (currentPathTile != startNode) {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                    count--;
                    //if (count < 0) throw new Exception();
                    //Debug.Log("sdfsdf");
                }
                    
                foreach (GridNode node in path) node.HighlightCellPath();
                startNode.HighlightCellPath();
                //Debug.Log(path.Count);
                return path;
            }

            foreach (var neighbor in current.neighbouringNodes.Where(t => t.nodeData.isWalkable && !processed.Contains(t))) {
                var inSearch = toSearch.Contains(neighbor);

                var costToNeighbor = current.G + current.GetDistance(neighbor);

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