public interface ITriggerable
{
    public void Trigger();
    public void SetEntityRef(string entityRefToSet);
    public string GetEntityRef();
    public void SetOccupyingNode(GridNode occupyingNode);
    public int GetCurrentNumberOfTriggers();
    public void SetCurrentNumberOfTriggers(int newNumberOfInputs);
    public bool GetIsTriggered();
    public void SetIsTriggered(bool isTriggered);
    public void SetLevelIndex(int _levelIndex);
    public int GetLevelIndex();
}
