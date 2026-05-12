using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Threading.Tasks;

public class PlayerHealthManager : MonoBehaviour, IDamageable
{
    PlayerController playerController;
    CharacterData characterData;
    [SerializeField] GameObject syringeArms;

    [Header("Stats")]
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;
    [SerializeField] int currentEvasion;
    [SerializeField] int currentArmour;

    [Header("Syringe")]
    [SerializeField] float delayBeforeRegen;
    [SerializeField] float syringeCooldown;
    [SerializeField] bool isRegenActive;
    public bool canUseSyringe;


    public static event Action<CharacterData, float> onMaxHealthUpdated;
    public static event Action<CharacterData, float> onCurrentHealthUpdated;

    [SerializeField] AudioClip[] damageTakenSFx;
    [SerializeField] float damageTakenSFXVolume;
    AudioEmitter audioEmitter;

    private void OnEnable()
    {
        StatData.onStatUpdated += OnStatUpdated;
    }

    private void OnDisable()
    {
        StatData.onStatUpdated -= OnStatUpdated;
    }

    void OnStatUpdated(StatData updatedStat)
    {
        if(updatedStat.stat == CharacterStats.MaxHealth)
        {
            UpdateMaxHealth(updatedStat.GetCurrentStatValue());
        }
    }

    void UpdateMaxHealth(float newMaxHealth)
    {
        maxHealth = Mathf.CeilToInt(newMaxHealth);
        onMaxHealthUpdated?.Invoke(characterData, maxHealth);
    }

