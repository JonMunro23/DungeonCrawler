using System.Collections.Generic;
using UnityEngine;

public class InteractableBase : MonoBehaviour, IInteractable
{
    public bool isSingleUse;
    public bool canUse = true;
    public bool isActivated = false;

    public List<ITriggerable> objectsToTrigger = new List<ITriggerable>();
    public List<Dictionary<string, object>> entityRefsToTrigger = new List<Dictionary<string, object>>();

    public virtual void Interact()
    {
    }
    public virtual void InteractWithItem(ItemData item)
    {
    }

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
}
