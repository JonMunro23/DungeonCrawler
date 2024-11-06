using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/New Item")]
public class ItemData : ScriptableObject
{
    [Header("Global Item Properties")]
    public string itemName;
    [TextArea]
    public string itemDescription;
    public Sprite itemSprite;
    public WorldItem itemWorldModel;
    public float itemWeight;
    public float itemValue;
    public bool isItemStackable;
    public int maxItemStackSize = 1;
}

