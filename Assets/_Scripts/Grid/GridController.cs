using LDtkUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LevelData
{
    public int levelIndex;
    Dictionary<Vector2, GridNode> levelNodes = new Dictionary<Vector2, GridNode>();
    List<ITriggerable> spawnedTriggerables = new List<ITriggerable>();
    List<IInteractable> spawnedInteractables = new List<IInteractable>();
    List<NPCController> spawnedNPCs = new List<NPCController>();


    public LevelData(Dictionary<Vector2, GridNode> levelNodes, List<ITriggerable> spawnedTriggerables, List<IInteractable> spawnedInteractables, List<NPCController> spawnedNPCs)
    {
        this.levelNodes = new Dictionary<Vector2, GridNode>(levelNodes);
        this.spawnedNPCs = new List<NPCController>(spawnedNPCs);
        //this.spawnedTriggerables = spawnedTriggerables;
        //this.spawnedInteractables = spawnedInteractables;
    }

    public void UpdateLevelData(Dictionary<Vector2, GridNode> updatedNodes,List<ITriggerable> updatedTriggerables, List<IInteractable> updatedInteractables, List<NPCController> updatedNPCs)
    {
        levelNodes.Clear();
        levelNodes = new Dictionary<Vector2, GridNode>(updatedNodes);

        spawnedNPCs.Clear();
        spawnedNPCs = new List<NPCController>(updatedNPCs);


        //spawnedTriggerables.Clear();
        //spawnedTriggerables = updatedTriggerables;

        //spawnedInteractables.Clear();
        //spawnedInteractables = updatedInteractables;

    }

    public Dictionary<Vector2, GridNode> GetNodes()
    {
        return levelNodes;
    }

    //public List<ITriggerable> GetTriggerables()
    //{
    //    return spawnedTriggerables;
    //}

    //public List<IInteractable> GetInteractables()
    //{
    //    return spawnedInteractables;
    //}

    public List<NPCController> GetNPCs()
    {
        return spawnedNPCs;
    }
}

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
    [SerializeField] Dictionary<Vector2, GridNode> activeNodes = new Dictionary<Vector2, GridNode>();
    Grid grid;

    [Header("Levels")]
    [SerializeField] int startingLevelIndex;
    [SerializeField] int currentLevelIndex;
    Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();

    [Header("Player")]
    [SerializeField] CharacterData playerCharData;
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab, spawnedPlayerSpawnPoint;
    [SerializeField] PlayerController playerController;
    Vector2 playerSpawnCoords = Vector2.zero;

    [Header("NPCs")]
    [SerializeField] NPCSpawnPoint NPCSpawnPointPrefab;
    [SerializeField] List<NPCSpawnPoint> spawnedNPCSpawnPoints = new List<NPCSpawnPoint>();
    [SerializeField] List<NPCController> activeNPCs = new List<NPCController>();
    List<Vector2> NPCSpawnCoords = new List<Vector2>();

    [Header("World Items")]
    [SerializeField] WorldItem worldItemPrefab;
    [SerializeField] ItemDataContainer itemDataContainer;
    //[SerializeField] Transform worldItemsParent;

    [Header("Level Transitions")]
    [SerializeField] LevelTransition levelTransitionPrefab;

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

    private void OnEnable()
    {
        LevelTransition.onLevelTransitionEntered += OnLevelTransitionEntered;
    }

    private void OnDisable()
    {
        LevelTransition.onLevelTransitionEntered -= OnLevelTransitionEntered;
    }

    private void Awake()
    {
        Instance = this;
        grid = GetComponent<Grid>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentLevelIndex = startingLevelIndex;
        InstantiateLevel(currentLevelIndex);
    }

    void OnLevelTransitionEntered(int levelIndex, Vector2 playerMoveToCoords)
    {
        SaveCurrentLevel();
        UnloadCurrentLevel();

        currentLevelIndex = levelIndex;

        if (levels.TryGetValue(levelIndex, out LevelData level))
        {
            LoadLevel(level);
        }
        else
            InstantiateLevel(levelIndex);


        MovePlayer(playerMoveToCoords);
    }

    private void MovePlayer(Vector2 coordsToMoveTo)
    {
        playerController.MoveToCoords(coordsToMoveTo);
    }

    void SaveCurrentLevel()
    {
        Debug.Log("Saving level " + currentLevelIndex + " ...");

        if (levels.TryGetValue(currentLevelIndex, out LevelData level))
        {
            level.UpdateLevelData(activeNodes, spawnedTriggerables, spawnedInteractables, activeNPCs);
            Debug.Log("Updated level " + level.levelIndex + " data ...");
            return;
        }

        levels.Add(currentLevelIndex, new LevelData(activeNodes, spawnedTriggerables, spawnedInteractables, activeNPCs));
        Debug.Log("Added level "+ currentLevelIndex +" to levels list");
    }

    void LoadLevel(LevelData levelData)
    {
        Debug.Log("Loading level: " +  levelData.levelIndex);
        foreach(GridNode node in levelData.GetNodes().Values)
        {
            //Instantiate(node, grid.GetCellCenterLocal(new Vector3Int((int)node.Coords.Pos.x, (int)node.Coords.Pos.y)), Quaternion.identity, transform);
            node.SetActive(true);
            activeNodes.Add(node.Coords.Pos, node);
        }

        foreach(NPCController NPC in levelData.GetNPCs())
        {
            NPC.SetActive(true);
            activeNPCs.Add(NPC);
        }

        //spawn saved nodes
        //spawn saved npcs
        //link triggerables to interactables
    }

    void InstantiateLevel(int levelIndex)
    {

        currentLevel = project.Json.FromJson.Levels[levelIndex];

        entityLayer = currentLevel.LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = currentLevel.LayerInstances[INTGRID_LAYER_INDEX];

        SpawnGridNodes();

        LinkInteractablesToTriggerables();

        SpawnNPCs();

        SpawnPlayer();

        CacheGridNodeNeighbours();
    }

    void UnloadCurrentLevel()
    {
        foreach (NPCController NPC in activeNPCs)
        {
            NPC.SnapToNode();
            NPC.SetActive(false);
        }
        activeNPCs.Clear();

        spawnedNPCSpawnPoints.Clear();
        NPCSpawnCoords.Clear();

        if (spawnedPlayerSpawnPoint)
            Destroy(spawnedPlayerSpawnPoint);

        foreach (GridNode node in activeNodes.Values)
        {
            node.SetActive(false);
            //Destroy(node.gameObject);
        }
        activeNodes.Clear();
    }

    private async void SpawnGridNodes()
    {
        int index = 0;
        GridNode clone = null;
        Vector2 spawnCoords = Vector2.zero;

        GameObject levelParent = new GameObject($"Level {currentLevelIndex}");
        Transform levelParentTransform = levelParent.transform;
        levelParentTransform.SetParent(transform);

        for (int i = 0; i < intGridLayer.CWid; i++)
        {
            for (int j = 0; j < intGridLayer.CHei; j++)
            {
                //Spawn tiles
                //i index is reversed to match orientation in LDtk
                switch (intGridLayer.IntGridCsv[index])
                {
                    case 1:
                        clone = Instantiate(wallPrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, levelParentTransform);
                        clone.transform.localPosition += new Vector3(-1.5f, 1.5f, -1.5f);
                        spawnCoords = new Vector2(-i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(-i, j) });
                        activeNodes.Add(spawnCoords, clone);

                        break;
                    case 2:
                        clone = Instantiate(walkablePrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, levelParentTransform);
                        spawnCoords = new Vector2(-i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(-i, j) });
                        activeNodes.Add(spawnCoords, clone);
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
                            case "Level_Transition":
                                LevelTransition spawnedLevelTransition = Instantiate(levelTransitionPrefab, spawnNode.transform.position + centeredEntitySpawnOffset + new Vector3(0,1.5f,0), Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                int levelIndex = Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[1].Value);
                                List<object> levelCoords = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                spawnedLevelTransition.InitLevelTransition(levelIndex, new Vector2(-Convert.ToInt32(levelCoords[1]), Convert.ToInt32(levelCoords[0])));
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
                                        spawnNode.SetOccupant(new GridNodeOccupant(spawnedDoor.gameObject, GridNodeOccupantType.Obstacle));
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

        
    }

    private void CacheGridNodeNeighbours()
    {
        foreach (var node in activeNodes.Values)
        {
            node.CacheNeighbours();
        }
    }

    private void SpawnNPCs()
    {
        for (int i = 0; i < spawnedNPCSpawnPoints.Count; i++)
        {
            activeNPCs.Add(spawnedNPCSpawnPoints[i].SpawnNPC(GetNodeAtCoords(NPCSpawnCoords[i])));
        }
    }

    private void SpawnPlayer()
    {
        if (playerController)
            return;

        if(spawnedPlayerSpawnPoint)
        {
            playerController = spawnedPlayerSpawnPoint.SpawnPlayer(playerCharData, GetNodeAtCoords(playerSpawnCoords));
        }
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

    public GridNode GetNodeAtCoords(Vector2 coords) => activeNodes.TryGetValue(coords, out var node) ? node : null;

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
