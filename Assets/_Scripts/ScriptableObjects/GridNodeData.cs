using UnityEngine;

[CreateAssetMenu(fileName = "GridNodeData", menuName = "Grid/Grid Node Data")]
public class GridNodeData : ScriptableObject
{
    public GridNode gridNodePrefab;
    public bool isWalkable;
    public bool isPlayerWalkable;
}
