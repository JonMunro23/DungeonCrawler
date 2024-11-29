using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIController : MonoBehaviour
{
    CharacterData playerCharData;

    [SerializeField] Image playerPortrait, inventoryPlayerPortrait;
    [SerializeField] Slider playerHealthbar, inventoryPlayerHealthbar;
    [SerializeField] TMP_Text healthbarText, inventoryHealthbarText;

    int currentHealth, maxHealth;

    private void OnEnable()
    {
        PlayerHealthController.onCurrentHealthUpdated += OnCurrentHealthUpdated;
        PlayerHealthController.onMaxHealthUpdated += OnMaxHealthUpdated;
        PlayerInventoryManager.onInventoryOpened += OnInventoryOpened;
    }

    private void OnDisable()
    {
        PlayerHealthController.onCurrentHealthUpdated -= OnCurrentHealthUpdated;
        PlayerHealthController.onMaxHealthUpdated -= OnMaxHealthUpdated;
    }

    public void InitStatsUI(CharacterData charData)
    {
        playerCharData = charData;

        SetPlayerPortraitSprite(playerCharData.charPortrait);
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

    public void SetPlayerPortraitSprite(Sprite newSprite)
    {
        playerPortrait.sprite = newSprite;
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
