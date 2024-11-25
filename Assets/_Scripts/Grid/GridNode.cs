using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
public enum GridNodeOccupantType
{
    None,
    Enemy,
    Obstacle,
    Player
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

public class GridNode : MonoBehaviour
{
    [SerializeField] bool showDebugInfo;

    public GridNodeData nodeData;
    public ICoords Coords;
    PlayerSpawnPoint playerSpawnPoint;
    NPCSpawnPoint enemySpawnPoint;
    MeshRenderer meshRenderer;
    [SerializeField]
    Material highlightPathMat, highlightOpenMat, highlightClosedMat, defaultMat;
    [SerializeField] TMP_Text coordText;
    public List<GridNode> neighbouringNodes = new List<GridNode>();

    public Transform moveToTransform;
    public GridNodeOccupant currentOccupant;

    public static Action onNodeOccupancyUpdated;

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
    public void SetOccupant(GridNodeOccupant newOccupant)
    {
        currentOccupant = newOccupant;
        onNodeOccupancyUpdated?.Invoke();
    }

    public void ClearOccupant()
    {
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

    public float GetDistance(GridNode other) => Coords.GetDistance(other.Coords); // Helper to reduce noise in pathfinding

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


    public void InitNode(ICoords _coords)
    {
        Coords = _coords;

        coordText.text = $"({Coords.Pos.x},{Coords.Pos.y})";
    }

    public void CacheNeighbours()
    {
        neighbouringNodes = new List<GridNode>();

        foreach (GridNode neighbouringNode in Dirs.Select(dir => GridController.Instance.GetNodeAtCoords(Coords.Pos + dir)).Where(tile => tile != null))
        {
            neighbouringNodes.Add(neighbouringNode);
        }
    }

    public GridNode GetNodeInDirection(Vector3 direction)
    {
        // Convert the direction into a grid offset
        Vector2 offset = Vector2.zero;

        if (direction == Vector3.forward)
            offset = new Vector2(1, 0);  // Up
        else if (direction == Vector3.back)
            offset = new Vector2(-1, 0); // Down
        else if (direction == Vector3.left)
            offset = new Vector2(0, -1); // Left
        else if (direction == Vector3.right)
            offset = new Vector2(0, 1);  // Right

        // Calculate the target position by adding the offset to the start node position
        Vector2 targetPosition = Coords.Pos + offset;

        // Retrieve and return the node at the target position
        return GridController.Instance.GetNodeAtCoords(targetPosition);
    }
}
