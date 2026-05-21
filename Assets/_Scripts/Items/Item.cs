using UnityEngine;

[System.Serializable]
public class Item
{
    [SerializeField] ItemData itemData;
    public ItemData ItemData => itemData;

    public Item (ItemData itemData)
    {
        this.itemData = itemData;
    }
}
