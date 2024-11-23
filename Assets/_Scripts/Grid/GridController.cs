using LDtkUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance;

    [SerializeField] LDtkComponentProject project;
    [Header("Grid")]
    [SerializeField] GridNode wallPrefab;
    [SerializeField] GridNode walkablePrefab;
    [SerializeField] Dictionary<Vector2, GridNode> spawnedNodes = new Dictionary<Vector2, GridNode>();
    Grid grid;

    [Header("Player")]
    [SerializeField] CharacterData playerCharData;
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab, spawnedPlayerSpawnPoint;
    Vector2 playerSpawnCoords = Vector2.zero;

    [Header("NPCs")]
    [SerializeField] NPCSpawnPoint NPCSpawnPointPrefab;
    [SerializeField] List<NPCSpawnPoint> spawnedNPCSpawnPoints = new List<NPCSpawnPoint>();
    List<Vector2> NPCSpawnCoords = new List<Vector2>();

    [Header("World Items")]
    [SerializeField] ItemDataContainer itemDataContainer;
    [SerializeField] WorldItem worldItemPrefab;
    [SerializeField] Transform worldItemsParent;

    [Header("Spawn Offsets")]
    [SerializeField] Vector3 canteredEntitySpawnOffset;
    [SerializeField] Vector3 worldItemSpawnOffset;

    [SerializeField] int currentLevelIndex;

    private void Awake()
    {
        Instance = this;
        grid = GetComponent<Grid>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Level currentLevel = project.Json.FromJson.Levels[currentLevelIndex];
        int index = 0;
        GridNode clone = null;
        Vector2 spawnCoords = Vector2.zero;


        for (int i = 0; i < currentLevel.LayerInstances[0].CWid; i++)
        {
            for (int j = 0; j < currentLevel.LayerInstances[0].CHei; j++)
            {

                //Spawn tiles
                switch (currentLevel.LayerInstances[1].IntGridCsv[index])
                {
                    case 1:
                        clone = Instantiate(wallPrefab, grid.GetCellCenterLocal(new Vector3Int(i, j)), Quaternion.identity, transform);
                        spawnCoords = new Vector2(i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(i, j) });
                        spawnedNodes.Add(spawnCoords, clone);

                        break;
                    case 2:
                        clone = Instantiate(walkablePrefab, grid.GetCellCenterLocal(new Vector3Int(i, j)), Quaternion.identity, transform);
                        spawnCoords = new Vector2(i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(i, j) });
                        spawnedNodes.Add(spawnCoords, clone);
                        break;
                }

                //Spawn Entities
                for (int k = 0; k < currentLevel.LayerInstances[0].EntityInstances.Length; k++)
                {
                    if (currentLevel.LayerInstances[0].EntityInstances[k].Grid[1] == i && project.Json.FromJson.Levels[0].LayerInstances[0].EntityInstances[k].Grid[0] == j)
                    {
                        GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                        switch (currentLevel.LayerInstances[0].EntityInstances[k].Identifier)
                        {
                            case "Player_Start":
                                PlayerSpawnPoint playerSpawnPointClone = Instantiate(playerSpawnPointPrefab, spawnNode.transform.position + canteredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(currentLevel.LayerInstances[0].EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                spawnedPlayerSpawnPoint = playerSpawnPointClone;
                                playerSpawnCoords = spawnCoords;
                                break;
                            case "NPC_Spawn":
                                NPCSpawnPoint NPCSpawnPonintClone = Instantiate(NPCSpawnPointPrefab, spawnNode.transform.position + canteredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(currentLevel.LayerInstances[0].EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                spawnedNPCSpawnPoints.Add(NPCSpawnPonintClone);
                                NPCSpawnCoords.Add(spawnCoords);
                                break;
                            case "WorldItem":
                                WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + canteredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(currentLevel.LayerInstances[0].EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                ItemData worldItemData = itemDataContainer.GetDataWithIdentifier(currentLevel.LayerInstances[0].EntityInstances[k].FieldInstances[1].Value.ToString());
                                spawnedWorldItem.InitWorldItem(new ItemStack(worldItemData, Convert.ToInt32(currentLevel.LayerInstances[0].EntityInstances[k].FieldInstances[2].Value), Convert.ToInt32(currentLevel.LayerInstances[0].EntityInstances[k].FieldInstances[3].Value)));
                                break;
                        }

                    }
                }
                index++;
            }
        }

        foreach (var node in spawnedNodes.Values)
        {
            node.CacheNeighbours();
        }


        //Spawn player and entities
        spawnedPlayerSpawnPoint.SpawnPlayer(playerCharData, GetNodeAtCoords(playerSpawnCoords));

        for (int i = 0; i < spawnedNPCSpawnPoints.Count; i++)
        {
            spawnedNPCSpawnPoints[i].SpawnEnemy(GetNodeAtCoords(NPCSpawnCoords[i]));
        }
    }

    public GridNode GetNodeAtCoords(Vector2 coords) => spawnedNodes.TryGetValue(coords, out var node) ? node : null;

    public GridNode GetNodeFromWorldPos(Vector3 worldPos)
    {
        return GetNodeAtCoords(new Vector2(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y));
    }

    public struct SquareCoords : ICoords
    {
        public float GetDistance(ICoords other)
        {
            var dist = new Vector2Int(Mathf.Abs((int)Pos.x - (int)other.Pos.x), Mathf.Abs((int)Pos.y - (int)other.Pos.y));

            var lowest = Mathf.Min(dist.x, dist.y);
            var highest = Mathf.Max(dist.x, dist.y);

            var horizontalMovesRequired = highest - lowest;

            return lowest * 14 + horizontalMovesRequired * 10;
        }

        public Vector2 Pos { get; set; }
    }

    public float DecideSpawnDir(string dir)
    {
        float returnDir = 0;
        switch (dir)
        {
            case "North":
                returnDir = 0;
                break;
            case "East":
                returnDir = 90;
                break;
            case "South":
                returnDir = 180;
                break;
            case "West":
                returnDir = 270;
                break;
        }
        return returnDir;
    }
}
