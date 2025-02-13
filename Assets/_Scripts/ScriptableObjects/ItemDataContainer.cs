using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ItemDataContainer", menuName = "Items/New ItemData Container")]

public class ItemDataContainer : ScriptableObject
{
    public List<ItemData> itemData = new List<ItemData>();

    public ItemData GetDataFromIdentifier(string identifier)
    {
        foreach (ItemData item in itemData)
        {
            if(item.itemIdentifier == identifier)
            {
                return item;
            }
        }
        Debug.Log($"Cant find {identifier}");
        return null;
    }
}
