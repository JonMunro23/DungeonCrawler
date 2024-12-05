using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSkillsUIManager : MonoBehaviour
{
    [SerializeField] List<PlayerSkillData> availablePlayerSkills = new List<PlayerSkillData>();
    List<PlayerSkill> spawnedPlayerSkills = new List<PlayerSkill>();
    [SerializeField] PlayerSkill playerSKillPrefab;
    [SerializeField] Transform playerSkillsTransform;
    [SerializeField] GameObject skillsMenuParent;
    [SerializeField] GameObject skillsMenu;

    [SerializeField] TMP_Text availableSkillPointsText;

    private void OnEnable()
    {
        PlayerSkillsController.onSkillPointsUpdated += OnSkillPointsUpdated;
        PlayerSkillsController.onSkillMenuOpened += OpenSkillMenu;
        PlayerSkillsController.onSkillMenuClosed += CloseSkillMenu;
    }

    private void OnDisable()
    {
        PlayerSkillsController.onSkillPointsUpdated -= OnSkillPointsUpdated;
        PlayerSkillsController.onSkillMenuOpened -= OpenSkillMenu;
        PlayerSkillsController.onSkillMenuClosed -= CloseSkillMenu;
    }

    void OnSkillPointsUpdated(int newSkillPointsValue)
    {
        availableSkillPointsText.text = $"Available Skill Points: {newSkillPointsValue}";
    }

    public void Init()
    {
        SpawnPlayerSkills();
    }

    void SpawnPlayerSkills()
    {
        foreach (PlayerSkillData skillData in availablePlayerSkills)
        {
            PlayerSkill clone = Instantiate(playerSKillPrefab, playerSkillsTransform);
            clone.InitSkill(skillData);
            spawnedPlayerSkills.Add(clone);
        }
    }

    void OpenSkillMenu()
    {
        skillsMenuParent.SetActive(true);
        skillsMenu.SetActive(true);
    }

    void CloseSkillMenu()
    {
        skillsMenuParent.SetActive(false);
        skillsMenu.SetActive(false);
    }
}
