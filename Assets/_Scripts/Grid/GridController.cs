using LDtkUnity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public struct LevelSaveData
{
    public int currentLevelIndex;
    public List<SaveableLevelData> levels;
}

[System.Serializable]
public class NPCSpawnData
{
    public NPCSpawnPoint spawnPoint;
    public Vector2 spawnCoords;
    public NPCData spawnData;

    public NPCSpawnData (NPCSpawnPoint spawnPoint, Vector2 spawnCoords, NPCData spawnData)
    {
        this.spawnPoint = spawnPoint;
        this.spawnCoords = spawnCoords;
        this.spawnData = spawnData;
    }
}

[System.Serializable]
public class SaveableLevelData
{
    [System.Serializable]
    public class TriggerableSaveData
    {
        public bool isTriggered;
        public int currentNumberOfTriggers;

        public TriggerableSaveData(bool isTriggered, int currentNumberOfTriggers)
        {
            this.isTriggered = isTriggered;
            this.currentNumberOfTriggers = currentNumberOfTriggers;
        }
    }

    [System.Serializable]
    public class WorldItemSaveData
    {
        public Vector2 coords;
        public float rotation;
        public ItemStack itemStack;

        public WorldItemSaveData(Vector2 coords, float rotation, ItemStack itemStack)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.itemStack = itemStack;
        }
    }

    [System.Serializable]
    public class ContainerSaveData
    {
        public Vector2 coords;
        public List<ContainerItemStack> containedItemStacks;

        public ContainerSaveData(Vector2 coords, List<ContainerItemStack> containedItemStacks)
        {
            this.coords = coords;
            this.containedItemStacks = containedItemStacks;
        }
    }

    [System.Serializable]
    public class NPCSaveData
    {
        public Vector2 coords;
        public float rotation;
        public int currentHealth;
        public NPCData npcData;

        public NPCSaveData(Vector2 coords, float rotation, int currentHealth, NPCData npcData)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.currentHealth = currentHealth;
            this.npcData = npcData;
        }
    }

    public int levelIndex;
    public List<bool> interactableActivationStates = new List<bool>();
    public List<TriggerableSaveData> triggerableSaveData = new List<TriggerableSaveData>();
    public List<WorldItemSaveData> worldItems = new List<WorldItemSaveData>();
    public List<ContainerSaveData> containers = new List<ContainerSaveData>();
    public List<NPCSaveData> spawnedNPCs = new List<NPCSaveData>();

    public SaveableLevelData(int levelIndex, List<bool> interactableActivationStates, List<TriggerableSaveData> triggerableSaveData, List<WorldItemSaveData> worldItems, List<ContainerSaveData> containers, List<NPCSaveData> NPCs)
    {
        this.levelIndex = levelIndex;
        this.interactableActivationStates = new List<bool>(interactableActivationStates);
        this.triggerableSaveData = new List<TriggerableSaveData>(triggerableSaveData);
        this.worldItems = new List<WorldItemSaveData>(worldItems);
        this.containers = new List<ContainerSaveData>(containers);
        this.spawnedNPCs = new List<NPCSaveData>(NPCs);
    }
}

[System.Serializable]
public class LevelData
{
    public Dictionary<Vector2, GridNode> levelNodes = new Dictionary<Vector2, GridNode>();
    public List<NPCController> spawnedNPCs = new List<NPCController>();

    public LevelData(Dictionary<Vector2, GridNode> levelNodes, List<NPCController> spawnedNPCs)
    {
        this.levelNodes = new Dictionary<Vector2, GridNode>(levelNodes);
        this.spawnedNPCs = new List<NPCController>(spawnedNPCs);
    }

    public void UpdateLevelData(Dictionary<Vector2, GridNode> updatedNodes, List<NPCController> updatedNPCs)
    {
        levelNodes.Clear();
        levelNodes = new Dictionary<Vector2, GridNode>(updatedNodes);

        spawnedNPCs.Clear();
        spawnedNPCs = new List<NPCController>(updatedNPCs);
    }

