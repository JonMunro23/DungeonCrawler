using _Scripts.Tiles;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance;

    [Header("Player Spawning")]
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab;
    [SerializeField] EnemySpawnPoint enemySpawnPointPrefab;
    [SerializeField] int playerSpawnPointIndex;
    [SerializeField] int[] enemySpawnPointIndices;
    [SerializeField] GridNode spawnPointNode;
    [SerializeField] List<GridNode> enemySpawnPointNodes = new List<GridNode>();
    [SerializeField] CharacterData playerCharData;

    [Header("Grid Data")]
    [SerializeField] Grid grid;
    [SerializeField] float gridSize;
    [SerializeField] float gridCellGap;
    [SerializeField] int xLength, yLength;
    [SerializeField] Dictionary<Vector2, GridNode> spawnedNodes = new Dictionary<Vector2, GridNode>();

    [Header("NodeData")]
    [SerializeField] GridNodeData[] defaultGridNodeLayout;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitGrid();

        GenerateGrid();

        ActivateSpawnPoints();
    }

    private void InitGrid()
    {
        //Vector3 newGridSize = new Vector3(gridSize, gridSize, gridSize);
        Vector3 newCellGap = new Vector3(gridCellGap, gridCellGap, gridCellGap);

        //grid.cellSize = newGridSize;
        grid.cellGap = newCellGap;
    }

    void GenerateGrid()
    {
        int nodeIndex = 0;
        for (int i = 0; i < xLength; i++)
        {
            for(int j = 0; j < yLength; j++)
            {
                GridNode clone = Instantiate(defaultGridNodeLayout[nodeIndex].gridNodePrefab, grid.GetCellCenterLocal(new Vector3Int(i, j)), Quaternion.identity, transform);
                Vector2 spawnCoords = new Vector2(i, j);
                clone.InitNode(defaultGridNodeLayout[nodeIndex], new SquareCoords { Pos = new Vector2(i, j) });
                spawnedNodes.Add(spawnCoords, clone);

                if (nodeIndex == playerSpawnPointIndex)
                {
                    clone.CreatePlayerSpawnPoint(playerSpawnPointPrefab);
                    spawnPointNode = clone;
                }
                else
                {
                    if(enemySpawnPointIndices.Contains(nodeIndex))
                    {
                        clone.CreateEnemySpawnPoint(enemySpawnPointPrefab);
                        enemySpawnPointNodes.Add(clone);
                    }
                }

                nodeIndex++;
            }
        }

        foreach (GridNode node in spawnedNodes.Values)
        {
            node.CacheNeighbours();
        }
    }

    void ActivateSpawnPoints()
    {
        if (spawnPointNode != null)
            spawnPointNode.SpawnPlayer(playerCharData);

        if(enemySpawnPointNodes.Count > 0)
            foreach (GridNode spawnPointNode in enemySpawnPointNodes)
            {
                spawnPointNode.SpawnEnemy();
            }
    }

    public GridNode GetNodeAtCoords(Vector2 coords) => spawnedNodes.TryGetValue(coords, out var node) ? node : null;

    public GridNode GetNodeFromWorldPos(Vector3 worldPos)
    {
        return GetNodeAtCoords(new Vector2(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y));
    }

    public GridNode GetNodeInDirection(GridNode startNode, Vector3 direction)
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
        Vector2 targetPosition = startNode.Coords.Pos + offset;

        // Retrieve and return the node at the target position
        return GetNodeAtCoords(targetPosition);
    }
}
