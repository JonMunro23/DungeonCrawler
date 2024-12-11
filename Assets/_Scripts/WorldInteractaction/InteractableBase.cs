using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    int levelIndex;
    Vector2 coords;

    public bool isSingleUse;
    public bool canUse = true;
    public bool isActivated = false;

    public List<ITriggerable> objectsToTrigger = new List<ITriggerable>();
    public List<Dictionary<string, object>> entityRefsToTrigger = new List<Dictionary<string, object>>();

    public abstract void Interact();
    public abstract void InteractWithItem(ItemData item);

    public void AddObjectToTrigger(ITriggerable objectToTrigger)
    {
        if (objectsToTrigger.Contains(objectToTrigger))
            return;

        objectsToTrigger.Add(objectToTrigger);
    }

    public void AddEntityRefToTrigger(Dictionary<string, object> entityRefToTrigger)
    {
        entityRefsToTrigger.Add(entityRefToTrigger);
    }

    public List<string> GetEntityRefsToTrigger()
    {
        List<string> list = new List<string>();

        foreach (var item in entityRefsToTrigger)
        {
            if(item.TryGetValue("entityIid", out object value))
                list.Add(value.ToString());
        }

        return list;
    }

    public void TriggerObjects()
    {
        isActivated = !isActivated;
        foreach (ITriggerable obj in objectsToTrigger)
        {
            obj.Trigger();
        }

        if (isSingleUse)
            canUse = false;
    }

    public abstract void SetIsActivated(bool activatedState);
    public bool GetIsActivated() => isActivated;

    public void SetLevelIndex(int _levelIndex)
    {
        levelIndex = _levelIndex;
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }

    public Vector2 GetCoords()
    {
        return coords;
    }

    public void SetCoords(Vector2 _coords)
    {
        coords = _coords;
    }

    public virtual void SetRequiredKeycardType(string keycardType)
    {
        
    }

    public void LoadData(SaveableLevelData.InteractableSaveData interactableSaveData)
    {
        SetIsActivated(interactableSaveData.isActivated);
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
