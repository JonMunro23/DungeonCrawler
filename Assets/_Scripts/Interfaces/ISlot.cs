public interface ISlot
{
    public void AddItem(ItemStack itemStackToAdd);
    public ItemStack TakeItem();
    public void RemoveItemStack();
    public int AddToCurrentItemStack(int amountToAdd);
    public int RemoveFromExistingStack(int amountToRemove);
    public ItemStack SwapItem(ItemStack itemStackToSwapTo);
    public ItemStack GetItemStack();
    public void SetInteractable(bool isInteractable);
    public bool IsInteractable();
    public bool IsSlotEmpty();
    public int GetSlotIndex();
}
