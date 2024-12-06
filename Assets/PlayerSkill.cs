using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSkill : MonoBehaviour, IPointerClickHandler
{
    public PlayerSkillData skillData;

    [SerializeField] Image skillImage;
    [SerializeField] TMP_Text skillNameText;
    [SerializeField] TMP_Text skillDescriptionText;
    [SerializeField] TMP_Text skillLevelText;
    public int currentSkillLevel;
    public int maxSkillLevel = 5;

    bool interactable;

    public static Action<PlayerSkill> onPlayerSkillClicked;
    public static Action<PlayerSkill> onPlayerSkillBought;

    private void Start()
    {
        InitSkill(skillData);
    }

    public void InitSkill(PlayerSkillData newSkillData)
    {
        skillData = newSkillData;

        skillImage.sprite = skillData.skillSprite;
        skillNameText.text = skillData.skillName;
        skillDescriptionText.text = skillData.skillDescription;

        maxSkillLevel = skillData.maxSkillLevel;

        UpdateSkillLevelText();

        SetInteractable(true);
    }

    public void BuySkill()
    {
        currentSkillLevel++;
        if (currentSkillLevel == maxSkillLevel)
        {
            SetInteractable(false);
        }
        UpdateSkillLevelText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GetInteractable())
            return;

        onPlayerSkillClicked?.Invoke(this);
    }

    void UpdateSkillLevelText()
    {
        skillLevelText.text = $"{currentSkillLevel} / {maxSkillLevel}";
    }

    void SetInteractable(bool isInteractable)
    {
        interactable = isInteractable;
    }

    public bool GetInteractable()
    {
        return interactable;
    }
}
