using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public void Interact();  
    public void InteractWithItem(ItemData item);
    public void AddObjectToTrigger(ITriggerable objectToTrigger);
    public void AddEntityRefToTrigger(Dictionary<string, object> entityRefToTrigger);
    public List<string> GetEntityRefsToTrigger();
    public bool GetIsActivated();
    public int GetLevelIndex();
    public Vector2 GetCoords();
    public bool GetIsPressurePlate();
    public void SetIsActivated(bool activatedState);
    public void SetLevelIndex(int _levelIndex);
    public void SetCoords(Vector2 _coords);
    public void SetRequiredKeycardType(string keycardType);
    public void LoadData(SaveableLevelData.InteractableSaveData interactableSaveData);
    public void Destroy();

}
