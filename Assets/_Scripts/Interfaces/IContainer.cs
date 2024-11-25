public interface IContainer
{
    public void InitContainer();
    public void AddNewStoredItem(ItemStack itemStackToAdd);

    public void OpenContainer();
    public void CloseContainer();
}
