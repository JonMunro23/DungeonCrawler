using LDtkUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance;

    const int ENTITY_LAYER_INDEX = 0;
    const int INTGRID_LAYER_INDEX = 1;

    [SerializeField] float gameTime;

    [SerializeField] LDtkComponentProject project;
    List<Level> LDtkLevels = new List<Level>();
    LayerInstance entityLayer;
    LayerInstance intGridLayer;

    [SerializeField] bool skipMainMenu = true;
    
    [Header("Grid")]
    [SerializeField] GridNode wallPrefab;
    [SerializeField] GridNode walkablePrefab;
    [SerializeField] GridNode voidPrefab;
    Dictionary<Vector2, GridNode> activeNodes = new Dictionary<Vector2, GridNode>();
    Dictionary<int, GridNode[]> nodesByIndexPerLevel = new Dictionary<int, GridNode[]>();
    const float GRID_SIZE = 3;
    Grid grid;


    [Header("Levels")]
    [SerializeField] int startingLevelIndex;
    [SerializeField] int currentLevelIndex;
    /// <summary>
    /// int = levelIndex
    /// </summary>
    [SerializeField] List<Transform> levelParents = new List<Transform>();
    Dictionary<int, LevelData> levelDataDictionary = new Dictionary<int, LevelData>();
    int levelSecrets;


    [Header("Player")]
    [SerializeField] PlayerController playerPrefab;
    [SerializeField] CharacterData playerCharData, defaultPlayerCharData;
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab, spawnedPlayerSpawnPoint;
    public PlayerController playerController;
    Vector2 playerSpawnCoords = Vector2.zero;

    [Header("NPCs")]
    [SerializeField] bool spawnNPCs = true;
    [SerializeField] NPCController zombieNpcPrefab;
    [SerializeField] NPCController rangerNpcPrefab;
    [SerializeField] NPCController bugNpcPrefab;
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
    [SerializeField] Container chestContainerPrefab;
    [SerializeField] List<IContainer> spawnedContainers = new List<IContainer>();

    [Header("Interactables")]
    [SerializeField] Lever leverPrefab;
    [SerializeField] Button buttonPrefab;
    [SerializeField] KeycardReader keycardReaderPrefab;
    [SerializeField] Lock lockPrefab;
    [SerializeField] PressurePlate pressurePlatePrefab;
    [SerializeField] Tripwire tripwirePrefab;
    [SerializeField] ShootableTarget shootableTargetPrefab;
    [SerializeField] PlayerTrigger playerTriggerPrefab;
    [SerializeField] SecretAreaTrigger secretAreaTriggerPrefab;
    [SerializeField] Sign signPrefab;
    List<IInteractable> spawnedInteractables = new List<IInteractable>();

    [Header("Triggerables")]
    [SerializeField] Door doorPrefab;
    [SerializeField] Door secretDoorPrefab;
    [SerializeField] NPCSpawner npcSpawnerPrefab;
    [SerializeField] RadiationEmitter radEmitterPrefab;
    List<ITriggerable> spawnedTriggerables = new List<ITriggerable>();

    [Header("Destructables")]
    [SerializeField] Destructable destructableWallPrefab;

    [Header("Spawn Offsets")]
    [SerializeField] Vector3 centeredEntitySpawnOffset;
    [SerializeField] Vector3 worldItemSpawnOffset;

    GridNode[] nodesByIndex;
    public GridNode GetNodeByIndex(int idx) => (nodesByIndex != null && idx >= 0 && idx < nodesByIndex.Length) ? nodesByIndex[idx] : null;
    public int CurrentNodeCount => nodesByIndex != null ? nodesByIndex.Length : 0;


    public static event Action OnFinishedGeneratingLevel;
    public static event Action<int> OnLevelGenerated;

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

        public Vector2Int Pos { get; set; }
    }

    #region Unity Lifecycle

    private void OnEnable()
    {
        MainMenu.onNewGameStartedSkippedIntro += NewGame;
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
        MainMenu.onNewGameStartedSkippedIntro -= NewGame;
        SelectableCharacter.OnCharacterSelected -= OnCharacterSelected;
        PauseMenu.onQuit -= OnQuit;

        NPCController.onNPCDeath -= OnNPCDeath;

        PlayerController.onPlayerDeath -= OnPlayerDeath;

        WorldItem.onWorldItemPickedUp -= OnWorldItemPickedUp;
        WorldItem.onWorldItemGrabbed -= OnWorldItemPickedUp;
    }

    void Awake()
    {
        Instance = this;
        grid = GetComponent<Grid>();
    }

    void Start()
    {
        GetLevelsFromLDtk();

        //generate nodes for each level but not entities
        //set levels inactive

        if (skipMainMenu)
        {
            playerCharData = defaultPlayerCharData;
            HelperFunctions.SetCursorActive(false);
            NewGame();
        }
    }

    void Update()
    {
        if (PauseMenu.isPaused || !PlayerController.isPlayerAlive) return;

        // change to only start counting game time when level and player have been fully initialised and user can control player
        gameTime += Time.deltaTime;
    }

    #endregion


    #region Event Handlers
    void OnCharacterSelected(CharacterData charData)
    {
        playerCharData = charData;
        NewGame();
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
        //foreach(LevelData levelData in levelDataDictionary.Values)
        //{
        //    if (levelData.spawnedNPCs.Contains(deadNPC))
        //        levelData.spawnedNPCs.Remove(deadNPC);
        //}

        //if(spawnedNPCs.Contains(deadNPC))
        //    spawnedNPCs.Remove(deadNPC);

        if(activeNPCs.Contains(deadNPC))
            activeNPCs.Remove(deadNPC);
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
    #endregion

    #region Getters
    public IEnumerable<GridNode> GetAllActiveNodes()
    {
        return activeNodes.Values;
    }
    void GetLevelsFromLDtk()
    {
        LDtkLevels.Clear();
        foreach (Level level in project.Json.FromJson.Levels)
        {
            LDtkLevels.Add(level);
        }
    }
    public int GetCurrentLevelIndex() => currentLevelIndex;
    public List<GridNode> GetCurrentNodesForLevel(int levelIndex)
    {
        if (nodesByIndexPerLevel.TryGetValue(levelIndex, out GridNode[] nodes))
            return nodes.ToList();
        else
            return null;
    }
    public GridNode GetNodeAtCoords(Vector2 coords) => activeNodes.TryGetValue(coords, out var node) ? node : null;
    public GridNode GetNodeFromWorldPos(Vector3 worldPos) => GetNodeAtCoords(new Vector2(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y));
    public string GetLevelNameFromIndex(int levelIndex)
    {
        return project.Json.FromJson.Levels[levelIndex].FieldInstances[0].Value.ToString();
    }
    public string GetCurrentLevelName()
    {
        return project.Json.FromJson.Levels[currentLevelIndex].FieldInstances[0].Value.ToString();
    }
    NPCData GetNPCData(object value)
    {
        string npcDataIdentifier = value.ToString();
        //Debug.Log($"Trying to spawn: {npcDataIdentifier}");
        return NPCDataContainer.GetDataFromIdentifier(npcDataIdentifier);
    }
    #endregion

    #region NewGame
    void NewGame()
    {
        InstantiateLevels();

        SetLevelActive(startingLevelIndex);

        SpawnPlayer();

        MovePlayer(playerSpawnCoords);

        OnFinishedGeneratingLevel?.Invoke();
    }

    void InstantiateLevels()
    {
        for (int i = 0; i < LDtkLevels.Count; i++)
        {
            InstantiateLevel(i);
            SaveLevel(i);
            UnloadCurrentLevel();
        }
    }

    void InstantiateLevel(int levelIndex)
    {
        entityLayer = LDtkLevels[levelIndex].LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = LDtkLevels[levelIndex].LayerInstances[INTGRID_LAYER_INDEX];

        Transform levelParent = new GameObject($"Level {levelIndex}").transform;
        levelParent.SetParent(transform);
        levelParents.Add(levelParent);

        GenerateLevel(levelIndex);

        OnLevelGenerated?.Invoke(levelIndex);

        LinkInteractablesToTriggerables();
        CacheGridNodeNeighbours();
    }

    void GenerateLevel(int levelIndex)
    {
        int nodeIndex = 0;
        Vector2Int spawnCoords = Vector2Int.zero;
        Vector3Int cellPos = Vector3Int.zero;
        Transform nodeParent = levelParents[levelIndex];

        GridNode[] levelNodesByIndex = new GridNode[intGridLayer.IntGridCsv.Length];

        for (int i = 0; i < intGridLayer.CWid; i++)
        {
            for (int j = 0; j < intGridLayer.CHei; j++)
            {
                GridNode clone = null;

                spawnCoords = new Vector2Int(-i, j);
                cellPos = new Vector3Int(spawnCoords.x, spawnCoords.y, 0);

                SquareCoords sqCoords = new SquareCoords { Pos = spawnCoords };

                switch (intGridLayer.IntGridCsv[nodeIndex])
                {
                    case 1:
                        clone = Instantiate(wallPrefab, grid.GetCellCenterLocal(cellPos), Quaternion.identity, nodeParent);
                        clone.transform.localPosition += new Vector3(-1.5f, 1.5f, -1.5f);
                        break;
                    case 2:
                        clone = Instantiate(walkablePrefab, grid.GetCellCenterLocal(cellPos), Quaternion.identity, nodeParent);
                        break;
                    case 3:
                        clone = Instantiate(voidPrefab, grid.GetCellCenterLocal(cellPos), Quaternion.identity, nodeParent);
                        clone.SetIsVoid(true);
                        break;
                }

                clone.InitNode(sqCoords, nodeIndex);

                levelNodesByIndex[nodeIndex] = clone;

                //activeNodes.Add(spawnCoords, clone);

                Vector2 loopIndices = new Vector2(i, j);
                GenerateEntities(levelIndex, loopIndices, spawnCoords);

                nodeIndex++;
            }
        }

        nodesByIndexPerLevel[levelIndex] = levelNodesByIndex;
    }


    void GenerateEntities(int levelIndex, Vector2 loopIndices, Vector2 spawnCoords)
    {
        GridNodeOccupant newOccupant;
        Item newItem;
        WeaponItemData weaponItemData;
        levelSecrets = 0;
        for (int k = 0; k < entityLayer.EntityInstances.Length; k++)
        {
            if (entityLayer.EntityInstances[k].Grid[1] == loopIndices.x && entityLayer.EntityInstances[k].Grid[0] == loopIndices.y)
            {
                GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                switch (entityLayer.EntityInstances[k].Identifier)
                {
                    case "Player_Start":
                        playerSpawnCoords = spawnCoords;
                        break;
                    case "WorldItem":
                        WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + centeredEntitySpawnOffset + (Vector3.up * .5f), Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        ItemData worldItemData = itemDataContainer.GetDataFromIdentifier(entityLayer.EntityInstances[k].FieldInstances[1].Value.ToString());

                        weaponItemData = worldItemData as WeaponItemData;
                        if (weaponItemData)
                            newItem = new WeaponItem(weaponItemData, weaponItemData.defaultLoadedAmmoData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value));
                        else
                            newItem = new Item(worldItemData);

                        spawnedWorldItem.InitWorldItem(levelIndex, new ItemStack(newItem, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value)));
                        spawnedWorldItems.Add(spawnedWorldItem);
                        break;
                    case "Level_Transition":
                        LevelTransition spawnedLevelTransition = Instantiate(levelTransitionPrefab, spawnNode.transform.position + centeredEntitySpawnOffset + new Vector3(0, 1.5f, 0), Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        int levelIndexToGoTo = LDtkFieldHelper.GetValue<int>(entityLayer.EntityInstances[k].FieldInstances[1].Value);
                        List<object> levelCoords = (List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value;
                        spawnedLevelTransition.InitLevelTransition(levelIndexToGoTo, new Vector2(-Convert.ToInt32(levelCoords[1]), Convert.ToInt32(levelCoords[0])));
                        spawnedLevelTransitions.Add(spawnedLevelTransition);
                        newOccupant = new GridNodeOccupant(spawnedLevelTransition.gameObject, GridNodeOccupantType.LevelTransition);
                        spawnNode.SetBaseOccupant(newOccupant);
                        spawnNode.SetOccupant(newOccupant);
                        break;
                    case "Container":
                        IContainer spawnedContainer = null;
                        List<object> itemNames = new List<object>();
                        List<object> itemAmounts = new List<object>();
                        switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                        {
                            case "Chest":
                                spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                                itemNames.AddRange((List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value);
                                itemAmounts.AddRange((List<object>)entityLayer.EntityInstances[k].FieldInstances[3].Value);
                                for (int l = 0; l < itemNames.Count; l++)
                                {
                                    ItemData itemData = itemDataContainer.GetDataFromIdentifier(itemNames[l].ToString());
                                    int itemAmount = Convert.ToInt32(itemAmounts[l]);

                                    weaponItemData = itemData as WeaponItemData;
                                    if (weaponItemData)
                                        newItem = new WeaponItem(weaponItemData, weaponItemData.defaultLoadedAmmoData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value));
                                    else
                                        newItem = new Item(itemData);

                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(newItem, itemAmount)));
                                }
                                break;
                            case "Desk":
                                //change to Desk prefab
                                spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                                itemNames.AddRange((List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value);
                                itemAmounts.AddRange((List<object>)entityLayer.EntityInstances[k].FieldInstances[3].Value);
                                for (int l = 0; l < itemNames.Count; l++)
                                {
                                    ItemData itemData = itemDataContainer.GetDataFromIdentifier(itemNames[l].ToString());
                                    int itemAmount = Convert.ToInt32(itemAmounts[l]);

                                    weaponItemData = itemData as WeaponItemData;
                                    if (weaponItemData)
                                        newItem = new WeaponItem(weaponItemData, weaponItemData.defaultLoadedAmmoData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value));
                                    else
                                        newItem = new Item(itemData);

                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(newItem, itemAmount)));

                                }
                                break;
                            case "Filling_Cabinet":
                                //change to Filling cabinet prefab
                                spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                                itemNames.AddRange((List<object>)entityLayer.EntityInstances[k].FieldInstances[2].Value);
                                itemAmounts.AddRange((List<object>)entityLayer.EntityInstances[k].FieldInstances[3].Value);
                                for (int l = 0; l < itemNames.Count; l++)
                                {
                                    ItemData itemData = itemDataContainer.GetDataFromIdentifier(itemNames[l].ToString());
                                    int itemAmount = Convert.ToInt32(itemAmounts[l]);

                                    weaponItemData = itemData as WeaponItemData;
                                    if (weaponItemData)
                                        newItem = new WeaponItem(weaponItemData, weaponItemData.defaultLoadedAmmoData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value));
                                    else
                                        newItem = new Item(itemData);

                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(newItem, itemAmount)));

                                }
                                break;

                        }
                        spawnedContainer.InitContainer(levelIndex, spawnCoords);
                        spawnedContainers.Add(spawnedContainer);
                        break;
                    case "NPC_Invis_Wall":
                        newOccupant = new GridNodeOccupant(null, GridNodeOccupantType.NPCInaccessible);
                        spawnNode.SetBaseOccupant(newOccupant);
                        spawnNode.SetOccupant(newOccupant);
                        break;
                    case "Destructable_Wall":
                        Destructable spawnedDestructable = null;
                        spawnedDestructable = Instantiate(destructableWallPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        spawnedDestructable.SetOccupyingNode(spawnNode);
                        spawnedDestructable.SetLevelIndex(levelIndex);
                        newOccupant = new GridNodeOccupant(spawnedDestructable.gameObject, GridNodeOccupantType.Obstacle);
                        spawnNode.SetOccupant(newOccupant);
                        break;
                    case "Sign":
                        Sign sign = Instantiate(signPrefab, spawnNode.transform.position, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        sign.SetSignText(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[1].Value));
                        //interactable = sign;
                        break;
                    case "Secret_Area":
                        SecretAreaTrigger trigger = Instantiate(secretAreaTriggerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        float width = (entityLayer.EntityInstances[k].Width / 16 * GRID_SIZE);
                        float height = (entityLayer.EntityInstances[k].Height / 16 * GRID_SIZE);
                        trigger.SetColliderSize(width, height);
                        trigger.SetExperienceValue(LDtkFieldHelper.GetValue<int>(entityLayer.EntityInstances[k].FieldInstances[0].Value));
                        levelSecrets++;
                        break;
                    case "Radiation_Emitter":
                        RadiationEmitter radEmitter = Instantiate(radEmitterPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        newOccupant = new GridNodeOccupant(radEmitter.gameObject, GridNodeOccupantType.RadiationEmitter);
                        spawnNode.SetOccupant(newOccupant);
                        radEmitter.Init(spawnNode, LDtkFieldHelper.GetValue<int>(entityLayer.EntityInstances[k].FieldInstances[0].Value));
                        break;

                }
                GenerateNPCs(levelIndex, loopIndices, spawnCoords);
                GenerateInteractables(levelIndex, loopIndices, spawnCoords);
                GenerateTriggerables(levelIndex, loopIndices, spawnCoords);
            }
        }
    }

    void GenerateNPCs(int levelIndex, Vector2 loopIndices, Vector2 spawnCoords)
    {
        for (int k = 0; k < entityLayer.EntityInstances.Length; k++)
        {
            if (entityLayer.EntityInstances[k].Grid[1] == loopIndices.x && entityLayer.EntityInstances[k].Grid[0] == loopIndices.y)
            {
                GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                NPCController NPCClone = null;
                switch (entityLayer.EntityInstances[k].Identifier)
                {
                            
                    case "NPC_Zombie":
                        if (spawnNPCs)
                        {
                            NPCClone = Instantiate(zombieNpcPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        }
                        break;
                    case "NPC_Ranger":
                        if (spawnNPCs)
                        {
                            NPCClone = Instantiate(rangerNpcPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        }
                        break;
                    case "NPC_Bug":
                        if (spawnNPCs)
                        {
                            NPCClone = Instantiate(bugNpcPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        }
                        break;
                }

                if (NPCClone == null)
                    continue;

                NPCClone.InitNPC(levelIndex, /*spawnData, */spawnNode);
                NPCClone.SetMovementBehaviour(HelperFunctions.ToEnum<NPCMovementBehaviour>(entityLayer.EntityInstances[k].FieldInstances[1].Value.ToString()));
                NPCClone.SetActive(false);
                spawnedNPCs.Add(NPCClone);
            }
        }
    }

    void GenerateInteractables(int levelIndex, Vector2 loopIndices, Vector2 spawnCoords)
    {
        for (int k = 0; k < entityLayer.EntityInstances.Length; k++)
        {
            if (entityLayer.EntityInstances[k].Grid[1] == loopIndices.x && entityLayer.EntityInstances[k].Grid[0] == loopIndices.y)
            {
                GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                GridNodeOccupant newOccupant;
                IInteractable interactable = null;
                switch (entityLayer.EntityInstances[k].Identifier)
                {
                    case "Lever":
                        interactable = Instantiate(leverPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        break;
                    case "Button":
                        interactable = Instantiate(buttonPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        break;
                    case "Keycard_Reader":
                        KeycardReader keycardReader = Instantiate(keycardReaderPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        keycardReader.SetRequiredKeycardType(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[4].Value));
                        interactable = keycardReader;
                        break;
                    case "Lock":
                        Lock @lock = Instantiate(lockPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        @lock.SetRequiredKeyType(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[4].Value));
                        interactable = @lock;
                        break;
                    case "Pressure_Plate":
                        PressurePlate plate = Instantiate(pressurePlatePrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        plate.SetTriggerOnExit(LDtkFieldHelper.GetValue<bool>(entityLayer.EntityInstances[k].FieldInstances[4].Value));
                        interactable = plate;
                        newOccupant = new GridNodeOccupant(interactable.GetGameObject(), GridNodeOccupantType.PressurePlate);
                        spawnNode.SetBaseOccupant(newOccupant);
                        spawnNode.SetOccupant(newOccupant);
                        break;
                    case "Tripwire":
                        Tripwire tripwire = Instantiate(tripwirePrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        tripwire.InitTripwire();
                        interactable = tripwire;
                        break;
                    case "Shootable_Target":
                        interactable = Instantiate(shootableTargetPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        break;
                    case "Trigger":
                        interactable = Instantiate(playerTriggerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        break;
                }

                if (interactable == null)
                    return;


                FieldInstance field = Array.Find(
                    entityLayer.EntityInstances[k].FieldInstances,
                    f => f.Identifier == "Entities_To_Trigger"
                    );

                if (field != null)
                {
                    var list = field.Value as List<object>;
                    foreach (var item in list)
                    {
                        if (item is Dictionary<string, object> dict)
                        {
                            interactable.AddEntityRefToTrigger(dict);
                        }
                        else
                        {
                            Debug.LogError($"Unexpected element type in EntityRef array: {item?.GetType().FullName}");
                        }
                    }
                }

                if (entityLayer.EntityInstances[k].FieldInstances.Length >= 3)
                    interactable.SetTriggerOperation(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[2].Value));

                if (entityLayer.EntityInstances[k].FieldInstances.Length >= 4)
                    interactable.SetIsSingleUse(LDtkFieldHelper.GetValue<bool>(entityLayer.EntityInstances[k].FieldInstances[3].Value));

                interactable.SetLevelIndex(levelIndex);
                interactable.SetOccupyingNode(spawnNode);
                spawnedInteractables.Add(interactable);
            }
        }
    }

    void GenerateTriggerables(int levelIndex, Vector2 loopIndices, Vector2 spawnCoords)
    {
        for (int k = 0; k < entityLayer.EntityInstances.Length; k++)
        {
            if (entityLayer.EntityInstances[k].Grid[1] == loopIndices.x && entityLayer.EntityInstances[k].Grid[0] == loopIndices.y)
            {
                GridNode spawnNode = GetNodeAtCoords(spawnCoords);
                GridNodeOccupant newOccupant;
                ITriggerable triggerable = null;
                switch (entityLayer.EntityInstances[k].Identifier)
                {
                    case "Door":
                        Door spawnedDoor = null;
                        switch (entityLayer.EntityInstances[k].FieldInstances[1].Value)
                        {
                            case "Door":
                                spawnedDoor = Instantiate(doorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                                break;
                            case "Secret_Door":
                                spawnedDoor = Instantiate(secretDoorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                                break;
                        }
                        spawnedDoor.SetRequiredNumberOfTriggers(LDtkFieldHelper.GetValue<int>(entityLayer.EntityInstances[k].FieldInstances[2].Value));
                        spawnedDoor.SetOccupyingNode(spawnNode);
                        newOccupant = new GridNodeOccupant(spawnedDoor.gameObject, GridNodeOccupantType.Obstacle);
                        spawnNode.SetBaseOccupant(newOccupant);
                        spawnNode.SetOccupant(newOccupant);
                        spawnedDoor.SetIsTriggered(LDtkFieldHelper.GetValue<bool>(entityLayer.EntityInstances[k].FieldInstances[2].Value));
                        triggerable = spawnedDoor;
                        break;
                    case "NPC_Spawner":
                        NPCSpawner npcSpawner = Instantiate(npcSpawnerPrefab, spawnNode.transform.position, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        npcSpawner.AssignSpawnedNPCsList(ref spawnedNPCs);
                        npcSpawner.SetOccupyingNode(spawnNode);
                        npcSpawner.SetLevelIndex(levelIndex); //need to do it early to pass to spawnedNPC
                        npcSpawner.SetSpawnBehaviour(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[1].Value));
                        npcSpawner.SpawnNPC(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[0].Value));
                        triggerable = npcSpawner;
                        break;
                }

                if (triggerable == null)
                    return;

                triggerable.SetEntityRef(entityLayer.EntityInstances[k].Iid);
                triggerable.SetLevelIndex(levelIndex);
                spawnedTriggerables.Add(triggerable);
            }
        }
    }
    #endregion

    #region LoadGame
    private void LoadGame(LevelSaveData data)
    {
        //Generate grid nodes but dont generate entities
        //loop through grid nodes and generate entities using loaded data

        foreach (Transform levelParent in levelParents)
        {
            Destroy(levelParent.gameObject);
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
        for (int i = 0; i < LDtkLevels.Count; i++)
        {
            LoadLevel(i, loadableData[i]);
            SaveLevel(i);
            UnloadCurrentLevel();
        }
    }

    void LoadLevel(int levelIndex, SaveableLevelData levelDataToLoad)
    {
        entityLayer = LDtkLevels[levelIndex].LayerInstances[ENTITY_LAYER_INDEX];
        intGridLayer = LDtkLevels[levelIndex].LayerInstances[INTGRID_LAYER_INDEX];

        LoadGridNodes(levelIndex, levelDataToLoad);

        LinkInteractablesToTriggerables();

        CacheGridNodeNeighbours();
    }

    void LoadGridNodes(int levelIndex, SaveableLevelData levelDataToLoad)
    {
        InstantiateLevel(levelIndex);

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
                NPCController NPCClone = Instantiate(zombieNpcPrefab, spawnNode.transform.localPosition + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, savedNPCData.rotation, 0)), spawnNode.transform);
                NPCClone.InitNPC(levelIndex, /*savedNPCData.npcData, */spawnNode);
                NPCClone.healthController.SetHealth(savedNPCData.currentHealth);
                NPCClone.SetActive(false);
                spawnedNPCs.Add(NPCClone);
            }
        }
        void LoadWorldItems(int levelIndex, SaveableLevelData levelDataToLoad)
        {
            foreach (SaveableLevelData.WorldItemSaveData savedWorldItem in levelDataToLoad.worldItems)
            {
                WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, savedWorldItem.position, Quaternion.Euler(savedWorldItem.rotation));
                spawnedWorldItem.InitWorldItem(levelIndex, savedWorldItem.itemStack);
                spawnedWorldItems.Add(spawnedWorldItem);
            }
        }
        void LoadContainers(int levelIndex, SaveableLevelData levelDataToLoad)
        {
            foreach (SaveableLevelData.ContainerSaveData savedContainer in levelDataToLoad.containers)
            {
                GridNode spawnNode = GetNodeAtCoords(savedContainer.coords);
                IContainer spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, savedContainer.rotation, 0)), spawnNode.transform);
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

    public void BeginLevelTransition(int levelIndex, Vector2 playerMoveToCoords)
    {
        SaveLevel(currentLevelIndex);
        UnloadCurrentLevel();

        SetLevelActive(levelIndex);

        MovePlayer(playerMoveToCoords);
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

        levelDataDictionary.Add(indexOfLevelToSave, new LevelData(activeNodes, npcsToSave, levelSecrets));
        //Debug.Log("Added level "+ indexOfLevelToSave + " to levels list");
    }

    void SetLevelActive(int levelIndex)
    {
        if (!levelDataDictionary.TryGetValue(levelIndex, out LevelData levelData))
            return;

        currentLevelIndex = levelIndex;

        if (nodesByIndexPerLevel.TryGetValue(levelIndex, out var arr))
            nodesByIndex = arr;
        else
            nodesByIndex = null;

        foreach (GridNode node in levelData.GetNodes().Values)
        {
            node.SetActive(true);
            activeNodes.Add(node.Coords.Pos, node);
        }

        foreach (NPCController NPC in levelData.GetNPCs())
        {
            NPC.SetActive(true);
            activeNPCs.Add(NPC);
        }
    }

    void UnloadCurrentLevel()
    {
        foreach (NPCController NPC in activeNPCs)
        {
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

    public float DecideSpawnDir(EntityInstance dir)
    {
        string meme = dir.FieldInstances[0].Value.ToString();

        float returnDir = 0;
        switch (meme)
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
        LoadGame(data.LevelData);
        gameTime = data.gameTime;
    }
}
