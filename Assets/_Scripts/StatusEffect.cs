using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "New Status Effect")]
public class StatusEffect : ScriptableObject
{
    [Header("Basic Properties")]
    public StatusEffectType effectType;
    public DamageType damageType;
    public Sprite effectSprite;
    public float effectLength;
    public bool dealsDOT;
    public float damage;
    public float damageInterval;

    [Header("GridNode Properties")]
    public bool canAffectNodes;
    public float nodeEffectLength;
    public float nodeDamage;
    public float nodeDamageInterval;

    [Header("Special Properties")]
    public int armourReduction;

    
}
