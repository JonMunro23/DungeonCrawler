using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerSkillsUIManager : MonoBehaviour
{

    [SerializeField] Transform playerSkillsTransform;
    [SerializeField] GameObject skillsMenuParent;
    [SerializeField] GameObject skillsMenu;

    [SerializeField] TMP_Text availableSkillPointsText;

    private void OnEnable()
    {
        PlayerSkillsManager.onPlayerSkillsSpawned += OnPlayerSkillsSpawned;
        PlayerSkillsManager.onSkillPointsUpdated += OnSkillPointsUpdated;
        PlayerSkillsManager.onSkillMenuOpened += OpenSkillMenu;
        PlayerSkillsManager.onSkillMenuClosed += CloseSkillMenu;
    }

    private void OnDisable()
    {
        PlayerSkillsManager.onPlayerSkillsSpawned -= OnPlayerSkillsSpawned;
        PlayerSkillsManager.onSkillPointsUpdated -= OnSkillPointsUpdated;
        PlayerSkillsManager.onSkillMenuOpened -= OpenSkillMenu;
        PlayerSkillsManager.onSkillMenuClosed -= CloseSkillMenu;
    }

    void OnSkillPointsUpdated(int newSkillPointsValue)
    {
        availableSkillPointsText.text = $"Available Skill Points: {newSkillPointsValue}";
    }

    void OnPlayerSkillsSpawned(List<PlayerSkill> spawnedSkill)
    {
        foreach (PlayerSkill skill in spawnedSkill)
        {
            skill.transform.SetParent(playerSkillsTransform, false);
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