    public Dictionary<Vector2, GridNode> GetNodes()
    {
        return levelNodes;
    }

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
    List<Level> levels = new List<Level>();
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
    /// <summary>
    /// int = levelIndex
    /// </summary>
    Dictionary<int, LevelData> levelDataDictionary = new Dictionary<int, LevelData>();


    [Header("Player")]
    [SerializeField] CharacterData playerCharData;
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab, spawnedPlayerSpawnPoint;
    public PlayerController playerController;
    Vector2 playerSpawnCoords = Vector2.zero;

    [Header("NPCs")]
    [SerializeField] NPCController npcPrefab;
    //[SerializeField] NPCSpawnPoint NPCSpawnPointPrefab;
    //[SerializeField] List<NPCSpawnData> NPCSpawnData = new List<NPCSpawnData>();
    [SerializeField] List<NPCController> spawnedNPCs = new List<NPCController>();
    [SerializeField] List<NPCController> activeNPCs = new List<NPCController>();
    [SerializeField] NPCDataContainer NPCDataContainer;

    [Header("World Items")]
    [SerializeField] WorldItem worldItemPrefab;
    [SerializeField] ItemDataContainer itemDataContainer;
    [SerializeField] List<WorldItem> spawnedWorldItems;

    [Header("Level Transitions")]
    [SerializeField] LevelTransition levelTransitionPrefab;

    [Header("Containers")]
    [SerializeField] Container largeContainerPrefab;
    [SerializeField] List<IContainer> spawnedContainers = new List<IContainer>();

    [Header("Interactables")]
    [SerializeField] Lever leverPrefab;
    [SerializeField] KeycardReader keycardReaderPrefab;
    [SerializeField] PressurePlate pressurePlatePrefab;
    List<IInteractable> spawnedInteractables = new List<IInteractable>();

    [Header("Triggerables")]
    [SerializeField] Door doorPrefab;
    [SerializeField] Door secretDoorPrefab;
    List<ITriggerable> spawnedTriggerables = new List<ITriggerable>();


    [Header("Spawn Offsets")]
    [SerializeField] Vector3 centeredEntitySpawnOffset;
    [SerializeField] Vector3 worldItemSpawnOffset;

    private void OnEnable()
    {
        NPCController.onNPCDeath += OnNPCDeath;

        PlayerController.onPlayerDeath += OnPlayerDeath;

        //Needs changed 
        WorldItem.onWorldItemPickedUp += OnWorldItemPickedUp;
        WorldItem.onWorldItemGrabbed += OnWorldItemPickedUp;
    }

    private void OnDisable()
    {
        NPCController.onNPCDeath -= OnNPCDeath;

        PlayerController.onPlayerDeath -= OnPlayerDeath;

        WorldItem.onWorldItemPickedUp -= OnWorldItemPickedUp;
        WorldItem.onWorldItemGrabbed -= OnWorldItemPickedUp;
    }

    private void Awake()
    {
        Instance = this;
        grid = GetComponent<Grid>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetLevels();

        InstantiateLevels();

        LoadLevel(startingLevelIndex);

        SpawnPlayer();

        //InstantiateLevel(currentLevelIndex);
    }

