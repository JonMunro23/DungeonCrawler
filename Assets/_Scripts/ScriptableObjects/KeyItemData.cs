using UnityEngine;

public enum KeyType
{
    Old,
    Rusty
}

[CreateAssetMenu(fileName = "KeyItem", menuName = "Items/New Key Item")]
public class KeyItemData : ItemData
{
    [Header("Key Item Properties")]
    public KeyType keyType;
}

