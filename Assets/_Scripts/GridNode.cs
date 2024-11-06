using _Scripts.Tiles;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GridNode : MonoBehaviour
{
    [HideInInspector]
    public GridNodeData nodeData;
    public ICoords coords;
    PlayerSpawnPoint playerSpawnPoint;
    EnemySpawnPoint enemySpawnPoint;
    MeshRenderer meshRenderer;
    [SerializeField]
    Material highlightPathMat, highlightOpenMat, highlightClosedMat, defaultMat;
    [SerializeField] TMP_Text coordText;
    public List<GridNode> neighbouringNodes = new List<GridNode>();

    public Transform moveToTransform;
    public bool isOccupied;

    [Header("Pathfinding")]
    [SerializeField]
    private TMP_Text _fCostText, _gCostText, _hCostText;
    public GridNode Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void SetOccupied(bool _isOccupied)
    {
        isOccupied = _isOccupied;
    }

    private void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void SetConnection(GridNode nodeBase)
    {
        Connection = nodeBase;
    }

    public float GetDistance(GridNode other) => coords.GetDistance(other.coords); // Helper to reduce noise in pathfinding

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

    private static readonly List<Vector2> Dirs = new List<Vector2>() {
        new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1), new Vector2(1, 0),
    };

    public void InitNode(GridNodeData newNodeData, ICoords _coords)
    {
        coords = _coords;
        nodeData = newNodeData;

        coordText.text = $"({coords.Pos.x},{coords.Pos.y})";
    }

    public void CacheNeighbours()
    {
        neighbouringNodes = new List<GridNode>();

        foreach (GridNode neighbouringNode in Dirs.Select(dir => GridController.Instance.GetNodeAtPosition(coords.Pos + dir)).Where(tile => tile != null))
        {
            neighbouringNodes.Add(neighbouringNode);
        }
    }

    public void CreatePlayerSpawnPoint(PlayerSpawnPoint spawnPointToCreate)
    {
        playerSpawnPoint = Instantiate(spawnPointToCreate, transform);
    }

    public void CreateEnemySpawnPoint(EnemySpawnPoint spawnPointToCreate)
    {
        enemySpawnPoint = Instantiate(spawnPointToCreate, transform);
    }


    public void SpawnPlayer(CharacterData playerCharData)
    {
        playerSpawnPoint.SpawnPlayer(playerCharData, this);
    }

    public void SpawnEnemy()
    {
        enemySpawnPoint.SpawnEnemy(this);
    }

}
