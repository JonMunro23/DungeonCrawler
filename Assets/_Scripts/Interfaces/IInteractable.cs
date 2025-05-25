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
    public void SetInteractableType(string interactableType);
    public InteractableType GetInteractableType();
    public void SetIsActivated(bool activatedState);
    public void SetLevelIndex(int _levelIndex);
    public void SetNode(GridNode spawnNode);
    public void SetRequiredKeycardType(string keycardType);
    public void SetTriggerOperation(string triggerOperation);

    /// <summary>
    /// Sets wether a pressure plate will be triggered when it is no longer pressed
    /// </summary>
    /// <param name="triggerOnExit"></param>
    public void SetTriggerOnExit(bool triggerOnExit);
    public TriggerOperation GetTriggerOperation();

    /// <summary>
    /// Get wether a pressure plater will be triggered when it is no longer pressed
    /// </summary>
    /// <returns></returns>
    public bool GetTriggerOnExit();
    public void SetIsSingleUse(bool isSingleUse);
    public void LoadData(SaveableLevelData.InteractableSaveData interactableSaveData);
    public void Destroy();
    public GameObject GetGameObject();
    public void SetHighlighted(bool isHighlighted);

}
