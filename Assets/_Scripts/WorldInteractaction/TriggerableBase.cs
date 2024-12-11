using UnityEngine;

public abstract class TriggerableBase : MonoBehaviour, ITriggerable
{
    int levelIndex;

    public GridNode occupyingGridNode;
    public bool isTriggered;
    public int requiredNumOfTriggers = 1;
    public int currentNumOfTriggers = 0;
    
    public string entityRef = string.Empty;

    public abstract void Trigger();

    public bool GetIsTriggered()
    {
        return isTriggered;
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

    public void SetLevelIndex(int _levelIndex)
    {
        levelIndex = _levelIndex;
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }

    public abstract void SetIsTriggered(bool isTriggered);

    public int GetCurrentNumberOfTriggers()
    {
        return currentNumOfTriggers;
    }

    public void SetCurrentNumberOfTriggers(int newNumberOfTriggers)
    {
        currentNumOfTriggers = newNumberOfTriggers;
    }

    public abstract void LoadData(SaveableLevelData.TriggerableSaveData data);

    public Vector2 GetCoords()
    {
        return occupyingGridNode.Coords.Pos;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
