using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharData/New Character Data")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public Sprite charPortrait;

    [Header("Stats")]
    public int startingMaxHealth = 100;
    public int maxHealthPerLevel = 5;
    [Space]
    public int startingArmour = 0;
    public int armourPerLevel = 0;
    [Space]
    public int startingEvasion = 0;
    public int evasionPerLevel = 0;
    [Space]
    public float startingCritChance = 5;
    public float critChancePerLevel = 0;
    [Space]
    public float startingCritDamage = 2;
    public float critDamagePerLevel = 0;

    public List<StatData> baseCharStats = new List<StatData>();

    public StatData GetStat(ModifiableStats statToGet)
    {
        StatData statToReturn = null;
        foreach (StatData stat in baseCharStats)
        {
            if (stat.stat == statToGet)
                statToReturn = stat;
        }
        return statToReturn;
    }
}
