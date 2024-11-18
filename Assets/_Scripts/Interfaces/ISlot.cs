public interface ISlot
{
    public void AddItem(ItemStack itemStackToAdd);
    public ItemStack TakeItem();
    public void RemoveItem();
    public void AddToExistingStack(int amountToAdd);
    public ItemStack SwapItem(ItemStack itemStackToSwapTo);
    public ItemStack GetItemStack();
    public void SetInteractable(bool isInteractable);
    public bool IsInteractable();
    public bool IsSlotEmpty();
}
