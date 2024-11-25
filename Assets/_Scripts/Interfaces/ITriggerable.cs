using System.Collections.Generic;

public interface ITriggerable
{
    public void Trigger();
    public bool IsTriggerable();
    public void SetEntityRef(string entityRefToSet);
    public string GetEntityRef();
    public void SetOccupyingNode(GridNode occupyingNode);
}
