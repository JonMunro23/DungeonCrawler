using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [Header("Player Spawning")]
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab;
    [SerializeField] int playerSpawnPointIndex;
    [SerializeField] GridNode spawnPointNode;
    [SerializeField] CharacterData playerCharData;

    [Header("Grid Data")]
    [SerializeField] Grid grid;
    [SerializeField] float gridSize;
    [SerializeField] float gridCellGap;
    [SerializeField] int xLength, yLength;
    [SerializeField] List<GridNode> spawnedNodes = new List<GridNode>();

    [Header("NodeData")]
    [SerializeField] GridNodeData defaultGridNode;
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
                GridNode clone = Instantiate(defaultGridNode.gridNodePrefab, grid.GetCellCenterLocal(new Vector3Int(i, j)), Quaternion.identity, transform);
                clone.InitNode(defaultGridNode);
                spawnedNodes.Add(clone);

                if (nodeIndex == playerSpawnPointIndex)
                {
                    clone.CreatePlayerSpawnPoint(playerSpawnPointPrefab);
                    spawnPointNode = clone;
                }

                nodeIndex++;
            }
        }
    }

    void ActivateSpawnPoints()
    {
        if (spawnPointNode != null)
            spawnPointNode.SpawnPlayer(playerCharData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
