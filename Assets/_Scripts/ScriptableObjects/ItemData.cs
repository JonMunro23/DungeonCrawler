using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/New Item")]
public class ItemData : ScriptableObject
{
    [Header("Global Item Properties")]
    public string itemName;
    public string itemIdentifier;
    [TextArea]
    public string itemDescription;
    public Sprite itemSprite;
    public GameObject itemWorldModel;
    public float itemWeight;
    public bool isItemStackable;
    public int maxItemStackSize = 1;
}

