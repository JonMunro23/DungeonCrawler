using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

public class PlayerHealthManager : MonoBehaviour, IDamageable
{
    PlayerController playerController;
    CharacterData characterData;
    [SerializeField] GameObject syringeArms;
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;

    [Header("Status Effects")]
    Dictionary<StatusEffectType, Coroutine> activeStatusEffects = new Dictionary<StatusEffectType, Coroutine>();

    [Header("Stats")]
    [SerializeField] int currentEvasion;
    [SerializeField] int currentArmour;

    [Header("Syringe")]
    [SerializeField] float delayBeforeRegen;
    [SerializeField] float syringeCooldown;
    [SerializeField] bool isRegenActive;
    public bool canUseSyringe;


    public static Action<CharacterData, float> onMaxHealthUpdated;
    public static Action<CharacterData, float> onCurrentHealthUpdated;
    public static Action<StatusEffect> onStatusEffectAdded;
    public static Action<StatusEffectType> onStatusEffectReset;
    public static Action<StatusEffectType> onStatusEffectEnded;

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
        if(updatedStat.stat == ModifiableCharacterStats.MaxHealth)
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

        UpdateMaxHealth(Mathf.CeilToInt(characterData.GetStat(ModifiableCharacterStats.MaxHealth).GetBaseStatValue()));

        currentHealth = maxHealth;
        canUseSyringe = true;

        audioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);
    }

    public void TakeDamageCheat(int damageToTake)
    {
        TryDamage(damageToTake, DamageType.Standard);
    }

    public void TryDamage(int damageTaken, DamageType damageType = DamageType.Standard)
    {
        if (!PlayerController.isPlayerAlive)
            return;

        if (RollForDodge())
        {
            Debug.Log("Dodged Attack");
            return;
        }

        TakeDamage(damageTaken);
    }

    private void TakeDamage(int damageTaken)
    {
        int damageToTake = damageTaken - currentArmour;
        audioEmitter.ForcePlay(GetRandomAudioClip(), damageTakenSFXVolume);
        playerController.ShakeScreen();
        currentHealth -= damageToTake;
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
        bool dodged = false;
        if (currentEvasion > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= currentEvasion)
            {
                dodged = true;
            }
        }
        return dodged;
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

    public void AddStatusEffect(StatusEffect statusEffectToAdd)
    {
        if (activeStatusEffects.ContainsKey(statusEffectToAdd.effectType))
        {
            ResetStatusEffect(statusEffectToAdd);
            return;
        }

        activeStatusEffects.TryAdd(statusEffectToAdd.effectType, StartCoroutine(StartStatusEffect(statusEffectToAdd)));
    }

    public void RemoveStatusEffect(StatusEffectType statusEffectToRemove)
    {
        if (activeStatusEffects.ContainsKey(statusEffectToRemove))
        {
            StopStatusEffect(statusEffectToRemove);
            return;
        }

        activeStatusEffects.Remove(statusEffectToRemove);
    }

    IEnumerator StartStatusEffect(StatusEffect statusEffectToAdd)
    {
        onStatusEffectAdded?.Invoke(statusEffectToAdd);
        if (statusEffectToAdd.dealsDOT)
        {
            StartCoroutine(HelperFunctions.DamageOverTime(this, statusEffectToAdd));
        }

        yield return new WaitForSeconds(statusEffectToAdd.effectLength);
        onStatusEffectEnded?.Invoke(statusEffectToAdd.effectType);
    }

    void ResetStatusEffect(StatusEffect statusEffectToReset)
    {
        if(activeStatusEffects.TryGetValue(statusEffectToReset.effectType, out Coroutine effectRoutine))
        {
            if(effectRoutine != null)
                StopCoroutine(effectRoutine);

            effectRoutine = StartCoroutine(StartStatusEffect(statusEffectToReset));
            onStatusEffectReset?.Invoke(statusEffectToReset.effectType);
        }
    }

    void StopStatusEffect(StatusEffectType statusEffectToStop)
    {
        if (activeStatusEffects.TryGetValue(statusEffectToStop, out Coroutine effectRoutine))
        {
            if (effectRoutine != null)
                StopCoroutine(effectRoutine);
        }

        activeStatusEffects.Remove(statusEffectToStop);
    }
}
