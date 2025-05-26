public interface IPickup : IHighlightable
{
    public void Pickup(bool wasGrabbed = false);
    public void AddToInventory(IInventory inventoryToAddTo);
}
