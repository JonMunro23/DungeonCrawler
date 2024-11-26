public interface IContainer
{
    public void InitContainer();
    public void AddNewStoredItem(ItemStack itemStackToAdd);
    public void ToggleContainer();
    public void CloseContainer();
}
