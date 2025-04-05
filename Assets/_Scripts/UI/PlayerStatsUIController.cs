using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{
    //CharacterData playerCharData;
    PlayerController playerController;
    PlayerStatsManager playerStatsManager;

    [SerializeField] GameObject statsMenu;

    [SerializeField] Image inventoryPlayerPortrait;
    [SerializeField] Slider playerHealthbar, inventoryPlayerHealthbar;
    [SerializeField] Slider playerExperienceBar, inventoryPlayerExperienceBar;
    [SerializeField] TMP_Text healthbarText, inventoryHealthbarText;
    [SerializeField] TMP_Text experienceBarText, inventoryExperienceBarText;

    [Header("Stats Menu References")]
    [SerializeField] TMP_Text statsMenuHealthText;
    [SerializeField] TMP_Text statsMenuExperienceText, 
                                statsMenuArmourRatingText, 
                                statsMenuEvasionRatingText;

    [SerializeField] TMP_Text statsMenuWeaponSlot1NameText, 
                                statsMenuWeaponSlot1WeaponTypeText, 
                                statsMenuWeaponSlot1DamageText,
                                statsMenuWeaponSlot1RangeText,
                                statsMenuWeaponSlot1ShotPerBurstText,
                                statsMenuWeaponSlot1ProjectilesPerShotText,
                                statsMenuWeaponSlot1CooldownText,
                                statsMenuWeaponSlot1MagSizeText,
                                statsMenuWeaponSlot1ReloadLengthText;

    [SerializeField] TMP_Text statsMenuWeaponSlot2NameText,
                                statsMenuWeaponSlot2WeaponTypeText,
                                statsMenuWeaponSlot2DamageText,
                                statsMenuWeaponSlot2RangeText,
                                statsMenuWeaponSlot2ShotPerBurstText,
                                statsMenuWeaponSlot2ProjectilesPerShotText,
                                statsMenuWeaponSlot2CooldownText,
                                statsMenuWeaponSlot2MagSizeText,
                                statsMenuWeaponSlot2ReloadLengthText;

    [SerializeField] GameObject[] Slot1RangedWeaponExclusiveStats, Slot2RangedWeaponExclusiveStats;

    int currentHealth, maxHealth;
    int currentExperience, requiredExperience;

    private void OnEnable()
    {
        PlayerHealthManager.onCurrentHealthUpdated += OnCurrentHealthUpdated;
        PlayerHealthManager.onMaxHealthUpdated += OnMaxHealthUpdated;
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;

        PlayerLevelManager.onPlayerExperienceUpdated += OnPlayerExperienceUpdated;
        PlayerLevelManager.onPlayerRequiredExperienceUpdated += OnPlayerRequiredExperienceUpdated;
    }

    private void OnDisable()
    {
        PlayerHealthManager.onCurrentHealthUpdated -= OnCurrentHealthUpdated;
        PlayerHealthManager.onMaxHealthUpdated -= OnMaxHealthUpdated;
        PlayerInventoryManager.onInventoryOpened -= OnInventoryOpened;

        PlayerLevelManager.onPlayerExperienceUpdated -= OnPlayerExperienceUpdated;
        PlayerLevelManager.onPlayerRequiredExperienceUpdated -= OnPlayerRequiredExperienceUpdated;
    }

    public void InitStatsUI(PlayerController initialisedPlayerController)
    {
        playerController = initialisedPlayerController;
        this.playerStatsManager = playerController.playerStatsManager;

        //UpdateMaxHealthValue(playerCh arData.GetStat(ModifiableCharacterStats.MaxHealth).GetBaseStatValue());
        UpdateMaxHealthValue(playerStatsManager.GetPlayerStat(ModifiableCharacterStats.MaxHealth).GetCurrentStatValue());
        UpdateCurrentHealthValue(maxHealth);
    }

    void OnInventoryOpened()
    {
        inventoryPlayerHealthbar.value = currentHealth;
        inventoryPlayerPortrait.sprite = playerStatsManager.playerCharData.classIcon;
    }

    void OnCurrentHealthUpdated(CharacterData charData, float newAmount)
    {
        //if(charData ==  playerCharData)
        //{
            currentHealth = Mathf.CeilToInt(newAmount);
            UpdateCurrentHealthValue(Mathf.CeilToInt(currentHealth));
        //}
    }

    void OnMaxHealthUpdated(CharacterData charData, float newAmount)
    {
        //if (charData == playerCharData)
        //{
            maxHealth = Mathf.CeilToInt(newAmount);
            UpdateMaxHealthValue(maxHealth);
        //}
    }

    void OnPlayerExperienceUpdated(int newExperienceValue)
    {
        currentExperience = newExperienceValue;
        playerExperienceBar.value = currentExperience;
        UpdateExperienceText();
    }

    void OnPlayerRequiredExperienceUpdated(int newRequiredExperienceValue)
    {
        playerExperienceBar.minValue = requiredExperience;
        requiredExperience = newRequiredExperienceValue;
        playerExperienceBar.maxValue = requiredExperience;
        playerExperienceBar.value = currentExperience;
        UpdateExperienceText();
    }


    public void OpenStatsMenu()
    {
        statsMenu.SetActive(true);
        InitStatsMenu();

        HelperFunctions.SetCursorActive(true);
    }

    public void CloseStatsMenu()
    {
        statsMenu.SetActive(false);
    }

    void InitStatsMenu()
    {
        //CHARACTER STATS
        statsMenuHealthText.text = $"{currentHealth} / {maxHealth}";
        statsMenuExperienceText.text = $"{currentExperience} / {requiredExperience}";
        statsMenuArmourRatingText.text = $"{playerStatsManager.GetPlayerStat(ModifiableCharacterStats.Armour).GetCurrentStatValue()}";
        statsMenuEvasionRatingText.text = $"{playerStatsManager.GetPlayerStat(ModifiableCharacterStats.Evasion).GetCurrentStatValue()}";

        //WEAPON SLOT 1 STATS
        IWeapon slot1Weapon = playerController.playerWeaponManager.spawnedWeaponSlots[0].GetWeapon();
        WeaponItemData weapon1Data = slot1Weapon.GetWeaponData();

        if(slot1Weapon.GetRangedWeapon() == null)
        {
            foreach (GameObject exclusiveStat in Slot1RangedWeaponExclusiveStats)
            {
                exclusiveStat.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject exclusiveStat in Slot1RangedWeaponExclusiveStats)
            {
                exclusiveStat.SetActive(true);
            }
        }

        statsMenuWeaponSlot1NameText.text = $"{weapon1Data.itemName}";
        statsMenuWeaponSlot1WeaponTypeText.text = $"{weapon1Data.weaponType}";
        statsMenuWeaponSlot1DamageText.text = $"{slot1Weapon.GetWeaponDamageRange().x} - {slot1Weapon.GetWeaponDamageRange().y}";
        statsMenuWeaponSlot1RangeText.text = $"{weapon1Data.itemRange}";
        statsMenuWeaponSlot1ShotPerBurstText.text = $"{(slot1Weapon.GetRangedWeapon() != null ? slot1Weapon.GetRangedWeapon().GetBurstCount() : 0)}";
        statsMenuWeaponSlot1ProjectilesPerShotText.text = $"{weapon1Data.projectileCount}";
        statsMenuWeaponSlot1CooldownText.text = $"{weapon1Data.itemCooldown}";
        statsMenuWeaponSlot1MagSizeText.text = $"{weapon1Data.magSize}";
        statsMenuWeaponSlot1ReloadLengthText.text = $"{weapon1Data.reloadAnimDuration}";


        //WEAPON SLOT 2 STATS
        IWeapon slot2Weapon = playerController.playerWeaponManager.spawnedWeaponSlots[1].GetWeapon();
        WeaponItemData weapon2Data = slot2Weapon.GetWeaponData();

        if (slot2Weapon.GetRangedWeapon() == null)
        {
            foreach (GameObject exclusiveStat in Slot2RangedWeaponExclusiveStats)
            {
                exclusiveStat.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject exclusiveStat in Slot2RangedWeaponExclusiveStats)
            {
                exclusiveStat.SetActive(true);
            }
        }

        statsMenuWeaponSlot2NameText.text = $"{weapon2Data.itemName}";
        statsMenuWeaponSlot2WeaponTypeText.text = $"{weapon2Data.weaponType}";
        statsMenuWeaponSlot2DamageText.text = $"{slot2Weapon.GetWeaponDamageRange().x} - {slot2Weapon.GetWeaponDamageRange().y}";
        statsMenuWeaponSlot2RangeText.text = $"{weapon2Data.itemRange}";
        statsMenuWeaponSlot2ShotPerBurstText.text = $"{(slot2Weapon.GetRangedWeapon() != null ? slot2Weapon.GetRangedWeapon().GetBurstCount() : 0)}";
        statsMenuWeaponSlot2ProjectilesPerShotText.text = $"{weapon2Data.projectileCount}";
        statsMenuWeaponSlot2CooldownText.text = $"{weapon2Data.itemCooldown}";
        statsMenuWeaponSlot2MagSizeText.text = $"{weapon2Data.magSize}";
        statsMenuWeaponSlot2ReloadLengthText.text = $"{weapon2Data.reloadAnimDuration}";
    }

    private void UpdateExperienceText()
    {
        experienceBarText.text = $"{currentExperience} / {requiredExperience}";
        inventoryExperienceBarText.text = $"{currentExperience} / {requiredExperience}";
    }

    public void UpdateCurrentHealthValue(float newCurrentHealthValue)
    {
        currentHealth = Mathf.CeilToInt(newCurrentHealthValue);
        playerHealthbar.value = currentHealth;
        UpdateHealthbarText();
    }

    public void UpdateMaxHealthValue(float newMaxHealthValue)
    {
        maxHealth = Mathf.CeilToInt(newMaxHealthValue);
        playerHealthbar.maxValue = maxHealth;
        inventoryPlayerHealthbar.maxValue = maxHealth;
        UpdateHealthbarText();
    }

    void UpdateHealthbarText()
    {
        healthbarText.text = $"{currentHealth} / {maxHealth}";
        inventoryHealthbarText.text = $"{currentHealth} / {maxHealth}";
    }
}
