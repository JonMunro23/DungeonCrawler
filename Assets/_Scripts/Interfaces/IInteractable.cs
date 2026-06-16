using System.Collections.Generic;
using UnityEngine;

public interface IInteractable : IHighlightable, IGridNode
{
    public bool CanUse {  get; }

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
    public TriggerOperation GetTriggerOperation();
    public void SetIsSingleUse(bool isSingleUse);
    public void LoadData(SaveableLevelData.InteractableSaveData interactableSaveData);
    public void Destroy();
    public GameObject GetGameObject();
}
