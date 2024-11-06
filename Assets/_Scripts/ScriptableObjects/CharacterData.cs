using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharData/New Character Data")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public Sprite charPortrait;

    public List<Stat> baseCharStats = new List<Stat>();

    public Stat GetStat(ModifiableStats statToGet)
    {
        Stat statToReturn = null;
        foreach (Stat stat in baseCharStats)
        {
            if (stat.stat == statToGet)
                statToReturn = stat;
        }
        return statToReturn;
    }
}
