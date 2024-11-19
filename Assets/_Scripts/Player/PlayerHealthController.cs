using UnityEngine;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;

public class PlayerHealthController : MonoBehaviour, IDamageable
{
    PlayerController playerController;
    CharacterData characterData;
    [SerializeField] GameObject syringeArms;
    [SerializeField] int currentHealth;
    bool canUseSyringe;
    int maxHealth;

    public static Action<CharacterData, int> onMaxHealthUpdated;
    public static Action<CharacterData, int> onCurrentHealthUpdated;

    [SerializeField] AudioClip[] damageTakenSFx;
    AudioSource audioSource;
    private void OnEnable()
    {
        Stat.onStatUpdated += OnStatUpdated;
    }

    private void OnDisable()
    {
        Stat.onStatUpdated -= OnStatUpdated;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
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

    public void InitHealthController(PlayerController newPlayerController)
    {
        playerController = newPlayerController;
        characterData = playerController.playerCharacterData;

        maxHealth = Mathf.CeilToInt(characterData.GetStat(ModifiableStats.MaxHealth).baseStatValue);
        onMaxHealthUpdated?.Invoke(characterData, maxHealth);

        currentHealth = maxHealth;
        canUseSyringe = true;
    }

    public void TakeDamageCheat(int damageToTake)
    {
        TakeDamage(damageToTake, false);
    }

    public void TakeDamage(int damageTaken, bool wasCrit = false)
    {
        int damageToTake = wasCrit ? damageTaken * 2 : damageTaken;
        audioSource.PlayOneShot(GetRandomAudioClip());
        playerController.ShakeScreen();
        currentHealth -= damageToTake;
        onCurrentHealthUpdated?.Invoke(characterData, currentHealth);
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

    public bool CanHeal() => currentHealth != maxHealth;
    public bool CanUseSyringe() => canUseSyringe;

    public async void UseHealthSyringe(ConsumableItemData syringeData)
    {
        if (!canUseSyringe)
            return;

        canUseSyringe = false;
        await PerformSyringeAnim(syringeData);
        StartCoroutine(HealthRegen(syringeData.healthRegenDuration));
        StartCoroutine(SyringeUseCooldown(syringeData.cooldownBetweenUses));
    }

    void EnableSyringeArms()
    {
        syringeArms.SetActive(true);
    }

    void DisableSyringeArms()
    {
        syringeArms.SetActive(false);
    }

    /// <summary>
    /// Regenerates the character progressively over time.
    /// </summary>
    /// <param name="healthAmount">Amount of vitality to be healed.</param>
    /// <param name="duration">Time needed to regenerate the player.</param>
    /// <returns></returns>
    //private IEnumerator HealProgressively(float healthAmount, float duration = 1)
    //{
    //    float targetLife = Mathf.Min(m_Vitality, CurrentVitality + healthAmount);

    //    for (float t = 0; t <= duration && m_Healing; t += Time.deltaTime)
    //    {
    //        CurrentVitality = Mathf.Lerp(CurrentVitality, targetLife, t / duration);
    //        yield return new WaitForFixedUpdate();
    //    }
    //    m_Healing = false;
    //}

    IEnumerator HealthRegen(float regenLength)
    {
        yield return new WaitForSeconds(regenLength);
        Debug.Log("Regen Ended");
    }

    IEnumerator SyringeUseCooldown(float cooldownLength)
    {
        yield return new WaitForSeconds(cooldownLength);
        Debug.Log("Cooldown ended");
        canUseSyringe = true;
    }

    async Task PerformSyringeAnim(ConsumableItemData syringeData)
    {
        EnableSyringeArms();
        await Task.Delay((int)(syringeData.useAnimationLength * 1000));
        DisableSyringeArms();
        await playerController.playerWeaponManager.currentWeapon.DrawWeapon();
    }
}
