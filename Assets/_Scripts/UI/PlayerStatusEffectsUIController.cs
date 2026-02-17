using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusEffectsUIController : MonoBehaviour
{
    [SerializeField] Transform statusIndicatorSpawnParent;
    [SerializeField] StatusEffectIndicator statusEffectIndicatorPrefab;
    Dictionary<StatusEffectData, StatusEffectIndicator> activeIndicators = new Dictionary<StatusEffectData, StatusEffectIndicator>();

    private void OnEnable()
    {
        PlayerStatusEffectManager.onStatusEffectAdded += OnStatusEffectAdded;
        PlayerStatusEffectManager.onStatusEffectEnded += OnStatusEffectEnded;
    }

    private void OnDisable()
    {
        PlayerStatusEffectManager.onStatusEffectAdded -= OnStatusEffectAdded;
        PlayerStatusEffectManager.onStatusEffectEnded -= OnStatusEffectEnded;
    }


    void OnStatusEffectAdded(StatusEffectData addedStatusEffect)
    {
        SpawnStatusIndicator(addedStatusEffect);
    }
    void OnStatusEffectEnded(StatusEffectData endedStatusEffect)
    {
        if (activeIndicators.TryGetValue(endedStatusEffect, out StatusEffectIndicator indicator))
            Destroy(indicator.gameObject);

        activeIndicators.Remove(endedStatusEffect);
    }

    void SpawnStatusIndicator(StatusEffectData effectToIndicate)
    {
        if (activeIndicators.ContainsKey(effectToIndicate)) return;

        StatusEffectIndicator clone = Instantiate(statusEffectIndicatorPrefab, statusIndicatorSpawnParent);
        activeIndicators.TryAdd(effectToIndicate, clone);
        clone.Init(effectToIndicate);
    }

}
