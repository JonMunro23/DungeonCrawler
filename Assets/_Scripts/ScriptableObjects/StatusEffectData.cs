using UnityEngine;

public enum StatusEffectType
{
    DamageOverTime,
    Debuff
}

[CreateAssetMenu(fileName = "NewStatusEffectData", menuName = "New Status Effect Data")]
public class StatusEffectData : ScriptableObject
{
    [Header("Basic Properties")]
    public StatusEffectType effectType;
    public DamageType damageType;
    public Sprite effectSprite;
    public float effectLength;

    [Header("Damage Properties")]
    public float damage;
    public float damageInterval;

    [Header("Debuff Properties")]
    public int armourReduction;

    [Header("GridNode Properties")]
    public bool canAffectNodes;
    public float nodeEffectLength;
    public float nodeDamage;
    public float nodeDamageInterval;


    
}
