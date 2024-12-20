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
    [SerializeField] int maxHealth;
    [SerializeField] int currentHealth;

    [Header("Stats")]
    [SerializeField] int evasion;
    [SerializeField] int armour;

    [Header("Syringe")]
    [SerializeField] float delayBeforeRegen;
    [SerializeField] float syringeCooldown;
    [SerializeField] bool isRegenActive;
    public bool canUseSyringe;


    public static Action<CharacterData, float> onMaxHealthUpdated;
    public static Action<CharacterData, float> onCurrentHealthUpdated;

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

    public void Init(PlayerController newPlayerController)
    {
        playerController = newPlayerController;
        characterData = playerController.playerCharacterData;

        UpdateMaxHealth(Mathf.CeilToInt(characterData.GetStat(ModifiableStats.MaxHealth).GetBaseStatValue()));

        currentHealth = maxHealth;
        canUseSyringe = true;

        audioEmitter = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterBody", transform.root, spatialBlend: 0);
    }

    public void TakeDamageCheat(int damageToTake)
    {
        TryDamage(damageToTake, false);
    }

    public void TryDamage(int damageTaken, bool wasCrit = false)
    {
        if (!PlayerController.isPlayerAlive)
            return;

        if (RollForDodge())
        {
            Debug.Log("Dodged Attack");
            return;
        }

        TakeDamage(damageTaken, wasCrit);
    }

    private void TakeDamage(int damageTaken, bool wasCrit)
    {
        int damageToTake = wasCrit ? damageTaken * 2 : damageTaken;
        damageTaken -= armour;
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

    public async void UseSyringeInSlot(InventorySlot slot)
    {
        //Debug.Log("Using syringe");
        ConsumableItemData consumableData = slot.GetItemStack().itemData as ConsumableItemData;
        if (consumableData == null)
            return;

        canUseSyringe = false;
        playerController.playerInventoryManager.RemoveHealthSyringe(1);
        slot.RemoveFromExistingStack(1);
        await InjectSyringe(slot.GetItemStack().itemData as ConsumableItemData);
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
        if (evasion > 0)
        {
            float rand = Random.Range(0, 101);
            if (rand <= evasion)
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
}
