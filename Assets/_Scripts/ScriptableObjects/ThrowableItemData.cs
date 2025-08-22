using UnityEngine;

//public enum ThrowableType
//{
//    Grenade,
//    RemoteExplosive
//}

[CreateAssetMenu(fileName = "ThrowableItem", menuName = "Items/New Throwable Item")]
public class ThrowableItemData : ItemData
{
    [Header("Throwable Item Properties")]
    public ThrowableArms throwableArmsPrefab;
    public Throwable throwablePrefab;
    public float blastRadius = 1f;
    public float throwVelocity;

    public float cooldownBetweenUses;

    [Header("Animation Lengths")]
    public float throwDelay;
}

