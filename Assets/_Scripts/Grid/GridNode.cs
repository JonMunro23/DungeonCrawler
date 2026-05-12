using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
public enum GridNodeOccupantType
{
    None,
    NPC,
    Obstacle,
    Player,
    LevelTransition,
    PressurePlate,
    NPCInaccessible
}

[System.Serializable]
public class GridNodeOccupant
{
    public GameObject occupyingGameobject;
    public GridNodeOccupantType occupantType;

    public GridNodeOccupant(GameObject occupyingGameobject, GridNodeOccupantType occupantType)
    {
        this.occupyingGameobject = occupyingGameobject;
        this.occupantType = occupantType;
    }
}

public interface ICoords
{
    public float GetDistance(ICoords other);
    public Vector2 Pos { get; set; }
}

[System.Serializable]
[SelectionBase]
public class GridNode : MonoBehaviour
{
    [SerializeField] bool showDebugInfo;
    [SerializeField] Canvas debugCanvas;
    public GridNodeData nodeData;
    public ICoords Coords;
    MeshRenderer meshRenderer;
    [SerializeField]
    Material highlightPathMat, highlightOpenMat, highlightClosedMat, defaultMat;
    [SerializeField] TMP_Text coordText;
    public List<GridNode> neighbouringNodes = new List<GridNode>();
    public List<GridNode> allNeighbouringNodes = new List<GridNode>();
    public Transform moveToTransform;
    public GridNodeOccupant currentOccupant;
    public GridNodeOccupant baseOccupant;

    int nodeIndex;
    public int NodeIndex => nodeIndex;
    bool isExplored;
    bool isVoid;

    //public void IrradiateNode(StatusEffectData radiationStatusEffect)
    //{
    //    if(!activeNodeEffects.Contains(radiationStatusEffect))
    //    {
    //        activeNodeEffects.Add(radiationStatusEffect);
    //    }
    //    HighlightCellPath();
    //}

    //public void RemoveRadiation()
    //{
    //    UnhighlightCell();
    //}


    public static event Action onNodeOccupancyUpdated;

    //[Header("Node Effects")]
    //public List<StatusEffectData> activeNodeEffects = new List<StatusEffectData>();
    //[SerializeField] ParticleSystem fireParticles;
    //bool isIgnited;
    //Coroutine ignitedRoutine;

