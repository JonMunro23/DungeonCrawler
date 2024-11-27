using LDtkUnity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance;

    const int ENTITY_LAYER_INDEX = 0;
    const int INTGRID_LAYER_INDEX = 1;

    [SerializeField] LDtkComponentProject project;
    Level currentLevel;
    LayerInstance entityLayer;
    LayerInstance intGridLayer;

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

    [Header("Containers")]
    [SerializeField] Container largeContainerPrefab;

    [Header("Interactables")]
    [SerializeField] Lever leverPrefab;
    [SerializeField] KeycardReader keycardReaderPrefab;
    [SerializeField] PressurePlate pressurePlatePrefab;
    List<IInteractable> spawnedInteractables = new List<IInteractable>();

    [Header("Triggerables")]
    [SerializeField] Door doorPrefab;
    List<ITriggerable> spawnedTriggerables = new List<ITriggerable>();


    [Header("Spawn Offsets")]
    [SerializeField] Vector3 centeredEntitySpawnOffset;
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
        currentLevel = project.Json.FromJson.Levels[currentLevelIndex];
        entityLayer = currentLevel.LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = currentLevel.LayerInstances[INTGRID_LAYER_INDEX];
        SpawnGridNodes();
    }

    private async void SpawnGridNodes()
    {
        int index = 0;
        GridNode clone = null;
        Vector2 spawnCoords = Vector2.zero;

        for (int i = 0; i < intGridLayer.CWid; i++)
        {
            for (int j = 0; j < intGridLayer.CHei; j++)
            {
                //Spawn tiles
                //i index is reversed to match orientation in LDtk
                switch (intGridLayer.IntGridCsv[index])
                {
                    case 1:
                        clone = Instantiate(wallPrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, transform);
                        clone.transform.localPosition += new Vector3(-1.5f, 1.5f, -1.5f);
                        spawnCoords = new Vector2(-i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(-i, j) });
                        spawnedNodes.Add(spawnCoords, clone);

                        break;
                    case 2:
                        clone = Instantiate(walkablePrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, transform);
                        spawnCoords = new Vector2(-i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(-i, j) });
                        spawnedNodes.Add(spawnCoords, clone);
                        break;
                }

                //Spawn Entities
                for (int k = 0; k < entityLayer.EntityInstances.Length; k++)
                {
                    if (entityLayer.EntityInstances[k].Grid[1] == i && entityLayer.EntityInstances[k].Grid[0] == j)
                    {
                        GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                        switch (entityLayer.EntityInstances[k].Identifier)
                        {
                            case "Player_Start":
                                PlayerSpawnPoint playerSpawnPointClone = Instantiate(playerSpawnPointPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                spawnedPlayerSpawnPoint = playerSpawnPointClone;
                                playerSpawnCoords = spawnCoords;
                                break;
                            case "NPC_Spawn":
                                NPCSpawnPoint NPCSpawnPonintClone = Instantiate(NPCSpawnPointPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                spawnedNPCSpawnPoints.Add(NPCSpawnPonintClone);
                                NPCSpawnCoords.Add(spawnCoords);
                                break;
                            case "WorldItem":
                                WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                ItemData worldItemData = itemDataContainer.GetDataFromIdentifier(entityLayer.EntityInstances[k].FieldInstances[1].Value.ToString());
                                spawnedWorldItem.InitWorldItem(new ItemStack(worldItemData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value), Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value)));
                                break;
                            case "Container":

                                switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                                {
                                    case "Large":
                                        IContainer spawnedContainer = Instantiate(largeContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        List<object> itemNames = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                        for (int l = 0; l < itemNames.Count; l++)
                                        {
                                            ItemData itemData = itemDataContainer.GetDataFromIdentifier(itemNames[l].ToString());
                                            List<object> itemAmounts = (List<object>)entityLayer.EntityInstances[k].FieldInstances[3].Value;
                                            int itemAmount = Convert.ToInt32(itemAmounts[l]);
                                            spawnedContainer.AddNewStoredItem(new ItemStack(itemData, itemAmount));

                                        }
                                        spawnedContainer.InitContainer();
                                        break;
                                }

                                break;
                            case "Interactable":

                                List<object> entityRefsToTrigger = new List<object>();

                                switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                                {
                                    case "Lever":
                                        //spawn lever
                                        Lever spawnedLever = Instantiate(leverPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        entityRefsToTrigger = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                        foreach (object entityRef in entityRefsToTrigger)
                                        {
                                            spawnedLever.AddEntityRefToTrigger((Dictionary<string, object>)entityRef);
                                        }
                                        spawnedInteractables.Add(spawnedLever);
                                        break;
                                    case "Keycard_Reader":
                                        //spawn keycard reader
                                        KeycardReader spawnedKeycardReader = Instantiate(keycardReaderPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        spawnedKeycardReader.SetRequiredKeycardType((string)entityLayer.EntityInstances[k].FieldInstances[3].Value);
                                        entityRefsToTrigger = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                        foreach (object entityRef in entityRefsToTrigger)
                                        {
                                            spawnedKeycardReader.AddEntityRefToTrigger((Dictionary<string, object>)entityRef);
                                        }
                                        spawnedInteractables.Add(spawnedKeycardReader);
                                        break;
                                }
                                break;
                            case "Triggerable":
                                switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                                {
                                    case "Door":
                                        //spawn door
                                        Door spawnedDoor = Instantiate(doorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        spawnedDoor.SetOccupyingNode(spawnNode);
                                        spawnedDoor.SetEntityRef(entityLayer.EntityInstances[k].Iid);
                                        spawnedDoor.SetRequiredNumberOfTriggers(Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value));
                                        spawnedTriggerables.Add(spawnedDoor);
                                        break;
                                }
                                break;
                        }

                    }
                }
                index++;
                await Task.Delay(0);
            }
        }

        CacheGridNodeNeighbours();

        SpawnPlayer();
        SpawnNPCs();
        LinkInteractablesToTriggerables();
    }

    private void CacheGridNodeNeighbours()
    {
        foreach (var node in spawnedNodes.Values)
        {
            node.CacheNeighbours();
        }
    }

    private void SpawnNPCs()
    {
        for (int i = 0; i < spawnedNPCSpawnPoints.Count; i++)
        {
            spawnedNPCSpawnPoints[i].SpawnEnemy(GetNodeAtCoords(NPCSpawnCoords[i]));
        }
    }

    private void SpawnPlayer()
    {
        spawnedPlayerSpawnPoint.SpawnPlayer(playerCharData, GetNodeAtCoords(playerSpawnCoords));
    }

    void LinkInteractablesToTriggerables()
    {
        foreach (IInteractable interactable in spawnedInteractables)
        {
            foreach (string entityRef in interactable.GetEntityRefsToTrigger())
            {
                foreach (ITriggerable triggerable in spawnedTriggerables)
                {
                    if (triggerable.GetEntityRef() == entityRef)
                    {
                        interactable.AddObjectToTrigger(triggerable);
                    }
                }
            }
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
