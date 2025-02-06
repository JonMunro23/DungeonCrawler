using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharData/New Character Data")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public Sprite classIcon;
    public string className;
    [TextArea(3, 5)]
    public string charDescription;

    [Space]
    public List<StatData> baseCharStats = new List<StatData>();
    public List<PlayerSkillData> classSpecificSkills = new List<PlayerSkillData>();

    public StatData GetStat(ModifiableCharacterStats statToGet)
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
