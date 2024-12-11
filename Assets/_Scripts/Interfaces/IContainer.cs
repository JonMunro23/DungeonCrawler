using System.Collections.Generic;
using UnityEngine;

public interface IContainer
{
    public void InitContainer(int levelIndex, Vector2 coords);
    public void AddNewStoredItemStack(ContainerItemStack itemToAdd);
    public void LoadContainerItemStacks(List<ContainerItemStack> itemStacks);
    public List<ContainerItemStack> GetStoredItems();
    public Vector2 GetCoords();
    public int GetLevelIndex();
    public float GetRotation();
    public void ToggleContainer();
    public void CloseContainer();
    public bool IsOpen();
    public void Destroy();
}
