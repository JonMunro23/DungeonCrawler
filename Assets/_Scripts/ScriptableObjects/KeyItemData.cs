using UnityEngine;

public enum KeyType
{
    Key,
    Keycard,
}

[CreateAssetMenu(fileName = "KeyItem", menuName = "Items/New Key Item")]
public class KeyItemData : ItemData
{
    [Header("Key Item Properties")]
    public KeyType keyType;
    public KeycardType keycardType;
}

