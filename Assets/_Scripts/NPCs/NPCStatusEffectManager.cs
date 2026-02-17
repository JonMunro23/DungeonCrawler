using UnityEngine;

[DisallowMultipleComponent]
public class NPCStatusEffectManager : MonoBehaviour
{
    [Header("Runner Settings")]
    [SerializeField] StatusEffectRunner.OverlapRule overlapRule = StatusEffectRunner.OverlapRule.StrongestWins;

    [Tooltip("Leaving a hazardous tile keeps affecting the NPC for effectLength (fallback nodeEffectLength).")]
    [SerializeField] bool enableNodeLinger = true;

    NPCController controller;
    NPCHealthController health;
    StatusEffectRunner runner;

    GridNode cachedNode;

    void Awake()
    {
        controller = GetComponent<NPCController>();
        health = GetComponent<NPCHealthController>();

        runner = new StatusEffectRunner(
            isAlive: () => health != null && health.CurrentHealth > 0f,
            applyDamage: (amount, type) => health.TryDamage(amount, type, false),
            overlapRule: overlapRule,
            enableNodeLinger: enableNodeLinger
        );

        // Debuffs (armourReduction) -> NPC armour
        runner.onTotalArmourReductionChanged += health.SetArmourFromDebuffs;
    }

    void OnEnable()
    {
        GridNode.onNodeEffectsChanged += OnAnyNodeEffectsChanged;
    }

    void OnDisable()
    {
        GridNode.onNodeEffectsChanged -= OnAnyNodeEffectsChanged;

        if (runner != null && health != null)
            runner.onTotalArmourReductionChanged -= health.SetArmourFromDebuffs;
    }

    void Update()
    {
        // Node tracking (NPC has no node-updated event like player)
        var newNode = controller != null ? controller.currentlyOccupiedGridnode : null;
        if (newNode != cachedNode)
        {
            cachedNode = newNode;
            runner.SetCurrentNode(newNode);
        }

        runner.Tick(Time.deltaTime);
    }

    void OnAnyNodeEffectsChanged(GridNode changedNode) => runner.NotifyNodeEffectsChanged(changedNode);

    // --- API used by weapons/attacks ---
    public void ApplyStatusEffect(StatusEffectData effect, bool refreshDuration = true)
        => runner.ApplyStatusEffect(effect, refreshDuration);

    public void RemoveStatusEffect(StatusEffectData effect)
        => runner.RemoveStatusEffect(effect);

    public void ClearStatusEffect(StatusEffectData effect)
        => runner.ClearStatusEffect(effect);

    public bool HasAnyStatusEffect(StatusEffectData effect)
        => runner.HasAnyStatusEffect(effect);
}
