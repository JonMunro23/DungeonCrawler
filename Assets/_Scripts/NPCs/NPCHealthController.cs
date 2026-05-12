using System.Collections.Generic;
using UnityEngine;
using System;

public class NPCHealthController : MonoBehaviour, IDamageable
{
    NPCController controller;
    NPCStatusEffectManager statusEffectManager;

    [SerializeField] float currentHealth;
    public float CurrentHealth => currentHealth;
    int baseArmourRating;

    [SerializeField] float maxHealth;
    [SerializeField] int currentArmourRating;
    bool isDead;

    [Header("Item Dropping")]
    public List<ItemData> guaranteedDrops = new List<ItemData>();
    public List<ItemData> randomDrops = new List<ItemData>();

    public event Action<int, DamageType, bool> onDamaged;

    public void Init(NPCController controller)
    {
        this.controller = controller;
        statusEffectManager = GetComponent<NPCStatusEffectManager>();

        maxHealth = controller.npcData.maxHealth;
        currentHealth = maxHealth;

        baseArmourRating = controller.npcData.baseArmourRating;
        currentArmourRating = baseArmourRating;
    }


    public void SetHealth(int newHealthValue)
    {
        currentHealth = newHealthValue;
    }

    public void TryDamage(int damage, DamageType damageType = DamageType.Physical, bool isCrit = false)
    {
        if (isDead) return;

        if (isCrit)
        {
            PlayHitReaction();
            damage *= 2;
        }

        currentHealth -= damage;
        controller.floatingTextController.SpawnDamageText(damage, damageType, isCrit);

        // NEW: notify listeners (eg. movement controller -> switch to Pursue)
        onDamaged?.Invoke(damage, damageType, isCrit);

        if (currentHealth <= 0)
        {
            isDead = true;
            if (guaranteedDrops.Count > 0)
            {
                foreach (ItemData drop in guaranteedDrops)
                {
                    Instantiate(drop.itemWorldModel, transform.position, Quaternion.identity);
                }
            }
            controller.OnDeath();
        }
    }

    public DamageData GetDamageData()
    {
        return new DamageData(Mathf.RoundToInt(currentHealth), currentArmourRating, 0);
    }

    public void PlayHitReaction()
    {
        //pause movement?
        controller.animController.PlayAnimation("HitReaction", 0);
    }

    public void AddStatusEffect(StatusEffectData statusEffectToAdd)
    {
        if (statusEffectToAdd == null) return;

        if (statusEffectManager != null)
            statusEffectManager.ApplyStatusEffect(statusEffectToAdd, refreshDuration: true);

    }

    public void SetArmourFromDebuffs(int totalArmourReduction)
    {
        int reduction = Mathf.Max(0, totalArmourReduction);
        currentArmourRating = Mathf.Max(0, baseArmourRating - reduction);
    }


}
