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

    [SerializeField] float gameTime;

    [SerializeField] LDtkComponentProject project;
    List<Level> levels = new List<Level>();
    LayerInstance entityLayer;
    LayerInstance intGridLayer;

    [SerializeField] bool skipMainMenu = true;
    
    [Header("Grid")]
    [SerializeField] GridNode wallPrefab;
    [SerializeField] GridNode walkablePrefab;
    [SerializeField] GridNode voidPrefab;
    [SerializeField] Dictionary<Vector2, GridNode> activeNodes = new Dictionary<Vector2, GridNode>();
    Grid grid;

    [Header("Levels")]
    [SerializeField] int startingLevelIndex;
    [SerializeField] int currentLevelIndex;
    /// <summary>
    /// int = levelIndex
    /// </summary>
    Dictionary<int, LevelData> levelDataDictionary = new Dictionary<int, LevelData>();
    [SerializeField] List<GameObject> levelParents = new List<GameObject>();


    [Header("Player")]
    [SerializeField] PlayerController playerPrefab;
    [SerializeField] CharacterData playerCharData, defaultPlayerCharData;
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab, spawnedPlayerSpawnPoint;
    public PlayerController playerController;
    Vector2 playerSpawnCoords = Vector2.zero;

    [Header("NPCs")]
    [SerializeField] NPCController npcPrefab;
    [SerializeField] List<NPCController> spawnedNPCs = new List<NPCController>();
    [SerializeField] List<NPCController> activeNPCs = new List<NPCController>();
    [SerializeField] NPCDataContainer NPCDataContainer;

    [Header("World Items")]
    [SerializeField] WorldItem worldItemPrefab;
    [SerializeField] ItemDataContainer itemDataContainer;
    [SerializeField] List<WorldItem> spawnedWorldItems;

    [Header("Level Transitions")]
    [SerializeField] LevelTransition levelTransitionPrefab;
    [SerializeField] List<LevelTransition> spawnedLevelTransitions = new List<LevelTransition>();

    [Header("Containers")]
    [SerializeField] Container largeContainerPrefab;
    [SerializeField] List<IContainer> spawnedContainers = new List<IContainer>();

    [Header("Interactables")]
    [SerializeField] Lever leverPrefab;
    [SerializeField] KeycardReader keycardReaderPrefab;
    [SerializeField] PressurePlate pressurePlatePrefab;
    [SerializeField] Tripwire tripwirePrefab;
    [SerializeField] ShootableTarget shootableTargetPrefab;
    List<IInteractable> spawnedInteractables = new List<IInteractable>();

    [Header("Triggerables")]
    [SerializeField] Door doorPrefab;
    [SerializeField] Door secretDoorPrefab;
    List<ITriggerable> spawnedTriggerables = new List<ITriggerable>();


    [Header("Spawn Offsets")]
    [SerializeField] Vector3 centeredEntitySpawnOffset;
    [SerializeField] Vector3 worldItemSpawnOffset;

    public static Action onQuickSave;

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

    private void OnEnable()
    {
        //MainMenu.onNewGameStarted += NewGame;
        SelectableCharacter.OnCharacterSelected += OnCharacterSelected;
        PauseMenu.onQuit += OnQuit;

        NPCController.onNPCDeath += OnNPCDeath;

        PlayerController.onPlayerDeath += OnPlayerDeath;

        //Needs changed 
        WorldItem.onWorldItemPickedUp += OnWorldItemPickedUp;
        WorldItem.onWorldItemGrabbed += OnWorldItemPickedUp;
    }

    private void OnDisable()
    {
        //MainMenu.onNewGameStarted -= NewGame;
        SelectableCharacter.OnCharacterSelected -= OnCharacterSelected;
        PauseMenu.onQuit -= OnQuit;

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

    void OnPlayerDeath()
    {
        RestartLevel();
    }
    void OnWorldItemPickedUp(WorldItem grabbedItem)
    {
        if(spawnedWorldItems.Contains(grabbedItem))
            spawnedWorldItems.Remove(grabbedItem);
    }
    void OnNPCDeath(NPCController deadNPC)
    {
        foreach(LevelData levelData in levelDataDictionary.Values)
        {
            if (levelData.spawnedNPCs.Contains(deadNPC))
                levelData.spawnedNPCs.Remove(deadNPC);
        }

        if(spawnedNPCs.Contains(deadNPC))
            spawnedNPCs.Remove(deadNPC);

        if(activeNPCs.Contains(deadNPC))
            activeNPCs.Remove(deadNPC);
    }

    void GetLevels()
    {
        levels.Clear();
        foreach (Level level in project.Json.FromJson.Levels)
        {
            levels.Add(level);
        }
    }

    public int GetCurrentLevelIndex() => currentLevelIndex;

    public Dictionary<Vector2, GridNode> GetCurrentActiveNodes() => activeNodes;

    // Start is called before the first frame update
    void Start()
    {
        GetLevels();

        if (skipMainMenu)
        {
            playerCharData = defaultPlayerCharData;
            HelperFunctions.SetCursorActive(false);
            NewGame();
        }
    }

    #region QuickSave/Load
    private void Update()
    {
        if(PauseMenu.isPaused || !PlayerController.isPlayerAlive) return;

        gameTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.F5))
        {
            QuickSave();
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            QuickLoad();
        }
    }

    public void QuickSave()
    {
        SaveSystem.Save("Quick Save");
        onQuickSave.Invoke();
    }

    public void QuickLoad()
    {
        SaveSystem.Load("Quick Save");
    }
    #endregion

    #region NewGame
    void OnCharacterSelected(CharacterData charData)
    {
        playerCharData = charData;
        NewGame();
    }

    void NewGame()
    {
        InstantiateLevels();

        SetLevelActive(startingLevelIndex);

        SpawnPlayer();

        MovePlayer(playerSpawnCoords);
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

    void OnQuit()
    {
        foreach (var level in levelParents)
        {
            Destroy(level);
        }
        levelParents.Clear();
        activeNodes.Clear();
        spawnedContainers.Clear();
        spawnedInteractables.Clear();
        spawnedLevelTransitions.Clear();
        spawnedNPCs.Clear();
        activeNPCs.Clear();
        spawnedTriggerables.Clear();
        spawnedWorldItems.Clear();

        levelDataDictionary.Clear();

        playerController.RemoveAudioSources();
        Destroy(playerController.gameObject);
        playerController = null;
    }

    void InstantiateLevel(int levelIndex)
    {
        entityLayer = levels[levelIndex].LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = levels[levelIndex].LayerInstances[INTGRID_LAYER_INDEX];

        GenerateLevel(levelIndex, false);

        LinkInteractablesToTriggerables();
        CacheGridNodeNeighbours();
    }

    void GenerateLevel(int levelIndex, bool isLoaded)
    {
        int index = 0;
        Vector2 spawnCoords = Vector2.zero;
        GameObject levelParent = new GameObject($"Level {levelIndex}");
        levelParents.Add(levelParent);
        Transform levelParentTransform = levelParent.transform;
        levelParentTransform.SetParent(transform);
        GridNodeOccupant newOccupant;
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
                    case 3:
                        clone = Instantiate(voidPrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, levelParentTransform);
                        spawnCoords = new Vector2(-i, j);
                        clone.SetIsVoid(true);
                        clone.InitNode(new SquareCoords { Pos = new Vector2(-i, j) });
                        activeNodes.Add(spawnCoords, clone);
                        break;
                }

                if (!isLoaded)
                {
                    //Spawn Entities
                    for (int k = 0; k < entityLayer.EntityInstances.Length; k++)
                    {
                        if (entityLayer.EntityInstances[k].Grid[1] == i && entityLayer.EntityInstances[k].Grid[0] == j)
                        {
                            GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                            switch (entityLayer.EntityInstances[k].Identifier)
                            {
                                case "Player_Start":
                                    playerSpawnCoords = spawnCoords;
                                    break;
                                case "NPC_Spawn":
                                    NPCController NPCClone = Instantiate(npcPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                    NPCData spawnData = GetNPCData(entityLayer.EntityInstances[k].FieldInstances[1].Value);
                                    NPCClone.InitNPC(levelIndex, spawnData, spawnNode);
                                    NPCClone.SetActive(false);
                                    spawnedNPCs.Add(NPCClone);
                                    break;
                                case "WorldItem":
                                    WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                    spawnedWorldItems.Add(spawnedWorldItem);
                                    ItemData worldItemData = itemDataContainer.GetDataFromIdentifier(entityLayer.EntityInstances[k].FieldInstances[1].Value.ToString());
                                    spawnedWorldItem.InitWorldItem(levelIndex, spawnCoords, new ItemStack(worldItemData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value), Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value)));
                                    break;
                                case "Level_Transition":
                                    LevelTransition spawnedLevelTransition = Instantiate(levelTransitionPrefab, spawnNode.transform.position + centeredEntitySpawnOffset + new Vector3(0, 1.5f, 0), Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                    int levelIndexToGoTo = Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[1].Value);
                                    List<object> levelCoords = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                    spawnedLevelTransition.InitLevelTransition(levelIndexToGoTo, new Vector2(-Convert.ToInt32(levelCoords[1]), Convert.ToInt32(levelCoords[0])));
                                    spawnedLevelTransitions.Add(spawnedLevelTransition);
                                    newOccupant = new GridNodeOccupant(spawnedLevelTransition.gameObject, GridNodeOccupantType.LevelTransition);
                                    spawnNode.SetBaseOccupant(newOccupant);
                                    spawnNode.SetOccupant(newOccupant);
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
                                                spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(itemData, itemAmount)));

                                            }
                                            break;
                                    }
                                    spawnedContainer.InitContainer(levelIndex, spawnCoords);
                                    spawnedContainers.Add(spawnedContainer);

                                    break;
                                case "Interactable":
                                    IInteractable interactable = null;
                                    switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                                    {
                                        case "Lever":
                                            interactable = Instantiate(leverPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                            break;
                                        case "Keycard_Reader":
                                            interactable = Instantiate(keycardReaderPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                            interactable.SetRequiredKeycardType((string)entityLayer.EntityInstances[k].FieldInstances[3].Value);
                                            break;
                                        case "Pressure_Plate":
                                            interactable = Instantiate(pressurePlatePrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()), 0)), spawnNode.transform);
                                            newOccupant = new GridNodeOccupant(interactable.GetGameObject(), GridNodeOccupantType.PressurePlate);
                                            spawnNode.SetBaseOccupant(newOccupant);
                                            spawnNode.SetOccupant(newOccupant);
                                            break;
                                        case "Tripwire":
                                            Tripwire tripwire = Instantiate(tripwirePrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()) + 180, 0)), spawnNode.transform);
                                            tripwire.InitTripwire();
                                            interactable = tripwire;
                                            break;
                                        case "Shootable_Target":
                                            interactable = Instantiate(shootableTargetPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k].FieldInstances[0].Value.ToString()) + 180, 0)), spawnNode.transform);
                                            break;
                                    }

                                    List<object> entityRefsToTrigger = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                                    foreach (object entityRef in entityRefsToTrigger)
                                    {
                                        interactable.AddEntityRefToTrigger((Dictionary<string, object>)entityRef);
                                    }
                                    interactable.SetInteractableType((string)entityLayer.EntityInstances[k].FieldInstances[1].Value);
                                    interactable.SetTriggerOperation((string)entityLayer.EntityInstances[k].FieldInstances[4].Value);
                                    interactable.SetTriggerOnExit((bool)entityLayer.EntityInstances[k].FieldInstances[5].Value);
                                    interactable.SetIsSingleUse((bool)entityLayer.EntityInstances[k].FieldInstances[6].Value);
                                    interactable.SetLevelIndex(levelIndex);
                                    interactable.SetNode(spawnNode);
                                    spawnedInteractables.Add(interactable);
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
                                    newOccupant = new GridNodeOccupant(spawnedDoor.gameObject, GridNodeOccupantType.Obstacle);
                                    spawnNode.SetBaseOccupant(newOccupant);
                                    spawnNode.SetOccupant(newOccupant);
                                    spawnedTriggerables.Add(spawnedDoor);
                                    break;
                                case "NPC_Invis_Wall":
                                    newOccupant = new GridNodeOccupant(null, GridNodeOccupantType.NPCInaccessible);
                                    spawnNode.SetBaseOccupant(newOccupant);
                                    spawnNode.SetOccupant(newOccupant);
                                    break;
                            }
                        }
                    }
                }
                index++;
            }
        }
    }
    #endregion

    #region LoadGame
    private void LoadGame(LevelSaveData data)
    {
        foreach (GameObject levelParent in levelParents)
        {
            Destroy(levelParent);
        }
        levelParents.Clear();

        foreach (LevelData levelData in levelDataDictionary.Values)
        {
            foreach (NPCController NPC in levelData.spawnedNPCs)
            {
                Destroy(NPC.gameObject);
            }
        }
        levelDataDictionary.Clear();

        activeNodes.Clear();
        spawnedNPCs.Clear();
        activeNPCs.Clear();
        spawnedWorldItems.Clear();
        spawnedContainers.Clear();
        spawnedLevelTransitions.Clear();
        spawnedInteractables.Clear();
        spawnedTriggerables.Clear();

        LoadLevels(data.levels);

        SetLevelActive(data.currentLevelIndex);

        SpawnPlayer();
        Time.timeScale = 1;
    }

    void LoadLevels(List<SaveableLevelData> loadableData)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            LoadLevel(i, loadableData[i]);
            SaveLevel(i);
            UnloadCurrentLevel();
        }
    }

    void LoadLevel(int levelIndex, SaveableLevelData levelDataToLoad)
    {
        entityLayer = levels[levelIndex].LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = levels[levelIndex].LayerInstances[INTGRID_LAYER_INDEX];

        LoadGridNodes(levelIndex, levelDataToLoad);

        LinkInteractablesToTriggerables();

        CacheGridNodeNeighbours();
    }

    void LoadGridNodes(int levelIndex, SaveableLevelData levelDataToLoad)
    {
        GenerateLevel(levelIndex, true);

        //Load Entities
        LoadNPCs(levelIndex, levelDataToLoad);
        LoadWorldItems(levelIndex, levelDataToLoad);
        LoadContainers(levelIndex, levelDataToLoad);
        LoadTriggerableData(levelDataToLoad);
        LoadInteractableData(levelDataToLoad);

        void LoadNPCs(int levelIndex, SaveableLevelData levelDataToLoad)
        {
            foreach (SaveableLevelData.NPCSaveData savedNPCData in levelDataToLoad.NPCs)
            {
                GridNode spawnNode = GetNodeAtCoords(savedNPCData.coords);
                NPCController NPCClone = Instantiate(npcPrefab, spawnNode.transform.localPosition + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, savedNPCData.rotation, 0)), spawnNode.transform);
                NPCClone.InitNPC(levelIndex, savedNPCData.npcData, spawnNode);
                NPCClone.SetNPCHealth(savedNPCData.currentHealth);
                NPCClone.SetActive(false);
                spawnedNPCs.Add(NPCClone);
            }
        }
        void LoadWorldItems(int levelIndex, SaveableLevelData levelDataToLoad)
        {
            foreach (SaveableLevelData.WorldItemSaveData savedWorldItem in levelDataToLoad.worldItems)
            {
                GridNode spawnNode = GetNodeAtCoords(savedWorldItem.coords);
                WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, savedWorldItem.rotation, 0)), spawnNode.transform);
                spawnedWorldItem.InitWorldItem(levelIndex, savedWorldItem.coords, savedWorldItem.itemStack);
                spawnedWorldItems.Add(spawnedWorldItem);
            }
        }
        void LoadContainers(int levelIndex, SaveableLevelData levelDataToLoad)
        {
            foreach (SaveableLevelData.ContainerSaveData savedContainer in levelDataToLoad.containers)
            {
                GridNode spawnNode = GetNodeAtCoords(savedContainer.coords);
                IContainer spawnedContainer = Instantiate(largeContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, savedContainer.rotation, 0)), spawnNode.transform);
                spawnedContainer.LoadContainerItemStacks(savedContainer.containedItemStacks);
                //Debug.Log(savedContainer.containedItemStacks[0].itemStack.itemData);
                spawnedContainer.InitContainer(levelIndex, savedContainer.coords);
                spawnedContainers.Add(spawnedContainer);
            }
        }
        void LoadTriggerableData(SaveableLevelData levelDataToLoad)
        {
            foreach (ITriggerable triggerable in spawnedTriggerables)
            {
                foreach (SaveableLevelData.TriggerableSaveData triggerableSaveData in levelDataToLoad.triggerableSaveData)
                {
                    if (triggerable.GetCoords() == triggerableSaveData.coords)
                        triggerable.LoadData(triggerableSaveData);
                }
            }
        }
        void LoadInteractableData(SaveableLevelData levelDataToLoad)
        {
            foreach (IInteractable interactable in spawnedInteractables)
            {
                foreach (SaveableLevelData.InteractableSaveData interactableSaveData in levelDataToLoad.interactableSaveData)
                {
                    if (interactable.GetCoords() == interactableSaveData.coords)
                        interactable.LoadData(interactableSaveData);
                }
            }
        }
    }
    #endregion
    
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


    public async Task BeginLevelTransition(int levelIndex, Vector2 playerMoveToCoords)
    {
        SaveLevel(currentLevelIndex);
        UnloadCurrentLevel();

        SetLevelActive(levelIndex);

        MovePlayer(playerMoveToCoords);

        await Task.Yield();
    }

    private void MovePlayer(Vector2 coordsToMoveTo)
    {
        playerController.MoveToCoords(coordsToMoveTo);
        if(activeNodes.TryGetValue(coordsToMoveTo, out GridNode node))
        {
            playerController.SetCurrentOccupiedNode(node);
        }
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

    void SetLevelActive(int levelIndex)
    {
        if (!levelDataDictionary.TryGetValue(levelIndex, out LevelData levelData))
            return;

        currentLevelIndex = levelIndex;

        //Debug.Log("Loading level: " + levelIndex);
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

    
    void UnloadCurrentLevel()
    {
        foreach (NPCController NPC in activeNPCs)
        {
            NPC.SnapToNode(NPC.movementController.targetNode);
            NPC.SetActive(false);
        }
        activeNPCs.Clear();

        if (spawnedPlayerSpawnPoint)
            Destroy(spawnedPlayerSpawnPoint);

        foreach (GridNode node in activeNodes.Values)
        {
            node.SetActive(false);
        }
        activeNodes.Clear();
    }

    

    NPCData GetNPCData(object value)
    {
        string npcDataIdentifier = value.ToString();
        //Debug.Log($"Trying to spawn: {npcDataIdentifier}");
        return NPCDataContainer.GetDataFromIdentifier(npcDataIdentifier);
    }

    void CacheGridNodeNeighbours()
    {
        foreach (var node in activeNodes.Values)
        {
            node.CacheNeighbours();
        }
    }

    void SpawnPlayer()
    {
        if (playerController)
        {
            if (!playerController.gameObject.activeSelf)
                playerController.gameObject.SetActive(true);

            return;
        }

        playerController = Instantiate(playerPrefab);
        playerController.InitPlayer(playerCharData);
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

    public GridNode GetNodeFromWorldPos(Vector3 worldPos) => GetNodeAtCoords(new Vector2(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y));

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

    public string GetCurrentLevelName()
    {
        return project.Json.FromJson.Levels[currentLevelIndex].FieldInstances[0].Value.ToString();
    }

    private List<SaveableLevelData> GetSaveableLevelData()
    {
        List<SaveableLevelData> saveableLevelDatas = new List<SaveableLevelData>();
        foreach (int levelIndex in levelDataDictionary.Keys)
        {
            saveableLevelDatas.Add(new SaveableLevelData(
                levelIndex,
                spawnedInteractables,
                spawnedTriggerables,
                spawnedWorldItems,
                spawnedContainers,
                spawnedNPCs
            ));
        }

        return saveableLevelDatas;
    }

    public void Save(ref SaveSystem.SaveData data)
    {
        data.gameTime = gameTime;
        data.LevelData.currentLevelIndex = currentLevelIndex;
        data.LevelData.currentLevelName = GetLevelNameFromIndex(currentLevelIndex);
        data.LevelData.levels = GetSaveableLevelData();
    }

    public void Load(SaveSystem.SaveData data)
    {
        gameTime = data.gameTime;
        LoadGame(data.LevelData);
    }


}
