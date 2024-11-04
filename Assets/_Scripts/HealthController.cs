using UnityEngine;
using System;

public class HealthController : MonoBehaviour, IDamageable
{
    CharacterData characterData;

    int currentHealth, maxHealth;

    public static Action<CharacterData, int> onMaxHealthUpdated;
    public static Action<CharacterData, int> onCurrentHealthUpdated;

    public void InitHealthController(CharacterData charData)
    {
        characterData = charData;

        maxHealth = characterData.maxHealth;
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
