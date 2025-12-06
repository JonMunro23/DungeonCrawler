using UnityEngine;

public interface IGridNode
{
    public Vector2 GetCoords();
    public int GetLevelIndex();
    public void SetLevelIndex(int _levelIndex);
    public void SetOccupyingNode(GridNode occupyingNode);
}
