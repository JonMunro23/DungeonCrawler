using UnityEngine;

[System.Serializable]
public class ItemStack
{

    [SerializeField] int slotIndex;
    public int SlotIndex => slotIndex;

    [SerializeField] Item item;
    public Item Item => item;

    [SerializeField] int itemAmount = 1;
    public int ItemAmount => itemAmount;

    public ItemStack(Item item, int itemAmount = 1)
    {
        this.item = item;
        this.itemAmount = itemAmount;
    }

    public int GetRemainingSpaceInStack()
    {
        if (item == null || item.ItemData == null)
            return 0;

        return item.ItemData.maxItemStackSize - itemAmount;
    }

    public void SetAmountInStack(int newValue)
    {
        itemAmount = newValue;
    }

    public void SetSlotIndex(int slotIndex) => this.slotIndex = slotIndex;

    public void AddToStack(int amountToAdd)
    {
        itemAmount += amountToAdd;
    }

    public void RemoveFromStack(int amountToRemove)
    {
        itemAmount -= amountToRemove;
    }
}
