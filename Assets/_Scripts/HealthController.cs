using UnityEngine;
using System;

public class HealthController : MonoBehaviour, IDamageable
{
    CharacterData characterData;

    [SerializeField] int currentHealth, maxHealth;

    public static Action<CharacterData, int> onMaxHealthUpdated;
    public static Action<CharacterData, int> onCurrentHealthUpdated;

    private void OnEnable()
    {
        Stat.onStatUpdated += OnStatUpdated;
    }

    private void OnDisable()
    {
        Stat.onStatUpdated -= OnStatUpdated;
    }

    void OnStatUpdated(Stat updatedStat)
    {
        if(updatedStat.stat == ModifiableStats.MaxHealth)
        {
            UpdateMaxHealth(updatedStat.GetCurrentStatValue());
        }
    }

    void UpdateMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.CeilToInt(newMaxHealth);
        onMaxHealthUpdated?.Invoke(characterData, maxHealth);
    }

    public void InitHealthController(CharacterData charData)
    {
        characterData = charData;

        maxHealth = Mathf.CeilToInt(characterData.GetStat(ModifiableStats.MaxHealth).baseStatValue);
        onMaxHealthUpdated?.Invoke(characterData, maxHealth);

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageTaken, bool wasCrit = false)
    {
        int damageToTake = wasCrit ? damageTaken * 2 : damageTaken;

        currentHealth -= damageToTake;
        onCurrentHealthUpdated?.Invoke(characterData, currentHealth);
    }

    public void Heal(int healAmount)
    {
        if(currentHealth < maxHealth)
        {
            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            onCurrentHealthUpdated.Invoke(characterData, currentHealth);
        }
    }
}
