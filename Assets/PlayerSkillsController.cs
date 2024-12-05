using System;
using UnityEngine;

public class PlayerSkillsController : MonoBehaviour
{
    PlayerInventoryManager inventoryManager;

    public int availableSkillPoints;
    bool isSkillMenuOpen;
    public static Action<int> onSkillPointsUpdated;

    public static Action onSkillMenuOpened;
    public static Action onSkillMenuClosed;

    private void Awake()
    {
        inventoryManager = GetComponent<PlayerInventoryManager>();
    }

    private void OnEnable()
    {
        PlayerLevelController.onPlayerLevelUp += OnPlayerLevelUp;
        PlayerSkill.onPlayerSkillClicked += OnPlayerSkillClicked;
    }

    private void OnDisable()
    {
        PlayerLevelController.onPlayerLevelUp -= OnPlayerLevelUp;
        PlayerSkill.onPlayerSkillClicked -= OnPlayerSkillClicked;
    }

    void OnPlayerSkillClicked(PlayerSkill skillClicked)
    {
        if(availableSkillPoints > 0)
        {
            skillClicked.BuySkill();
            RemoveSkillPoint();
        }
    }
    void OnPlayerLevelUp(int playerLevel)
    {
        AddSkillPoint();
    }

    public void ToggleSkillsMenu()
    {
        if (isSkillMenuOpen == true)
        {
            CloseSkillsMenu();
        }
        else if (isSkillMenuOpen == false)
        {
            OpenSkillsMenu();
        }
    }

    private void OpenSkillsMenu()
    {
        isSkillMenuOpen = true;
        inventoryManager.SetCursorActive(true);
        onSkillMenuOpened?.Invoke();
    }

    public void CloseSkillsMenu()
    {
        isSkillMenuOpen = false;
        inventoryManager.SetCursorActive(false);
        onSkillMenuClosed?.Invoke();
    }


    public void AddSkillPoint()
    {
        availableSkillPoints++;
        onSkillPointsUpdated?.Invoke(availableSkillPoints);
    }

    public void RemoveSkillPoint()
    {
        availableSkillPoints--;
        onSkillPointsUpdated?.Invoke(availableSkillPoints);
    }
}
