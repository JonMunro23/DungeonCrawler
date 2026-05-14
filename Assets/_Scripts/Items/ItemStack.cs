using UnityEngine;

//[System.Serializable]
public class ItemStack
{
    Item item;
    public Item Item => item;

    int itemAmount = 1;
    public int ItemAmount => itemAmount;

    public ItemStack(Item item, int itemAmount = 1)
    {
        this.item = item;
        this.itemAmount = itemAmount;
    }

    public int GetRemainingSpaceInStack()
    {
        return Item.ItemData.maxItemStackSize - itemAmount;
    }

    public void SetAmountInStack(int newValue)
    {
        itemAmount = newValue;
    }

    public void AddToStack(int amountToAdd)
    {
        itemAmount += amountToAdd;
    }

    public void RemoveFromStack(int amountToRemove)
    {
        itemAmount -= amountToRemove;
    }
}