    void InstantiateLevels()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            InstantiateLevel(i);
            SaveLevel(i);
            UnloadCurrentLevel();
        }
    }

    void GetLevels()
    {
        levels.Clear();
        foreach (Level level in project.Json.FromJson.Levels)
        {
            levels.Add(level);
        }
    }

    void OnPlayerDeath()
    {
        RestartLevel();
    }

    void OnWorldItemPickedUp(WorldItem grabbedItem)
    {
        if(spawnedWorldItems.Contains(grabbedItem))
            spawnedWorldItems.Remove(grabbedItem);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }
        else if(Input.GetKeyDown(KeyCode.F6))
        {
            QuickLoad();
        }
    }

    public void QuickSave()
    {
        SaveSystem.Save();
    }

    public void QuickLoad()
    {
        SaveSystem.Load();
    }


    private void RestartLevel()
    {
        //UnloadCurrentLevel();
        //if (levels.TryGetValue(currentLevelIndex, out LevelData level))
        //{
        //    LoadLevel(level);
        //}
        //else
        //    InstantiateLevel(currentLevelIndex);
    }

    void OnNPCDeath(NPCController deadNPC)
    {
        if(spawnedNPCs.Contains(deadNPC))
            spawnedNPCs.Remove(deadNPC);

        if(activeNPCs.Contains(deadNPC))
            activeNPCs.Remove(deadNPC);
    }

    public async Task BeginLevelTransition(int levelIndex, Vector2 playerMoveToCoords)
    {
        SaveLevel(currentLevelIndex);
        UnloadCurrentLevel();

        LoadLevel(levelIndex);

        MovePlayer(playerMoveToCoords);

        await Task.Yield();
    }

    private void MovePlayer(Vector2 coordsToMoveTo)
    {
        playerController.MoveToCoords(coordsToMoveTo);
    }

    void SaveLevel(int indexOfLevelToSave)
    {
        //Debug.Log("Saving level " + indexOfLevelToSave + " ...");

        if (levelDataDictionary.TryGetValue(indexOfLevelToSave, out LevelData level))
        {
            level.UpdateLevelData(activeNodes, activeNPCs);
            //Debug.Log("Updated level " + indexOfLevelToSave + " data ...");
            return;
        }

        List<NPCController> npcsToSave = new List<NPCController>();
        foreach (NPCController npc in spawnedNPCs)
        {
            if(npc.levelIndex ==  indexOfLevelToSave)
                npcsToSave.Add(npc);

        }

        levelDataDictionary.Add(indexOfLevelToSave, new LevelData(activeNodes, npcsToSave));
        //Debug.Log("Added level "+ indexOfLevelToSave + " to levels list");
    }

    void LoadLevel(int indexOfLevelToLoad)
    {
        if (!levelDataDictionary.TryGetValue(indexOfLevelToLoad, out LevelData levelData))
            return;

        currentLevelIndex = indexOfLevelToLoad;

        Debug.Log("Loading level: " + indexOfLevelToLoad);
        foreach(GridNode node in levelData.GetNodes().Values)
        {
            node.SetActive(true);
            activeNodes.Add(node.Coords.Pos, node);
        }

        foreach(NPCController NPC in levelData.GetNPCs())
        {
            NPC.SetActive(true);
            activeNPCs.Add(NPC);
        }
    }

    void InstantiateLevel(int levelIndex)
    {
        entityLayer = levels[levelIndex].LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = levels[levelIndex].LayerInstances[INTGRID_LAYER_INDEX];

        SpawnGridNodes(levelIndex);
        LinkInteractablesToTriggerables();
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

        //NPCSpawnData.Clear();
        //spawnedNPCSpawnPoints.Clear();
        //NPCSpawnCoords.Clear();

        if (spawnedPlayerSpawnPoint)
            Destroy(spawnedPlayerSpawnPoint);

        foreach (GridNode node in activeNodes.Values)
        {
            node.SetActive(false);
            //Destroy(node.gameObject);
        }
        activeNodes.Clear();
    }

    private async void SpawnGridNodes(int levelIndex)
    {
        int index = 0;
        Vector2 spawnCoords = Vector2.zero;
        GameObject levelParent = new GameObject($"Level {levelIndex}");
        Transform levelParentTransform = levelParent.transform;
        levelParentTransform.SetParent(transform);

        for (int i = 0; i < intGridLayer.CWid; i++)
        {
            for (int j = 0; j < intGridLayer.CHei; j++)
            {
                GridNode clone;
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
                        break; //Spawn Walls
                    case 2:
                        clone = Instantiate(walkablePrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, levelParentTransform);
                        spawnCoords = new Vector2(-i, j);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(-i, j) });
                        activeNodes.Add(spawnCoords, clone);
                        break; //Spawn Walkables
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
                                NPCController NPCClone = Instantiate(npcPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                NPCData spawnData = GetNPCData(entityLayer.EntityInstances[k].FieldInstances[1].Value);
                                NPCClone.InitNPC(levelIndex, spawnData, spawnNode);
                                NPCClone.SetActive(false);
                                spawnedNPCs.Add(NPCClone);
                                //NPCSpawnPoint NPCSpawnPointClone = Instantiate(NPCSpawnPointPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                //NPCSpawnData.Add(new NPCSpawnData(NPCSpawnPointClone, spawnCoords, spawnData));
                                //spawnedNPCSpawnPoints.Add(NPCSpawnPointClone);
                                //NPCSpawnCoords.Add(spawnCoords);
                                break;
                            case "WorldItem":
                                WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                spawnedWorldItems.Add(spawnedWorldItem);
                                ItemData worldItemData = itemDataContainer.GetDataFromIdentifier(entityLayer.EntityInstances[k].FieldInstances[1].Value.ToString());
                                spawnedWorldItem.InitWorldItem(levelIndex, spawnCoords, new ItemStack(worldItemData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value), Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value)));
                                break;
                            case "Level_Transition":
                                LevelTransition spawnedLevelTransition = Instantiate(levelTransitionPrefab, spawnNode.transform.position + centeredEntitySpawnOffset + new Vector3(0,1.5f,0), Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                int levelIndexToGoTo = Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[1].Value);
                                List<object> levelCoords = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                spawnedLevelTransition.InitLevelTransition(levelIndexToGoTo, new Vector2(-Convert.ToInt32(levelCoords[1]), Convert.ToInt32(levelCoords[0])));
                                break;
                            case "Container":
                                IContainer spawnedContainer = null;
                                switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                                {
                                    case "Large":
                                        spawnedContainer = Instantiate(largeContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        List<object> itemNames = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                        List<object> itemAmounts = (List<object>)entityLayer.EntityInstances[k].FieldInstances[3].Value;
                                        for (int l = 0; l < itemNames.Count; l++)
                                        {
                                            ItemData itemData = itemDataContainer.GetDataFromIdentifier(itemNames[l].ToString());
                                            int itemAmount = Convert.ToInt32(itemAmounts[l]);
                                            spawnedContainer.AddNewStoredItem(l, new ItemStack(itemData, itemAmount));

                                        }
                                        break;
                                }
                                spawnedContainer.InitContainer(levelIndex, spawnCoords);
                                spawnedContainers.Add(spawnedContainer);

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
                                        spawnedLever.SetLevelIndex(levelIndex);
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
                                        spawnedKeycardReader.SetLevelIndex(levelIndex);
                                        spawnedInteractables.Add(spawnedKeycardReader);
                                        break;
                                }
                                break;
                            case "Triggerable":
                                Door spawnedDoor = null;
                                switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                                {
                                    case "Door":
                                        spawnedDoor = Instantiate(doorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        break;
                                    case "Secret_Door":
                                        spawnedDoor = Instantiate(secretDoorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                        break;
                                }
                                spawnedDoor.SetOccupyingNode(spawnNode);
                                spawnedDoor.SetEntityRef(entityLayer.EntityInstances[k].Iid);
                                spawnedDoor.SetRequiredNumberOfTriggers(Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value));
                                spawnedDoor.SetLevelIndex(levelIndex);
                                spawnNode.SetOccupant(new GridNodeOccupant(spawnedDoor.gameObject, GridNodeOccupantType.Obstacle));
                                spawnedTriggerables.Add(spawnedDoor);
                                break;
                        }
                    }
                }
                index++;
                await Task.Delay(0);
            }
        }
    }

    private NPCData GetNPCData(object value)
    {
        string npcDataIdentifier = value.ToString();
        //Debug.Log($"Trying to spawn: {npcDataIdentifier}");
        return NPCDataContainer.GetDataFromIdentifier(npcDataIdentifier);
    }

    private void CacheGridNodeNeighbours()
    {
        foreach (var node in activeNodes.Values)
        {
            node.CacheNeighbours();
        }
    }

    //private void SetLevelNPCsActive(int levelIndex)
    //{
    //    foreach (NPCController npc in spawnedNPCs)
    //    {
    //        if(npc.levelIndex == levelIndex)
    //        {
    //            npc.SetActive(true);
    //            activeNPCs.Add(npc);
    //        }
    //    }
    //}

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

    public string GetLevelNameFromIndex(int levelIndex)
    {
        return project.Json.FromJson.Levels[levelIndex].FieldInstances[0].Value.ToString();
    }

    private List<bool> GetInteractableActivationStatesInLevel(int levelIndex)
    {
        List<bool> interactableActivationStates = new List<bool>();
        foreach (IInteractable interactable in spawnedInteractables)
        {
            if(interactable.GetLevelIndex() == levelIndex)
                interactableActivationStates.Add(interactable.GetIsActivated());
        }

        return interactableActivationStates;
    }

    private List<SaveableLevelData> GetSaveableLevelData()
    {
        List<SaveableLevelData> saveableLevelDatas = new List<SaveableLevelData>();
        foreach (int levelIndex in levelDataDictionary.Keys)
        {
            saveableLevelDatas.Add(new SaveableLevelData(
                levelIndex,
                GetInteractableActivationStatesInLevel(levelIndex),
                GetTriggerableSaveData(levelIndex),
                GetWorldItemSaveData(levelIndex),
                GetContainerSaveData(levelIndex),
                GetNPCSaveData(levelIndex)
            ));
        }

        return saveableLevelDatas;
    }

    private List<SaveableLevelData.NPCSaveData> GetNPCSaveData(int levelIndex)
    {
        List<SaveableLevelData.NPCSaveData> NPCSaveData = new List<SaveableLevelData.NPCSaveData>();
        foreach (NPCController npc in spawnedNPCs)
        {
            if(npc.levelIndex == levelIndex)
                NPCSaveData.Add(new SaveableLevelData.NPCSaveData(npc.currentlyOccupiedGridnode.Coords.Pos, npc.transform.localRotation.eulerAngles.y, Mathf.RoundToInt(npc.currentGroupHealth), npc.NPCData));
        }

        return NPCSaveData;
    }

    private List<SaveableLevelData.ContainerSaveData> GetContainerSaveData(int levelIndex)
    {
        List<SaveableLevelData.ContainerSaveData> containerSaveData = new List<SaveableLevelData.ContainerSaveData>();
        foreach (IContainer container in spawnedContainers)
        {
            if (container.GetLevelIndex() == levelIndex)
                containerSaveData.Add(new SaveableLevelData.ContainerSaveData(container.GetCoords(), container.GetStoredItems()));
        }

        return containerSaveData;
    }

    private List<SaveableLevelData.WorldItemSaveData> GetWorldItemSaveData(int levelIndex)
    {
        List<SaveableLevelData.WorldItemSaveData> worldItemSaveData = new List<SaveableLevelData.WorldItemSaveData>();
        foreach (WorldItem worldItem in spawnedWorldItems)
        {
            if(worldItem.levelIndex == levelIndex)
                worldItemSaveData.Add(new SaveableLevelData.WorldItemSaveData(worldItem.coords, worldItem.transform.rotation.eulerAngles.y, worldItem.item));
        }

        return worldItemSaveData;
    }
    private List<SaveableLevelData.TriggerableSaveData> GetTriggerableSaveData(int levelIndex)
    {
        List<SaveableLevelData.TriggerableSaveData> triggerableSaveData = new List<SaveableLevelData.TriggerableSaveData>();
        foreach (ITriggerable triggerable in spawnedTriggerables)
        {
            if (triggerable.GetLevelIndex() == levelIndex)
                triggerableSaveData.Add(new SaveableLevelData.TriggerableSaveData(triggerable.GetIsTriggered(), triggerable.GetCurrentNumberOfTriggers()));
        }

        return triggerableSaveData;
    }

    public void Save(ref LevelSaveData data)
    {
        data.currentLevelIndex = currentLevelIndex;
        data.levels = GetSaveableLevelData();
    }

    public void Load(LevelSaveData data)
    {
        //InstantiateLevels
        //Set triggerable states to saved state
        //Set interactable states to saved state
        //Spawn WorldItems
        //Spawn and populate Containers
        //Spawn saved NPCs

        LoadLevel(data.currentLevelIndex);
    }

}
