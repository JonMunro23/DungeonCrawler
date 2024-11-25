using UnityEngine;

public class TriggerableBase : MonoBehaviour, ITriggerable
{
    public GridNode occupyingGridNode;
    public bool isTriggerable;
    public int requiredNumOfTriggers = 1;
    public int currentNumOfTriggers = 0;
    
    public string entityRef = string.Empty;

    public virtual void Trigger()
    {
        
    }

    public bool IsTriggerable()
    {
        return isTriggerable;
    }
    public void SetRequiredNumberOfTriggers(int requiredNum)
    {
        requiredNumOfTriggers = requiredNum;
    }
    public void SetEntityRef(string entityRefToSet)
    {
        entityRef = entityRefToSet;
    }

    public string GetEntityRef()
    {
        return entityRef;
    }

    public void SetOccupyingNode(GridNode occupyingNode)
    {
        occupyingGridNode = occupyingNode;
    }
}
