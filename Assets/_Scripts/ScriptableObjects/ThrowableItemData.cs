using UnityEngine;

public enum DetonationType
{
    None,
    Timed,
    Contact,
    Remote,
    Proximity
}

[CreateAssetMenu(fileName = "ThrowableItem", menuName = "Items/New Throwable Item")]
public class ThrowableItemData : ItemData
{
    [Header("Throwable Item Properties")]
    public ThrowableArms throwableArmsPrefab;
    public Throwable throwablePrefab;
    public bool isExplosive;
    public DetonationType detonationType;
    public float fuseLength;
    public float blastRadius;
    public float proximityDetectionRadius;
    public DamageType damageType;
    public int damage;
    public StatusEffectType inflictedStatusEffect;
    public float statusEffectLength;
    public float minThrowVelocity;
    public float maxThrowVelocity;
    public float timeToMaxVelocity;

    public ParticleSystem explosionVFX;
    public AudioClip explosionSFX;
    public AudioClip bounceSFX;

    [Header("Animation Lengths")]
    public float readyDelay;
    public float throwDelay;
    public float holsterLength;
}

