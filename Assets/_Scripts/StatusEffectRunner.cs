using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class StatusEffectRunner
{
    public enum OverlapRule { StrongestWins, StackBoth }

    public event Action<StatusEffectData> onEffectBecameActive;
    public event Action<StatusEffectData> onEffectEnded;
    public event Action<int> onTotalArmourReductionChanged;

    readonly OverlapRule overlapRule;
    readonly bool enableNodeLinger;

    readonly Func<bool> isAliveFn;
    readonly Action<int, DamageType> applyDamageFn;

    GridNode currentNode;

    readonly HashSet<StatusEffectData> nodeEffectsActive = new HashSet<StatusEffectData>();
    readonly Dictionary<StatusEffectData, RuntimeState> effects = new Dictionary<StatusEffectData, RuntimeState>();

    int lastArmourReduction;

    sealed class RuntimeState
    {
        public StatusEffectData data;

        // Direct
        public int directCount;
        public float directRemaining; // seconds remaining; <=0 means infinite while directCount>0
        public bool DirectActive => directCount > 0;

        // Node
        public bool nodeActive;

        // Linger
        public bool lingerActive;
        public float lingerRemaining;

        // Transition
        public bool wasAnyActive;

        // DoT scheduling (seconds until next tick)
        public float nodeTickRemaining;
        public float directTickRemaining;
    }

    public StatusEffectRunner(
        Func<bool> isAlive,
        Action<int, DamageType> applyDamage,
        OverlapRule overlapRule = OverlapRule.StrongestWins,
        bool enableNodeLinger = true)
    {
        this.isAliveFn = isAlive ?? throw new ArgumentNullException(nameof(isAlive));
        this.applyDamageFn = applyDamage ?? throw new ArgumentNullException(nameof(applyDamage));
        this.overlapRule = overlapRule;
        this.enableNodeLinger = enableNodeLinger;
    }

    // --------------------------
    // Direct effects API
    // --------------------------

    public void ApplyStatusEffect(StatusEffectData effect, bool refreshDuration = true)
    {
        if (effect == null) return;
        if (!IsAlive()) return;

        var s = GetOrCreate(effect);
        bool wasActive = IsAnyActive(s);

        s.directCount = Mathf.Max(1, s.directCount + 1);

        if (refreshDuration)
        {
            float len = Mathf.Max(0f, effect.effectLength);
            s.directRemaining = (len > 0f) ? len : 0f; // 0 => infinite
        }

        // Initialize tick timers so we don't instantly tick on apply
        if (effect.effectType == StatusEffectType.DamageOverTime && effect.damageInterval > 0f)
            s.directTickRemaining = Mathf.Max(0.001f, effect.damageInterval);

        UpdateTransitionEvents(effect, s, wasActive);
        RecomputeDebuffs();
    }

    public void RemoveStatusEffect(StatusEffectData effect)
    {
        if (effect == null) return;

        if (effects.TryGetValue(effect, out var s))
        {
            bool wasActive = IsAnyActive(s);
            s.directCount = Mathf.Max(0, s.directCount - 1);

            CleanupIfEnded(effect, s, wasActive);
            RecomputeDebuffs();
        }
    }

    public void ClearStatusEffect(StatusEffectData effect)
    {
        if (effect == null) return;

        if (effects.TryGetValue(effect, out var s))
        {
            bool wasActive = IsAnyActive(s);

            s.directCount = 0;
            s.directRemaining = 0f;
            s.nodeActive = false;
            s.lingerActive = false;
            s.lingerRemaining = 0f;

            if (wasActive)
                onEffectEnded?.Invoke(effect);

            effects.Remove(effect);
            RecomputeDebuffs();
        }
    }

    public bool HasAnyStatusEffect(StatusEffectData effect)
        => effect != null && effects.TryGetValue(effect, out var s) && IsAnyActive(s);

    // --------------------------
    // Node syncing API
    // --------------------------

    public void SetCurrentNode(GridNode node)
    {
        if (node == currentNode) return;
        currentNode = node;
        RefreshNodeEffectsFromCurrentNode();
    }

    public void NotifyNodeEffectsChanged(GridNode changedNode)
    {
        if (changedNode == null) return;
        if (changedNode != currentNode) return;
        RefreshNodeEffectsFromCurrentNode();
    }

    void RefreshNodeEffectsFromCurrentNode()
    {
        if (nodeEffectsActive.Count > 0)
        {
            tmpUnset.Clear();
            foreach (var e in nodeEffectsActive)
            {
                if (currentNode == null || !currentNode.HasNodeEffect(e))
                    tmpUnset.Add(e);
            }
            for (int i = 0; i < tmpUnset.Count; i++)
                SetNodeActive(tmpUnset[i], false);
        }

        nodeEffectsActive.Clear();

        if (currentNode == null)
        {
            RecomputeDebuffs();
            return;
        }

        foreach (var e in currentNode.activeNodeEffects)
        {
            if (e == null) continue;
            if (!e.canAffectNodes) continue;

            nodeEffectsActive.Add(e);
            SetNodeActive(e, true);
        }

        RecomputeDebuffs();
    }

    readonly List<StatusEffectData> tmpUnset = new List<StatusEffectData>(16);

    void SetNodeActive(StatusEffectData effect, bool isActive)
    {
        var s = GetOrCreate(effect);
        bool wasActive = IsAnyActive(s);

        if (isActive)
        {
            s.nodeActive = true;

            // Cancel lingering if we re-enter
            s.lingerActive = false;
            s.lingerRemaining = 0f;

            // Initialize node tick timer (no instant tick)
            if (effect.effectType == StatusEffectType.DamageOverTime && effect.nodeDamageInterval > 0f)
                s.nodeTickRemaining = Mathf.Max(0.001f, effect.nodeDamageInterval);

            UpdateTransitionEvents(effect, s, wasActive);
            return;
        }

        // Leaving / no longer applies
        s.nodeActive = false;

        if (enableNodeLinger)
            StartLinger(effect, s);
        else
        {
            s.lingerActive = false;
            s.lingerRemaining = 0f;
        }

        CleanupIfEnded(effect, s, wasActive);

        if (effects.TryGetValue(effect, out var stillThere))
            UpdateTransitionEvents(effect, stillThere, wasActive);
    }

    void StartLinger(StatusEffectData effect, RuntimeState s)
    {
        float len = effect.effectLength > 0f ? effect.effectLength : effect.nodeEffectLength;
        if (len <= 0f)
        {
            s.lingerActive = false;
            s.lingerRemaining = 0f;
            return;
        }

        s.lingerActive = true;
        s.lingerRemaining = len;

        // Ensure node tick timer exists during linger
        if (effect.effectType == StatusEffectType.DamageOverTime && effect.nodeDamageInterval > 0f)
            s.nodeTickRemaining = Mathf.Max(0.001f, effect.nodeDamageInterval);
    }

    // --------------------------
    // Tick
    // --------------------------

    public void Tick(float dt)
    {
        if (dt <= 0f) return;
        if (!IsAlive()) return;

        tmpRemove.Clear();

        foreach (var kvp in effects)
        {
            var effect = kvp.Key;
            var s = kvp.Value;
            if (effect == null) { tmpRemove.Add(effect); continue; }

            // Update timers (direct expiry)
            if (s.directCount > 0 && s.directRemaining > 0f)
            {
                s.directRemaining -= dt;
                if (s.directRemaining <= 0f)
                {
                    // direct expired
                    s.directCount = 0;
                    s.directRemaining = 0f;
                }
            }

            // Update linger expiry
            if (s.lingerActive)
            {
                s.lingerRemaining -= dt;
                if (s.lingerRemaining <= 0f)
                {
                    s.lingerActive = false;
                    s.lingerRemaining = 0f;
                }
            }

            // Apply DoT
            TickDamage(effect, s, dt);

            // End?
            if (!IsAnyActive(s) && s.wasAnyActive)
                tmpRemove.Add(effect);
        }

        // Remove ended effects and raise end events once
        for (int i = 0; i < tmpRemove.Count; i++)
        {
            var e = tmpRemove[i];
            if (e == null) continue;

            if (effects.TryGetValue(e, out var s))
            {
                if (!IsAnyActive(s))
                {
                    onEffectEnded?.Invoke(e);
                    effects.Remove(e);
                }
            }
        }

        tmpRemove.Clear();
        RecomputeDebuffs();
    }

    readonly List<StatusEffectData> tmpRemove = new List<StatusEffectData>(16);

    void TickDamage(StatusEffectData effect, RuntimeState s, float dt)
    {
        if (effect.effectType != StatusEffectType.DamageOverTime) return;

        bool nodeApplies = (s.nodeActive || s.lingerActive) && effect.nodeDamage > 0f && effect.nodeDamageInterval > 0f;
        bool directApplies = s.DirectActive && effect.damage > 0f && effect.damageInterval > 0f;

        if (!nodeApplies && !directApplies) return;

        if (overlapRule == OverlapRule.StackBoth && nodeApplies && directApplies)
        {
            // Node stream
            s.nodeTickRemaining -= dt;
            if (s.nodeTickRemaining <= 0f)
            {
                s.nodeTickRemaining += effect.nodeDamageInterval;
                ApplyDamage(effect.nodeDamage, effect.damageType);
            }

            // Direct stream
            s.directTickRemaining -= dt;
            if (s.directTickRemaining <= 0f)
            {
                s.directTickRemaining += effect.damageInterval;
                ApplyDamage(effect.damage, effect.damageType);
            }

            return;
        }

        // StrongestWins: pick one stream and tick it
        float dmg = 0f;
        float interval = float.MaxValue;
        bool usingNode = false;

        if (nodeApplies)
        {
            dmg = effect.nodeDamage;
            interval = effect.nodeDamageInterval;
            usingNode = true;
        }

        if (directApplies)
        {
            bool directBetter =
                (effect.damage > dmg) ||
                (Mathf.Approximately(effect.damage, dmg) && effect.damageInterval < interval);

            if (directBetter)
            {
                dmg = effect.damage;
                interval = effect.damageInterval;
                usingNode = false;
            }
        }

        if (usingNode)
        {
            s.nodeTickRemaining -= dt;
            if (s.nodeTickRemaining <= 0f)
            {
                s.nodeTickRemaining += interval;
                ApplyDamage(dmg, effect.damageType);
            }
        }
        else
        {
            s.directTickRemaining -= dt;
            if (s.directTickRemaining <= 0f)
            {
                s.directTickRemaining += interval;
                ApplyDamage(dmg, effect.damageType);
            }
        }
    }

    // --------------------------
    // Debuffs
    // --------------------------

    void RecomputeDebuffs()
    {
        int total = 0;

        foreach (var kvp in effects)
        {
            var e = kvp.Key;
            var s = kvp.Value;

            if (e == null) continue;
            if (!IsAnyActive(s)) continue;

            if (e.effectType == StatusEffectType.Debuff && e.armourReduction > 0)
                total += e.armourReduction;
        }

        if (total != lastArmourReduction)
        {
            lastArmourReduction = total;
            onTotalArmourReductionChanged?.Invoke(total);
        }
    }

    // --------------------------
    // Helpers
    // --------------------------

    RuntimeState GetOrCreate(StatusEffectData effect)
    {
        if (!effects.TryGetValue(effect, out var s))
        {
            s = new RuntimeState { data = effect, wasAnyActive = false };
            effects.Add(effect, s);
        }
        return s;
    }

    bool IsAlive() => isAliveFn != null && isAliveFn();

    bool IsAnyActive(RuntimeState s)
        => s != null && (s.nodeActive || s.lingerActive || s.DirectActive);

    void ApplyDamage(float amount, DamageType type)
    {
        if (!IsAlive()) return;
        int dmg = Mathf.CeilToInt(amount);
        if (dmg <= 0) return;
        applyDamageFn.Invoke(dmg, type);
    }

    void UpdateTransitionEvents(StatusEffectData effect, RuntimeState s, bool wasActive)
    {
        bool isActive = IsAnyActive(s);
        s.wasAnyActive = s.wasAnyActive || isActive;

        if (!wasActive && isActive)
            onEffectBecameActive?.Invoke(effect);
    }

    void CleanupIfEnded(StatusEffectData effect, RuntimeState s, bool wasActive)
    {
        if (IsAnyActive(s))
            return;

        // Only fire "ended" if it was active at some point
        if (wasActive || s.wasAnyActive)
            onEffectEnded?.Invoke(effect);

        effects.Remove(effect);
    }

    bool WasEverActive(RuntimeState s) => s != null && s.wasAnyActive;

}
