using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{
    CharacterData playerCharData;

    [SerializeField] Image playerPortrait, inventoryPlayerPortrait;
    [SerializeField] Slider playerHealthbar, inventoryPlayerHealthbar;
    [SerializeField] TMP_Text healthbarText, inventoryHealthbarText;

    float currentHealth, maxHealth;

    private void OnEnable()
    {
        HealthController.onCurrentHealthUpdated += OnCurrentHealthUpdated;
        HealthController.onMaxHealthUpdated += OnMaxHealthUpdated;
        PlayerInventory.onInventoryOpened += OnInventoryOpened;
    }

    private void OnDisable()
    {
        HealthController.onCurrentHealthUpdated -= OnCurrentHealthUpdated;
        HealthController.onMaxHealthUpdated -= OnMaxHealthUpdated;
    }

    public void InitStatsUI(CharacterData charData)
    {
        playerCharData = charData;

        SetPlayerPortraitSprite(playerCharData.charPortrait);
        UpdateMaxHealthValue(playerCharData.maxHealth);
        UpdateCurrentHealthValue(playerCharData.maxHealth);
    }

    void OnInventoryOpened()
    {
        inventoryPlayerHealthbar.maxValue = maxHealth;
        inventoryPlayerHealthbar.value = currentHealth;

        inventoryHealthbarText.text = $"{currentHealth} / {maxHealth}";

        inventoryPlayerPortrait.sprite = playerCharData.charPortrait;
    }

    void OnCurrentHealthUpdated(CharacterData charData, int newAmount)
    {
        if(charData ==  playerCharData)
        {
            currentHealth = newAmount;
            UpdateCurrentHealthValue(currentHealth);
        }
    }

    void OnMaxHealthUpdated(CharacterData charData, int newAmount)
    {
        if (charData == playerCharData)
        {
            maxHealth = newAmount;
            UpdateMaxHealthValue(maxHealth);
        }
    }

    public void SetPlayerPortraitSprite(Sprite newSprite)
    {
        playerPortrait.sprite = newSprite;
    }

    public void UpdateCurrentHealthValue(float newCurrentHealthValue)
    {
        currentHealth = newCurrentHealthValue;
        playerHealthbar.value = currentHealth;
        UpdateHealthbarText();
    }

    public void UpdateMaxHealthValue(float newMaxHealthValue)
    {
        maxHealth = newMaxHealthValue;
        playerHealthbar.maxValue = maxHealth;
        UpdateHealthbarText();
    }

    void UpdateHealthbarText() => healthbarText.text = $"{currentHealth} / {maxHealth}";
}
