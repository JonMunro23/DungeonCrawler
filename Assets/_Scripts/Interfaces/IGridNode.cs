using UnityEngine;

public interface IGridNode
{
    public Vector2Int GetCoords();
    public int GetLevelIndex();
    public void SetLevelIndex(int _levelIndex);
    public void SetOccupyingNode(GridNode occupyingNode);
}
