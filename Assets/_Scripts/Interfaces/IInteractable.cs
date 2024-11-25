using System.Collections.Generic;

public interface IInteractable
{
    public void Interact();  
    public void InteractWithItem(ItemData item);
    public void AddObjectToTrigger(ITriggerable objectToTrigger);
    public void AddEntityRefToTrigger(Dictionary<string, object> entityRefToTrigger);
    public List<string> GetEntityRefsToTrigger();
}
