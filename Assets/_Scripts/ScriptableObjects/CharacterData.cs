using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharData/New Character Data")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public Sprite charPortrait;

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
