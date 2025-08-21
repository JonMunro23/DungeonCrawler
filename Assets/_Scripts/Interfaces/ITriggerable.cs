using UnityEngine;

public interface ITriggerable : IGridNode
{
    public void LoadData(SaveableLevelData.TriggerableSaveData data);
    public void Trigger(IInteractable triggeredInteractable);
    public string GetEntityRef();
    public int GetCurrentNumberOfTriggers();
    public bool GetIsTriggered();
    public void SetEntityRef(string entityRefToSet);
    public void SetCurrentNumberOfTriggers(int newNumberOfInputs);
    public void SetIsTriggered(bool isTriggered);
    public void Destroy();

}
