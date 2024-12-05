using TMPro;
using UnityEngine;

public class PlayerSkillsUIManager : MonoBehaviour
{
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
