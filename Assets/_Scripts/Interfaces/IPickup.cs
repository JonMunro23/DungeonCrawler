public interface IPickup
{
    public void Pickup(bool wasGrabbed = false);
    public void SetHighlighted(bool isHighlighted);
    public void AddToInventory(IInventory inventoryToAddTo);
}
