using System.Collections.Generic;
using UnityEngine;

public interface IInteractable : IHighlightable, IGridNode
{
    public void Interact();  
    public bool TryInteractWithItem(ItemData item);
    public bool ConsumesItem();
    public bool RequiresItem();
    public void AddObjectToTrigger(ITriggerable objectToTrigger);
    public void AddEntityRefToTrigger(Dictionary<string, object> entityRefToTrigger);
    public List<string> GetEntityRefsToTrigger();
    public bool GetIsActivated();
    public void SetInteractableType(string interactableType);
    public InteractableType GetInteractableType();
    public void SetStartingActivationState(bool activatedState);
    public void SetTriggerOperation(string triggerOperation);

    /// <summary>
    /// Sets wether a pressure plate will be triggered when it is no longer pressed
    /// </summary>
    /// <param name="triggerOnExit"></param>
    //public void SetTriggerOnExit(bool triggerOnExit);
    public TriggerOperation GetTriggerOperation();

    /// <summary>
    /// Get wether a pressure plater will be triggered when it is no longer pressed
    /// </summary>
    /// <returns></returns>
    //public bool GetTriggerOnExit();
    public void SetIsSingleUse(bool isSingleUse);
    public void LoadData(SaveableLevelData.InteractableSaveData interactableSaveData);
    public void Destroy();
    public GameObject GetGameObject();
}