    [Header("Pathfinding")]
    [SerializeField]
    private TMP_Text _fCostText, _gCostText, _hCostText;
    public GridNode Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    private static readonly List<Vector2> Dirs = new List<Vector2>() {
        new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0),
    };

    private static readonly List<Vector2> DiagDirs = new List<Vector2>() {
        new Vector2(1, 1), new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1),
    };

    private void Start()
    {
        if(!showDebugInfo)
        {
            if(debugCanvas)
                debugCanvas.gameObject.SetActive(false);

            if(_fCostText)
                _fCostText.enabled = false;

            if(_gCostText)
                _gCostText.enabled = false;

            if(_hCostText)
                _hCostText.enabled = false;

            if(coordText)
                coordText.enabled = false;
        }

    }

    public void InitNode(ICoords _coords, int nodeIndex)
    {
        this.nodeIndex = nodeIndex;
        gameObject.name = $"{gameObject.name}_{nodeIndex.ToString()}";
        Coords = _coords;
        if (coordText)
            coordText.text = $"({Coords.Pos.x},{Coords.Pos.y})";
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetBaseOccupant(GridNodeOccupant newOccupant)
    {
        baseOccupant = newOccupant;
    }

    public void SetOccupant(GridNodeOccupant newOccupant)
    {
        currentOccupant = newOccupant;
        onNodeOccupancyUpdated?.Invoke();
    }

    public void ResetOccupant()
    {
        //if (baseOccupant != null)
        //{
            currentOccupant = baseOccupant;
        //    return;
        //}

        //currentOccupant.occupantType = GridNodeOccupantType.None;
        //currentOccupant.occupyingGameobject = null;
    }

    public GridNodeOccupantType GetOccupantType()
    {
        return currentOccupant.occupantType;
    }

    public GameObject GetOccupyingGameobject()
    {
        return currentOccupant.occupyingGameobject;
    }

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void SetConnection(GridNode nodeBase)
    {
        Connection = nodeBase;
    }

    public float GetDistance(GridNode other)
    {
        if (other == null)
            return 0;

        return Coords.GetDistance(other.Coords);
    }

    public void SetG(float g)
    {
        G = g;
        SetText();
    }

    public void SetH(float h)
    {
        H = h;
        SetText();
    }

    public void HighlightCellOpen()
    {
        meshRenderer.material = highlightOpenMat;
    }

    public void HighlightCellClosed()
    {
        meshRenderer.material = highlightClosedMat;
    }

    public void HighlightCellPath()
    {
        meshRenderer.material = highlightPathMat;
    }

    public void UnhighlightCell()
    {
        meshRenderer.material = defaultMat;
    }

    private void SetText()
    {
        //if (_selected) return;
        if(_gCostText.enabled)
            _gCostText.text = G.ToString();
        if(_hCostText.enabled)
            _hCostText.text = H.ToString();
        if(_fCostText)
            _fCostText.text = F.ToString();
    }

    public void RevertTile()
    {
        UnhighlightCell();
        _gCostText.text = "";
        _hCostText.text = "";
        _fCostText.text = "";
    }

    public void SetIsVoid(bool isVoid) => this.isVoid = isVoid;

    public bool GetIsVoid() => isVoid;

    public void SetIsExplored(bool isExplored) => this.isExplored = isExplored;

    public void SetSelfAndSurroundingNodesExplored()
    {
        SetIsExplored(true);

        List<GridNode> nodesToSetExplored = new List<GridNode>(allNeighbouringNodes);

        foreach(GridNode node in nodesToSetExplored)
        {
            node.SetIsExplored(true);
        }
    }

    public bool GetIsExplored() => isExplored;



    public void CacheNeighbours()
    {
        neighbouringNodes = GetNeighbouringNodes();
        allNeighbouringNodes = GetNeighbouringNodes(true);
    }

    public List<GridNode> GetNeighbouringNodes(bool getDiagonals = false)
    {
        List<GridNode> nodes = new List<GridNode>();

        foreach (GridNode neighbouringNode in Dirs.Select(dir => GridController.Instance.GetNodeAtCoords(Coords.Pos + dir)).Where(tile => tile != null))
        {
            nodes.Add(neighbouringNode);
        }

        if(getDiagonals)
        {
            foreach (GridNode diagNeighbouringNode in DiagDirs.Select(dir => GridController.Instance.GetNodeAtCoords(Coords.Pos + dir)).Where(tile => tile != null))
            {
                nodes.Add(diagNeighbouringNode);
            }
        }

        return nodes;
    }

    public GridNode GetNodeInDirection(Vector3 direction)
    {
        //Debug.Log($"Input Direction: {direction}");

        // Round the input direction to nearest integers
        Vector3 roundedMoveDir = new Vector3(
            Mathf.RoundToInt(direction.x),
            Mathf.RoundToInt(direction.y),
            Mathf.RoundToInt(direction.z));

        //Debug.Log($"Rounded Move Direction: {roundedMoveDir}");

        // Initialize the offset to zero
        Vector2 offset = Vector2.zero;

        // Handle cardinal directions
        if (roundedMoveDir == Vector3.forward)
        {
            offset = new Vector2(1, 0);  // Forward
        }
        else if (roundedMoveDir == Vector3.back)
        {
            offset = new Vector2(-1, 0); // Backward
        }
        else if (roundedMoveDir == Vector3.left)
        {
            offset = new Vector2(0, -1); // Left
        }
        else if (roundedMoveDir == Vector3.right)
        {
            offset = new Vector2(0, 1);  // Right
        }
        else
        {
            // Handle diagonals (or other edge cases)
            float absX = Mathf.Abs(direction.x);
            float absZ = Mathf.Abs(direction.z);

            if (absX > absZ)
            {
                // Horizontal dominance: use X for horizontal offset (left/right)
                offset = new Vector2(0, Mathf.RoundToInt(roundedMoveDir.x));
            }
            else
            {
                // Vertical dominance: use Z for vertical offset (forward/backward)
                offset = new Vector2(Mathf.RoundToInt(roundedMoveDir.z), 0);
            }
        }

        // Calculate the target position
        Vector2 targetPosition = Coords.Pos + offset;

        // Retrieve and return the node at the target position
        return GridController.Instance.GetNodeAtCoords(targetPosition);
    }

    // ==========================
    // Node Effects (runtime)
    // ==========================
    public static event System.Action<GridNode> onNodeEffectsChanged;

    [Header("Node Effects")]
    public List<StatusEffectData> activeNodeEffects = new List<StatusEffectData>();

    // For emitter-style effects (eg radiation): which sources currently apply this effect
    readonly Dictionary<StatusEffectData, HashSet<Object>> _effectSources = new();

    // For timed effects (eg fire): expiry + coroutine per effect
    readonly Dictionary<StatusEffectData, float> _effectExpiryTime = new();
    readonly Dictionary<StatusEffectData, Coroutine> _effectExpiryRoutine = new();

    public bool HasNodeEffect(StatusEffectData effect) =>
        effect != null && activeNodeEffects.Contains(effect);

    /// <summary>
    /// Adds a timed node effect (eg Fire). Refreshes duration if applied again.
    /// If duration <= 0, uses effect.nodeEffectLength.
    /// </summary>
    public void AddTimedNodeEffect(StatusEffectData effect, float durationSeconds = 0f)
    {
        if (effect == null) return;
        if (!effect.canAffectNodes) return;

        float duration = durationSeconds > 0f ? durationSeconds : effect.nodeEffectLength;

        // If length is not set, treat as "infinite" (but still timed API)
        if (duration <= 0f)
        {
            AddNodeEffectInternal(effect);
            // No expiry coroutine if infinite
            RaiseNodeEffectsChanged();
            return;
        }

        AddNodeEffectInternal(effect);

        // Refresh expiry to max(existing, now+duration)
        float newExpiry = Time.time + duration;
        if (_effectExpiryTime.TryGetValue(effect, out float existing))
            _effectExpiryTime[effect] = Mathf.Max(existing, newExpiry);
        else
            _effectExpiryTime[effect] = newExpiry;

        // Ensure a single expiry coroutine per effect
        if (_effectExpiryRoutine.TryGetValue(effect, out var routine) && routine != null)
            StopCoroutine(routine);

        _effectExpiryRoutine[effect] = StartCoroutine(ExpireEffectAfterTime(effect));

        RaiseNodeEffectsChanged();
    }

    /// <summary>
    /// Adds an effect from a source object (eg RadiationEmitter). Effect remains while ANY source remains.
    /// Duration is ignored for source-based effects.
    /// </summary>
    public void AddNodeEffectFromSource(StatusEffectData effect, Object source)
    {
        if (effect == null) return;
        if (!effect.canAffectNodes) return;
        if (source == null)
        {
            // fallback: treat as timed/infinite
            AddTimedNodeEffect(effect, effect.nodeEffectLength);
            return;
        }

        AddNodeEffectInternal(effect);

        if (!_effectSources.TryGetValue(effect, out var set) || set == null)
        {
            set = new HashSet<Object>();
            _effectSources[effect] = set;
        }

        // Avoid double-adding same source
        bool added = set.Add(source);
        if (added)
            RaiseNodeEffectsChanged();
    }

    /// <summary>
    /// Removes an effect contribution from a given source.
    /// If this was the last source and there is no timed expiry keeping it alive, the effect is removed.
    /// </summary>
    public void RemoveNodeEffectFromSource(StatusEffectData effect, Object source)
    {
        if (effect == null || source == null) return;

        if (_effectSources.TryGetValue(effect, out var set) && set != null)
        {
            bool removed = set.Remove(source);

            if (set.Count == 0)
                _effectSources.Remove(effect);

            if (removed)
            {
                // If no more sources AND no active timed expiry -> remove entirely
                bool hasTimed = _effectExpiryTime.ContainsKey(effect);
                bool hasSources = _effectSources.ContainsKey(effect);

                if (!hasTimed && !hasSources)
                    RemoveNodeEffect(effect);
                else
                    RaiseNodeEffectsChanged();
            }
        }
    }

    /// <summary>
    /// Fully removes an effect from this node (used by timed expiry, cleanup, etc).
    /// </summary>
    public void RemoveNodeEffect(StatusEffectData effect)
    {
        if (effect == null) return;

        // Stop expiry coroutine if any
        if (_effectExpiryRoutine.TryGetValue(effect, out var routine) && routine != null)
            StopCoroutine(routine);

        _effectExpiryRoutine.Remove(effect);
        _effectExpiryTime.Remove(effect);
        _effectSources.Remove(effect);

        activeNodeEffects.Remove(effect);
        RaiseNodeEffectsChanged();
    }

    void AddNodeEffectInternal(StatusEffectData effect)
    {
        if (!activeNodeEffects.Contains(effect))
            activeNodeEffects.Add(effect);
    }

    System.Collections.IEnumerator ExpireEffectAfterTime(StatusEffectData effect)
    {
        while (true)
        {
            if (!_effectExpiryTime.TryGetValue(effect, out float expiry))
                yield break;

            float remaining = expiry - Time.time;
            if (remaining <= 0f)
                break;

            yield return null;
        }

        // Only remove if no sources are keeping it alive
        bool hasSources = _effectSources.TryGetValue(effect, out var set) && set != null && set.Count > 0;
        if (!hasSources)
            RemoveNodeEffect(effect);
        else
            RaiseNodeEffectsChanged();
    }

    void RaiseNodeEffectsChanged()
    {
        onNodeEffectsChanged?.Invoke(this);
    }

    void SetNodeEffectParticlesActive(StatusEffectData effect, bool isActive)
    {

    }

}
