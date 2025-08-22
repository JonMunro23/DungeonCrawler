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
    public float fuseLength = 3f;
    public float blastRadius = 1f;
    public int damage;
    public float minThrowVelocity;
    public float maxThrowVelocity;
    public float timeToMaxVelocity;

    public ParticleSystem explosionVFX;
    public AudioClip explosionSFX;
    public AudioClip bounceSFX;

    [Header("Animation Lengths")]
    public float throwDelay;
}

