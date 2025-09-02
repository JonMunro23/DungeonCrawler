using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
public class GridNode : MonoBehaviour
{
    [SerializeField] bool showDebugInfo;

    public GridNodeData nodeData;
    public ICoords Coords;
    MeshRenderer meshRenderer;
    [SerializeField]
    Material highlightPathMat, highlightOpenMat, highlightClosedMat, defaultMat;
    [SerializeField] TMP_Text coordText;
    public List<GridNode> neighbouringNodes = new List<GridNode>();

    public Transform moveToTransform;
    public GridNodeOccupant currentOccupant;
    public GridNodeOccupant baseOccupant;

    bool isExplored;
    bool isVoid;

    public static Action onNodeOccupancyUpdated;

    [Header("Tile Effects")]
    [SerializeField] ParticleSystem fireParticles;
    bool isIgnited;
    Coroutine ignitedRoutine;

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
            _fCostText.enabled = false;
            _gCostText.enabled = false;
            _hCostText.enabled = false;
            coordText.enabled = false;
        }

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
        if (baseOccupant != null)
        {
            currentOccupant = baseOccupant;
            return;
        }

        currentOccupant.occupantType = GridNodeOccupantType.None;
        currentOccupant.occupyingGameobject = null;
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
        meshRenderer.material = highlightOpenMat;
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
        _gCostText.text = G.ToString();
        _hCostText.text = H.ToString();
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

        List<GridNode> nodesToSetExplored = new List<GridNode>(GetNeighbouringNodes(true));

        foreach(GridNode node in nodesToSetExplored)
        {
            node.SetIsExplored(true);
        }
    }

    public bool GetIsExplored() => isExplored;

    public void InitNode(ICoords _coords)
    {
        Coords = _coords;

        coordText.text = $"({Coords.Pos.x},{Coords.Pos.y})";
    }

    public void IgniteNode(float igniteLength)
    {
        if (!nodeData.isWalkable || isVoid) return;

        isIgnited = true;
        //display ignited VFX
        //play ignited SFX
        if(GetOccupyingGameobject().TryGetComponent(out IDamageable damageable))
        {
            damageable.AddStatusEffect(StatusEffectType.Fire);
        }

        if (ignitedRoutine != null)
            StopCoroutine(ignitedRoutine);

        ignitedRoutine = StartCoroutine(TileEffectTimer(StatusEffectType.Fire, igniteLength));
    }

    public void RemoveTileEffect(StatusEffectType effectToRemove)
    {
        switch (effectToRemove)
        {
            case StatusEffectType.Fire:
                isIgnited = false;
                break;
            case StatusEffectType.Acid:
                break;
            default:
                break;
        }
    }

    IEnumerator TileEffectTimer(StatusEffectType effect, float length)
    {
        yield return new WaitForSeconds(length);
        RemoveTileEffect(effect);
    }

    public void CacheNeighbours()
    {
        neighbouringNodes = GetNeighbouringNodes();
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
}
