using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStatusEffectManager : MonoBehaviour
{
    // UI listens to these
    public static event System.Action<StatusEffectData> onStatusEffectAdded;
    public static event System.Action<StatusEffectData> onStatusEffectEnded;

    [Header("Runner Settings")]
    [SerializeField] StatusEffectRunner.OverlapRule overlapRule = StatusEffectRunner.OverlapRule.StrongestWins;

    [Tooltip("Leaving a hazardous tile keeps affecting the player for effectLength (fallback nodeEffectLength).")]
    [SerializeField] bool enableNodeLinger = true;

    PlayerHealthManager health;
    StatusEffectRunner runner;

    void Awake()
    {
        health = GetComponent<PlayerHealthManager>();

        // IMPORTANT: PlayerHealthManager must have TryDamageOverTime(int, DamageType)
        runner = new StatusEffectRunner(
            isAlive: () => PlayerController.isPlayerAlive,
            applyDamage: (amount, type) => health.TryDamageOverTime(amount, type),
            overlapRule: overlapRule,
            enableNodeLinger: enableNodeLinger
        );

        runner.onEffectBecameActive += e => onStatusEffectAdded?.Invoke(e);
        runner.onEffectEnded += e => onStatusEffectEnded?.Invoke(e);
    }

    void OnEnable()
    {
        PlayerController.onPlayerOccupiedNodeUpdated += OnPlayerNodeChanged;
        GridNode.onNodeEffectsChanged += OnAnyNodeEffectsChanged;
    }

    void OnDisable()
    {
        PlayerController.onPlayerOccupiedNodeUpdated -= OnPlayerNodeChanged;
        GridNode.onNodeEffectsChanged -= OnAnyNodeEffectsChanged;
    }

    void Update()
    {
        runner?.Tick(Time.deltaTime);
    }

    void OnPlayerNodeChanged(GridNode node) => runner?.SetCurrentNode(node);
    void OnAnyNodeEffectsChanged(GridNode changed) => runner?.NotifyNodeEffectsChanged(changed);

    // --- API for enemies/traps/etc ---
    public void ApplyStatusEffect(StatusEffectData effect, Object source = null, bool refreshDuration = true)
        => runner?.ApplyStatusEffect(effect, refreshDuration);

    public void RemoveStatusEffect(StatusEffectData effect)
        => runner?.RemoveStatusEffect(effect);

    public void ClearStatusEffect(StatusEffectData effect)
        => runner?.ClearStatusEffect(effect);

    public bool HasAnyStatusEffect(StatusEffectData effect)
        => runner != null && runner.HasAnyStatusEffect(effect);
}
