using System.Collections.Generic;
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
    [Tooltip("The sound effect to play whilst affected by this status effect")]
    public AudioClip effectSFX;
    public float effectLength;

    [Header("Damage Properties")]
    public float damage;
    public float damageInterval;

    [Header("Node Properties")]
    public bool canAffectNodes;
    public float nodeEffectLength;
    public float nodeDamage;
    public float nodeDamageInterval;

    [Header("Debuff Properties")]
    public List<StatModifier> statsToDebuff;


    
}