    public void Init(PlayerController newPlayerController)
    {
        playerController = newPlayerController;
        characterData = playerController.playerCharacterData;

        UpdateMaxHealth(Mathf.CeilToInt(characterData.GetStat(CharacterStats.MaxHealth).GetBaseStatValue()));

        currentHealth = maxHealth;
        canUseSyringe = true;

        audioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);
    }

    public void TakeDamageCheat(int damageToTake)
    {
        TryDamage(damageToTake);
    }

    public void TryDamage(int damageTaken, DamageType damageType = DamageType.Physical, bool isCrit = false)
    {
        if (!PlayerController.isPlayerAlive)
            return;

        if (RollForDodge())
        {
            Debug.Log("Dodged Attack");
            return;
        }

        damageTaken = ApplyDamageResistances(damageTaken, damageType);

        if(damageTaken > 0)
            TakeDamage(damageTaken);
    }



    private void TakeDamage(int damageTaken, bool isDOT = false)
    {
        audioEmitter.ForcePlay(GetRandomAudioClip(), damageTakenSFXVolume);
        if(!isDOT)
            playerController.ShakeScreen();
        currentHealth -= damageTaken;
        if (currentHealth < 0)
            currentHealth = 0;

        onCurrentHealthUpdated?.Invoke(characterData, currentHealth);

        if (currentHealth == 0)
        {
            playerController.OnDeath();
        }
    }

    public void KillPlayer()
    {
        audioEmitter.ForcePlay(GetRandomAudioClip(), damageTakenSFXVolume);
        currentHealth -= currentHealth;
        onCurrentHealthUpdated?.Invoke(characterData, currentHealth);
        playerController.OnDeath();
    }

    AudioClip GetRandomAudioClip()
    {
        int rand = Random.Range(0, damageTakenSFx.Length);
        return damageTakenSFx[rand];
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

    public bool CanUseSyringe() => canUseSyringe && currentHealth != maxHealth;

    public async void UseSyringeInSlot(ISlot slot)
    {
        ConsumableItemData consumableData = slot.GetItemStack().itemData as ConsumableItemData;
        if (consumableData == null)
            return;

        canUseSyringe = false;
        playerController.playerInventoryManager.RemoveHealthSyringe(1);
        ConsumableItemData syringeItemData = slot.GetItemStack().itemData as ConsumableItemData;
        slot.RemoveFromExistingStack(1);
        await InjectSyringe(syringeItemData);
    }

    void EnableSyringeArms()
    {
        syringeArms.SetActive(true);
    }

    void DisableSyringeArms()
    {
        syringeArms.SetActive(false);
    }

    private IEnumerator RegenHealth(int startingHealth, int targetHealth, float duration)
    {
        isRegenActive = true;
        float timeElapsed = 0;
        while (timeElapsed < duration && isRegenActive)
        {
            float t = timeElapsed / duration;

            if(currentHealth != maxHealth)
            {
                currentHealth = Mathf.CeilToInt(Mathf.Lerp(startingHealth, targetHealth, t));
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                onCurrentHealthUpdated.Invoke(characterData, currentHealth);
            }

            timeElapsed += Time.deltaTime;

            yield return null;
        }
        isRegenActive = false;
    }

    IEnumerator SyringeUseCooldown()
    {
        //Debug.Log("Cooldown started");
        yield return new WaitForSeconds(syringeCooldown);
        canUseSyringe = true;
        //Debug.Log("Cooldown ended");
    }

    async Task InjectSyringe(ConsumableItemData syringeData)
    {
        //Debug.Log("Injecting...");
        EnableSyringeArms();
        StartCoroutine(SyringeUseCooldown());
        await Task.Delay((int)(delayBeforeRegen * 1000));

        StartCoroutine(RegenHealth(currentHealth, currentHealth + syringeData.totalRegenAmount, syringeData.regenDuration));

        await Task.Delay((int)((syringeData.useAnimationLength - delayBeforeRegen) * 1000));
        DisableSyringeArms();

        await playerController.playerWeaponManager.currentWeapon.DrawWeapon();
    }


    bool RollForDodge()
    {
        float evasionChance = Mathf.Clamp01(currentEvasion * 0.01f);
        return Random.value < evasionChance;
    }

    public void Save(ref PlayerSaveData data)
    {
        data.currentHealth = currentHealth;
    }

    public void Load(PlayerSaveData data)
    {
        currentHealth = data.currentHealth;
        onCurrentHealthUpdated?.Invoke(characterData, currentHealth);
    }

    public DamageData GetDamageData()
    {
        return new DamageData(currentHealth, currentArmour, currentEvasion);
    }

    public void TryDamageOverTime(int damageTaken, DamageType damageType = DamageType.Physical)
    {
        if (!PlayerController.isPlayerAlive)
            return;

        damageTaken = ApplyDamageResistances(damageTaken, damageType);

        // DoTs should not roll evasion. Armour still reduces damage via TakeDamage().

        if(damageTaken > 0)
            TakeDamage(damageTaken, true);
    }

    public void AddStatusEffect(StatusEffectData statusEffectToAdd)
    {
        if (statusEffectToAdd == null) return;

        // Prefer the dedicated manager (handles node + direct effects)
        if (playerController.playerStatusEffectManager != null)
        {
            playerController.playerStatusEffectManager.ApplyStatusEffect(statusEffectToAdd, source: null, refreshDuration: true);
        }
    }

    int ApplyDamageResistances(int damageTaken, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                {
                    float armour = playerController.playerStatsManager
                        .GetPlayerStat(CharacterStats.Armour)
                        .GetCurrentStatValue();

                    damageTaken = Mathf.RoundToInt(damageTaken - armour);
                    break;
                }

            case DamageType.Radiation:
                {
                    float radiationResistance = playerController.playerStatsManager
                        .GetPlayerStat(CharacterStats.RadiationResistance)
                        .GetCurrentStatValue();

                    radiationResistance = Mathf.Clamp01(radiationResistance);

                    damageTaken = Mathf.RoundToInt(damageTaken * (1f - radiationResistance));
                    break;
                }

            case DamageType.Fire:
                {
                    float fireResistance = playerController.playerStatsManager
                        .GetPlayerStat(CharacterStats.FireResistance)
                        .GetCurrentStatValue();

                    fireResistance = Mathf.Clamp01(fireResistance);

                    damageTaken = Mathf.RoundToInt(damageTaken * (1f - fireResistance));
                    break;
                }

            case DamageType.Acid:
                {
                    float acidResistance = playerController.playerStatsManager
                        .GetPlayerStat(CharacterStats.AcidResistance)
                        .GetCurrentStatValue();

                    acidResistance = Mathf.Clamp01(acidResistance);

                    damageTaken = Mathf.RoundToInt(damageTaken * (1f - acidResistance));
                    break;
                }
        }

        return damageTaken;
    }
} 
