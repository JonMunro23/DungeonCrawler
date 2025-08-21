using System.Collections.Generic;
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
    public float blastRadius = 1f;
    public float throwVelocity;

    public float cooldownBetweenUses;

    //[Header("Animation Lengths")]
    //public float useAnimationLength;
}

