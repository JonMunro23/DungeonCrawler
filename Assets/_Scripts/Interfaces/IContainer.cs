using System.Collections.Generic;
using UnityEngine;

public interface IContainer
{
    public void InitContainer(int levelIndex, Vector2 coords);
    public void AddNewStoredItem(int containerIndex, ItemStack itemStackToAdd);
    public List<ContainerItemStack> GetStoredItems();
    public Vector2 GetCoords();
    public int GetLevelIndex();
    public void ToggleContainer();
    public void CloseContainer();
    public bool IsOpen();
}
