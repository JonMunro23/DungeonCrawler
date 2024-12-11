using UnityEngine;

public interface ITriggerable
{
    public void LoadData(SaveableLevelData.TriggerableSaveData data);
    public void Trigger();


    public string GetEntityRef();
    public Vector2 GetCoords();
    public int GetCurrentNumberOfTriggers();
    public bool GetIsTriggered();
    public int GetLevelIndex();


    public void SetEntityRef(string entityRefToSet);
    public void SetOccupyingNode(GridNode occupyingNode);
    public void SetCurrentNumberOfTriggers(int newNumberOfInputs);
    public void SetIsTriggered(bool isTriggered);
    public void SetLevelIndex(int _levelIndex);
    public void Destroy();

}
