using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCHealthController : MonoBehaviour, IDamageable
{
    NPCController controller;

    [SerializeField] float currentHealth;
    public float CurrentHealth => currentHealth;

    [SerializeField] float maxHealth;
    [SerializeField] int currentArmourRating;
    [SerializeField] int currentEvasionRating;
    bool isDead;

    [Header("Item Dropping")]
    public List<ItemData> guaranteedDrops = new List<ItemData>();
    public List<ItemData> randomDrops = new List<ItemData>();

    public void Init(NPCController controller)
    {
        this.controller = controller;
        maxHealth = controller.npcData.maxHealth;
        currentHealth = maxHealth;
        currentArmourRating = controller.npcData.baseArmourRating;
        currentEvasionRating = controller.npcData.baseEvasionRating;
    }

    public void SetHealth(int newHealthValue)
    {
        currentHealth = newHealthValue;
    }

    public void TryDamage(int damage, DamageType damageType = DamageType.Standard, bool isCrit = false)
    {
        if (isDead) return;

        if(isCrit)
        {
            PlayHitReaction();
            damage *= 2;
        }
        currentHealth -= damage;
        controller.floatingTextController.SpawnDamageText(damage, damageType, isCrit);


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

    public void AddStatusEffect(StatusEffect statusEffectToAdd)
    {
        //switch (statusEffectToAdd.effectType)
        //{
        //    case StatusEffectType.Fire:
        //        if(fireDamageCoroutine != null)
        //            StopCoroutine(fireDamageCoroutine);

        //        fireDamageCoroutine = StartCoroutine(HelperFunctions.TakeDamageOverTime(this, statusEffectToAdd));
        //        break;
        //    case StatusEffectType.Acid:
        //        int acidArmourReduction = 20;
        //        if (armourReductionCoroutine != null)
        //        {
        //            StopCoroutine(armourReductionCoroutine);
        //            currentArmourRating = NPCData.baseArmourRating;
        //        }

        //        armourReductionCoroutine = StartCoroutine(ReduceArmourRating(statusEffectToAdd.effectLength, acidArmourReduction));

        //        if(acidDamageCoroutine != null)
        //            StopCoroutine(acidDamageCoroutine);

        //        acidDamageCoroutine = StartCoroutine(HelperFunctions.TakeDamageOverTime(this, statusEffectToAdd));
        //        break;
        //}
        //statusEffectToAdd.ApplyStatusEffect(this);
    }

    public DamageData GetDamageData()
    {
        return new DamageData(Mathf.RoundToInt(currentHealth), currentArmourRating, currentEvasionRating);
    }

    IEnumerator ReduceArmourRating(float duration, int reductionAmount)
    {
        currentArmourRating -= reductionAmount;
        if (currentArmourRating < 0)
            currentArmourRating = 0;

        yield return new WaitForSeconds(duration);

        currentArmourRating = controller.npcData.baseArmourRating;

    }

    public void PlayHitReaction()
    {
       //pause movement?
        controller.animController.PlayAnimation("HitReaction", 0);
    }
}
