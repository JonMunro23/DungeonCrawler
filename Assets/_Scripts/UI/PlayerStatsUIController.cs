using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{
    CharacterData playerCharData;

    [SerializeField] Image inventoryPlayerPortrait;
    [SerializeField] Slider playerHealthbar, inventoryPlayerHealthbar;
    [SerializeField] Slider playerExperienceBar, inventoryPlayerExperienceBar;
    [SerializeField] TMP_Text healthbarText, inventoryHealthbarText;
    [SerializeField] TMP_Text experienceBarText, inventoryExperienceBarText;

    int currentHealth, maxHealth;
    int currentExperience, requiredExperience;

    private void OnEnable()
    {
        PlayerHealthManager.onCurrentHealthUpdated += OnCurrentHealthUpdated;
        PlayerHealthManager.onMaxHealthUpdated += OnMaxHealthUpdated;
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;

        PlayerLevelController.onPlayerExperienceUpdated += OnPlayerExperienceUpdated;
        PlayerLevelController.onPlayerRequiredExperienceUpdated += OnPlayerRequiredExperienceUpdated;
    }

    private void OnDisable()
    {
        PlayerHealthManager.onCurrentHealthUpdated -= OnCurrentHealthUpdated;
        PlayerHealthManager.onMaxHealthUpdated -= OnMaxHealthUpdated;
        PlayerInventoryManager.onInventoryOpened -= OnInventoryOpened;

        PlayerLevelController.onPlayerExperienceUpdated -= OnPlayerExperienceUpdated;
        PlayerLevelController.onPlayerRequiredExperienceUpdated -= OnPlayerRequiredExperienceUpdated;
    }

    public void InitStatsUI(CharacterData charData)
    {
        playerCharData = charData;

        UpdateMaxHealthValue(playerCharData.GetStat(ModifiableStats.MaxHealth).GetCurrentStatValue());
        UpdateCurrentHealthValue(maxHealth);
    }

    void OnInventoryOpened()
    {
        inventoryPlayerHealthbar.value = currentHealth;
        inventoryPlayerPortrait.sprite = playerCharData.charPortrait;
    }

    void OnCurrentHealthUpdated(CharacterData charData, float newAmount)
    {
        if(charData ==  playerCharData)
        {
            currentHealth = Mathf.CeilToInt(newAmount);
            UpdateCurrentHealthValue(Mathf.CeilToInt(currentHealth));
        }
    }

    void OnMaxHealthUpdated(CharacterData charData, float newAmount)
    {
        if (charData == playerCharData)
        {
            maxHealth = Mathf.CeilToInt(newAmount);
            UpdateMaxHealthValue(maxHealth);
        }
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
