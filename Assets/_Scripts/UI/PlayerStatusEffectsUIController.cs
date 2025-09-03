using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusEffectsUIController : MonoBehaviour
{
    [SerializeField] Transform statusIndicatorSpawnParent;
    [SerializeField] StatusEffectIndicator statusEffectIndicatorPrefab;
    Dictionary<StatusEffectType, StatusEffectIndicator> activeIndicators = new Dictionary<StatusEffectType, StatusEffectIndicator>();

    private void OnEnable()
    {
        PlayerHealthManager.onStatusEffectAdded += OnStatusEffectAdded;
        PlayerHealthManager.onStatusEffectEnded += OnStatusEffectEnded;
    }

    private void OnDisable()
    {
        PlayerHealthManager.onStatusEffectAdded -= OnStatusEffectAdded;
        PlayerHealthManager.onStatusEffectEnded -= OnStatusEffectEnded;
    }

    void OnStatusEffectAdded(StatusEffect addedStatusEffect)
    {
        SpawnStatusIndicator(addedStatusEffect);
    }
    void OnStatusEffectEnded(StatusEffectType endedStatusEffect)
    {
        if(activeIndicators.TryGetValue(endedStatusEffect, out StatusEffectIndicator indicator))
        {
            Destroy(indicator.gameObject);
        }

        activeIndicators.Remove(endedStatusEffect);
    }
    void SpawnStatusIndicator(StatusEffect effectToIndicate)
    {
        if (activeIndicators.ContainsKey(effectToIndicate.effectType)) return;

        StatusEffectIndicator clone = Instantiate(statusEffectIndicatorPrefab, statusIndicatorSpawnParent);
        activeIndicators.TryAdd(effectToIndicate.effectType, clone);
        clone.Init(effectToIndicate);
    }

}
