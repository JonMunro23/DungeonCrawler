using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSkillData", menuName = "Player Skills/New Player Skill")]
public class PlayerSkillData : ScriptableObject
{
    [Header("Global Item Properties")]
    public string skillName;
    [TextArea(2, 4)]
    public string skillDescription;
    public Sprite skillSprite;

    public int maxSkillLevel;
}

