using UnityEngine;

[CreateAssetMenu(fileName = "KeycardItem", menuName = "Items/New Keycard Item")]
public class KeycardItemData : ItemData
{
    [Header("Key Item Properties")]
    public KeycardType keycardType;
}

