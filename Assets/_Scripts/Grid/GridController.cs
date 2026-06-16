using LDtkUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public class LevelGenerationContext
    {
        public bool IsLoading { get; private set; }
        public SaveableLevelData SaveData { get; private set; }

        public static LevelGenerationContext NewGame()
        {
            return new LevelGenerationContext
            {
                IsLoading = false,
                SaveData = null
            };
        }

        public static LevelGenerationContext Load(SaveableLevelData saveData)
        {
            return new LevelGenerationContext
            {
                IsLoading = true,
                SaveData = saveData
            };
        }
    }

    public static GridController Instance;

    const int ENTITY_LAYER_INDEX = 0;
    const int INTGRID_LAYER_INDEX = 1;

    [SerializeField] float gameTime;

    [SerializeField] LDtkComponentProject project;
    List<Level> LDtkLevels = new List<Level>();

    [SerializeField] bool skipMainMenu = true;
    
    [Header("Grid")]
    [SerializeField] GridNode wallPrefab;
    [SerializeField] GridNode walkablePrefab;
    [SerializeField] GridNode voidPrefab;
    const float GRID_SIZE = 3;
    Grid grid;

    [Header("Levels")]
    [SerializeField] int startingLevelIndex;
    [SerializeField] int currentLevelIndex;
    [SerializeField] List<Transform> levelParents = new List<Transform>();
    [SerializeField] LevelData activeLevel;
    Dictionary<int, LevelData> levelDataByIndex = new Dictionary<int, LevelData>();

    [Header("Player")]
    [SerializeField] PlayerController playerPrefab;
    [SerializeField] CharacterData playerCharData, defaultPlayerCharData;
    [SerializeField] PlayerSpawnPoint playerSpawnPointPrefab, spawnedPlayerSpawnPoint;
    public PlayerController playerController;
    Vector2Int playerSpawnCoords = Vector2Int.zero;

    [Header("NPCs")]
    [SerializeField] bool spawnNPCs = true;
    [SerializeField] NPCController zombieNpcPrefab;
    [SerializeField] NPCController rangerNpcPrefab;
    [SerializeField] NPCController bugNpcPrefab;

    [Header("World Items")]
    [SerializeField] WorldItem worldItemPrefab;
    public ItemDatabase itemDatabase;

    [Header("Level Transitions")]
    [SerializeField] LevelTransition levelTransitionPrefab;

    [Header("Containers")]
    [SerializeField] Container chestContainerPrefab;

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
    [SerializeField] Keypad keypadPrefab;

    [SerializeField] Sign signPrefab;

    [Header("Triggerables")]
    [SerializeField] Door doorPrefab;
    [SerializeField] Door secretDoorPrefab;
    [SerializeField] NPCSpawner npcSpawnerPrefab;
    [SerializeField] RadiationEmitter radEmitterPrefab;

    [Header("Destructables")]
    [SerializeField] Destructable destructableWallPrefab;

    [Header("Spawn Offsets")]
    [SerializeField] Vector3 centeredEntitySpawnOffset;
    [SerializeField] Vector3 worldItemSpawnOffset;

    public GridNode GetActiveNodeByIndex(int index) => activeLevel.GetNodeByIndex(index);
    public int CurrentActiveNodeCount => activeLevel.GetNodes().Count;


    public static event Action OnFinishedGeneratingLevel;
    public static event Action<LevelData> OnLevelNodesGenerated;

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

        WorldInteractionManager.onNewWorldItemSpawned += OnNewWorldItemSpawned;
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

        WorldInteractionManager.onNewWorldItemSpawned -= OnNewWorldItemSpawned;
    }

    void Awake()
    {
        Instance = this;
        grid = GetComponent<Grid>();
    }

    void Start()
    {
        GetLevelsFromLDtk();

        for (int i = 0; i < LDtkLevels.Count; i++)
        {
            LayerInstance intGridLayer = LDtkLevels[i].LayerInstances[INTGRID_LAYER_INDEX];
            LayerInstance entityLayer = LDtkLevels[i].LayerInstances[ENTITY_LAYER_INDEX];

            LevelData levelData = new LevelData(i, intGridLayer, entityLayer);
            levelDataByIndex.Add(i, levelData);
            GenerateLevelNodes(levelData);

            //Used by MapController to generate a map per level
            OnLevelNodesGenerated?.Invoke(levelData);
        }

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
    void OnNewWorldItemSpawned(WorldItem newWorldItem)
    {
        activeLevel.AddWorldItem(newWorldItem);
    }
    void OnWorldItemPickedUp(WorldItem pickedUpItem)
    {
        activeLevel.RemoveWorldItem(pickedUpItem);
    }
    void OnNPCDeath(NPCController deadNPC)
    {
        activeLevel.RemoveNPC(deadNPC);
    }
    void OnQuit()
    {
        foreach (var level in levelParents)
        {
            Destroy(level);
        }
        levelParents.Clear();
        //spawnedContainers.Clear();
        //spawnedInteractables.Clear();
        //spawnedLevelTransitions.Clear();
        //spawnedNPCs.Clear();
        //activeNPCs.Clear();
        //spawnedTriggerables.Clear();
        //spawnedWorldItems.Clear();

        levelDataByIndex.Clear();

        playerController.RemoveAudioSources();
        Destroy(playerController.gameObject);
        playerController = null;
    }
    #endregion

    #region Getters
    void GetLevelsFromLDtk()
    {
        LDtkLevels.Clear();
        foreach (Level level in project.Json.FromJson.Levels)
        {
            LDtkLevels.Add(level);
        }
    }
    public int GetCurrentLevelIndex() => currentLevelIndex;
    public GridNode GetNodeFromWorldPos(Vector3 worldPos) => activeLevel.GetNodeAtCoords(new Vector2Int(grid.WorldToCell(worldPos).x, grid.WorldToCell(worldPos).y));
    public LevelData GetLevelData(int levelIndex) => levelDataByIndex[levelIndex];
    public string GetCurrentLevelName() => GetLevelNameFromIndex(currentLevelIndex);
    public string GetLevelNameFromIndex(int levelIndex)
    {
        return project.Json.FromJson.Levels[levelIndex].FieldInstances[0].Value.ToString();
    }
    #endregion

    void NewGame()
    {
        LevelGenerationContext context = LevelGenerationContext.NewGame();

        for (int i = 0; i < LDtkLevels.Count; i++)
        {
            GenerateEntities(levelDataByIndex[i], context);
        }

        SetLevelActive(startingLevelIndex);

        SpawnPlayer();

        MovePlayer(playerSpawnCoords);

        OnFinishedGeneratingLevel?.Invoke();
    }

    void GenerateLevelNodes(LevelData levelData)
    {
        Transform levelParent = new GameObject($"Level {levelData.LevelIndex}").transform;
        levelParent.SetParent(transform);
        levelParents.Add(levelParent);

        int nodeIndex = 0;
        Vector2Int spawnCoords = Vector2Int.zero;
        Vector3Int cellPos = Vector3Int.zero;

        Dictionary<Vector2Int, GridNode> levelNodes = new Dictionary<Vector2Int, GridNode>();

        for (int i = 0; i < levelData.IntGridLayer.CWid; i++)
        {
            for (int j = 0; j < levelData.IntGridLayer.CHei; j++)
            {
                GridNode node = null;

                spawnCoords = new Vector2Int(-i, j);
                cellPos = new Vector3Int(spawnCoords.x, spawnCoords.y, 0);

                SquareCoords sqCoords = new SquareCoords { Pos = spawnCoords };

                switch (levelData.IntGridLayer.IntGridCsv[nodeIndex])
                {
                    case 1:
                        node = Instantiate(wallPrefab, grid.GetCellCenterLocal(cellPos), Quaternion.identity, levelParent);
                        node.transform.localPosition += new Vector3(-1.5f, 1.5f, -1.5f);
                        break;
                    case 2:
                        node = Instantiate(walkablePrefab, grid.GetCellCenterLocal(cellPos), Quaternion.identity, levelParent);
                        break;
                    case 3:
                        node = Instantiate(voidPrefab, grid.GetCellCenterLocal(cellPos), Quaternion.identity, levelParent);
                        node.SetIsVoid(true);
                        break;
                }

                node.InitNode(levelData, sqCoords, nodeIndex);

                levelNodes.Add(spawnCoords, node);
                nodeIndex++;
            }
        }
        
        levelData.AssignNodesToLevel(levelNodes);
        levelData.CacheNodeNeighbours();
    }

    void GenerateEntities(LevelData levelData, LevelGenerationContext context)
    {
        GenerateStaticEntities(levelData, context);
        GenerateRuntimeEntities(levelData, context);

        levelData.LinkInteractablesToTriggerables();
    }

    void GenerateStaticEntities(LevelData levelData, LevelGenerationContext context)
    {
        int levelIndex = levelData.LevelIndex;
        GridNodeOccupant newOccupant;
        EntityInstance entityInstance = null;

        for (int k = 0; k < levelData.EntityLayer.EntityInstances.Length; k++)
        {
            entityInstance = levelData.EntityLayer.EntityInstances[k];
            List<GridNode> nodes = levelDataByIndex[levelIndex].GetNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                GridNode spawnNode = nodes[i];
                Vector2Int spawnCoords = spawnNode.Coords.Pos;

                if (entityInstance.Grid[1] == -spawnCoords.x && entityInstance.Grid[0] == spawnCoords.y)
                {
                    switch (entityInstance.Identifier)
                    {
                        case "Player_Start":
                            playerSpawnCoords = spawnCoords;
                            break;
                        case "Level_Transition":
                            LevelTransition spawnedLevelTransition = Instantiate(levelTransitionPrefab, spawnNode.transform.position + centeredEntitySpawnOffset + new Vector3(0, 1.5f, 0), Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                            int levelIndexToGoTo = LDtkFieldHelper.GetValue<int>(entityInstance.FieldInstances[1].Value);
                            List<object> levelCoords = (List<object>)entityInstance.FieldInstances[2].Value;
                            spawnedLevelTransition.InitLevelTransition(levelIndexToGoTo, new Vector2Int(-Convert.ToInt32(levelCoords[1]), Convert.ToInt32(levelCoords[0])));
                            //spawnedLevelTransitions.Add(spawnedLevelTransition);
                            newOccupant = new GridNodeOccupant(spawnedLevelTransition.gameObject, GridNodeOccupantType.LevelTransition);
                            spawnNode.SetBaseOccupant(newOccupant);
                            spawnNode.SetOccupant(newOccupant);
                            break;
                        case "Container":
                            IContainer spawnedContainer = null;
                            List<object> itemNames = new List<object>();
                            List<object> itemAmounts = new List<object>();
                            Item newItem;
                            WeaponItemData weaponItemData;
                            switch (entityInstance.FieldInstances[1].Value)
                            {
                                case "Chest":
                                    spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                                    break;
                                case "Desk":
                                    //change to Desk prefab
                                    spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                                    break;
                                case "Filling_Cabinet":
                                    //change to Filling cabinet prefab
                                    spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                                    break;
                                case "Corpse":
                                    //change to Corpse prefab
                                    spawnedContainer = Instantiate(chestContainerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                                    break;

                            }
                            if (context.IsLoading)
                            {
                                var savedContainerData = context.SaveData.FindSavedContainerData(spawnCoords);

                                List<ContainerItemStack> itemStacks = new List<ContainerItemStack>();
                                if (savedContainerData != null)
                                {
                                    foreach (ItemStackSaveData itemStackSaveData in savedContainerData.containedItemStackSaveDatas)
                                    {
                                        ItemData itemData = itemDatabase.GetItemDataFromIdentifier(itemStackSaveData.itemID);

                                        if (itemData == null)
                                            continue;

                                        if (itemData is WeaponItemData)
                                        {
                                            weaponItemData = itemData as WeaponItemData;

                                            AmmoItemData ammoItemData = null;

                                            if (!string.IsNullOrEmpty(itemStackSaveData.loadedAmmoType))
                                                ammoItemData = itemDatabase.GetItemDataFromIdentifier(itemStackSaveData.loadedAmmoType) as AmmoItemData;

                                            newItem = new WeaponItem(weaponItemData, ammoItemData, itemStackSaveData.loadedAmmo);
                                        }
                                        else
                                        {
                                            newItem = new Item(itemData);
                                        }

                                        ItemStack stack = new ItemStack(newItem, itemStackSaveData.amount);
                                        ContainerItemStack containerStack = new ContainerItemStack(itemStackSaveData.slotIndex, stack);
                                        itemStacks.Add(containerStack);
                                    }
                                }
                                spawnedContainer.LoadContainerItemStacks(itemStacks);
                            } 
                            else
                            {
                                itemNames.AddRange((List<object>)entityInstance.FieldInstances[2].Value);
                                itemAmounts.AddRange((List<object>)entityInstance.FieldInstances[3].Value);
                                for (int l = 0; l < itemNames.Count; l++)
                                {
                                    ItemData itemData = itemDatabase.GetItemDataFromIdentifier(itemNames[l].ToString());
                                    int itemAmount = Convert.ToInt32(itemAmounts[l]);

                                    weaponItemData = itemData as WeaponItemData;
                                    if (weaponItemData)
                                        newItem = new WeaponItem(weaponItemData, weaponItemData.defaultLoadedAmmoData, Convert.ToInt32(entityInstance.FieldInstances[3].Value));
                                    else
                                        newItem = new Item(itemData);

                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(newItem, itemAmount)));

                                }
                            }

                            spawnedContainer.InitContainer(levelIndex, spawnCoords);
                            levelData.AddContainer(spawnedContainer);


                            break;
                        case "NPC_Invis_Wall":
                            newOccupant = new GridNodeOccupant(null, GridNodeOccupantType.NPCInaccessible);
                            spawnNode.SetBaseOccupant(newOccupant);
                            spawnNode.SetOccupant(newOccupant);
                            break;
                        case "Sign":
                            Sign sign = Instantiate(signPrefab, spawnNode.transform.position, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                            sign.SetSignText(LDtkFieldHelper.GetValue<string>(entityInstance.FieldInstances[1].Value));
                            //interactable = sign;
                            break;
                        case "Secret_Area":
                            SecretAreaTrigger trigger = Instantiate(secretAreaTriggerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                            float width = (entityInstance.Width / 16 * GRID_SIZE);
                            float height = (entityInstance.Height / 16 * GRID_SIZE);
                            trigger.SetColliderSize(width, height);
                            trigger.SetExperienceValue(LDtkFieldHelper.GetValue<int>(entityInstance.FieldInstances[0].Value));
                            levelData.AddSecret();
                            break;
                        case "Radiation_Emitter":
                            RadiationEmitter radEmitter = Instantiate(radEmitterPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                            newOccupant = new GridNodeOccupant(radEmitter.gameObject, GridNodeOccupantType.RadiationEmitter);
                            spawnNode.SetOccupant(newOccupant);
                            radEmitter.Init(spawnNode, LDtkFieldHelper.GetValue<int>(entityInstance.FieldInstances[0].Value));
                            break;

                    }
                    GenerateInteractables(levelData, entityInstance, spawnNode, context);
                    GenerateTriggerables(levelData, entityInstance, spawnNode, context);
                }
            }
        }
    }
    void GenerateRuntimeEntities(LevelData levelData, LevelGenerationContext context)
    {
        if (context.IsLoading)
        {
            GenerateRuntimeEntitiesFromSave(levelData, context.SaveData);
            return;
        }

        GenerateRuntimeEntitiesFromLDtk(levelData);
    }
    void GenerateRuntimeEntitiesFromLDtk(LevelData levelData)
    {
        int levelIndex = levelData.LevelIndex;
        GridNodeOccupant newOccupant;
        EntityInstance entityInstance = null;

        for (int k = 0; k < levelData.EntityLayer.EntityInstances.Length; k++)
        {
            entityInstance = levelData.EntityLayer.EntityInstances[k];
            List<GridNode> nodes = levelDataByIndex[levelIndex].GetNodes();

            for (int i = 0; i < nodes.Count; i++)
            {
                GridNode spawnNode = nodes[i];
                Vector2Int spawnCoords = spawnNode.Coords.Pos;

                if (entityInstance.Grid[1] == -spawnCoords.x && entityInstance.Grid[0] == spawnCoords.y)
                {
                    ItemStack itemStack;
                    switch (entityInstance.Identifier)
                    {
                        case "WorldItem":
                            itemStack = CreateItemStackFromLDtk(entityInstance);
                            SpawnWorldItem(levelData, spawnNode, itemStack, DecideSpawnDir(entityInstance));
                            break;
                        case "NoteItem":
                            itemStack = CreateNoteItemStackFromLDtk(entityInstance);
                            SpawnWorldItem(levelData, spawnNode, itemStack, DecideSpawnDir(entityInstance));
                            break;
                        case "Destructable_Wall":
                            Destructable spawnedDestructable = null;
                            spawnedDestructable = Instantiate(destructableWallPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                            spawnedDestructable.SetOccupyingNode(spawnNode);
                            spawnedDestructable.SetLevelIndex(levelIndex);
                            newOccupant = new GridNodeOccupant(spawnedDestructable.gameObject, GridNodeOccupantType.Obstacle);
                            spawnNode.SetOccupant(newOccupant);
                            break;
                    }

                    GenerateNPCs(levelData, entityInstance, spawnNode);
                }
            }
        }
    }
    void GenerateRuntimeEntitiesFromSave(LevelData levelData, SaveableLevelData saveData)
    {
        GenerateWorldItemsFromSave(levelData, saveData);
        GenerateNPCsFromSave(levelData, saveData);

        // Later:
        // GenerateDestructablesFromSave(levelData, saveData);
    }

    void GenerateWorldItemsFromSave(LevelData levelData, SaveableLevelData saveData)
    {
        foreach (SaveableLevelData.WorldItemSaveData savedWorldItem in saveData.worldItemSaveData)
        {
            Vector2Int coords = savedWorldItem.coords;
            GridNode spawnNode = levelData.GetNodeAtCoords(coords);
            Vector3 pos = savedWorldItem.position;
            Vector3 rot = savedWorldItem.rotation;

            if (spawnNode == null)
                continue;

            ItemData itemData = itemDatabase.GetItemDataFromIdentifier(savedWorldItem.itemStackSaveData.itemID);

            if (itemData == null)
                continue;

            Item item;
            if (itemData is WeaponItemData weaponItemData)
            {
                AmmoItemData ammoItemData = null;

                if (!string.IsNullOrEmpty(savedWorldItem.itemStackSaveData.loadedAmmoType))
                    ammoItemData = itemDatabase.GetItemDataFromIdentifier(savedWorldItem.itemStackSaveData.loadedAmmoType) as AmmoItemData;

                item = new WeaponItem(weaponItemData, ammoItemData, savedWorldItem.itemStackSaveData.loadedAmmo);
            }
            else
            {
                item = new Item(itemData);
            }

            ItemStack stack = new ItemStack(item, savedWorldItem.itemStackSaveData.amount);

            SpawnWorldItemFromSave(
                pos,
                rot,
                levelData,
                spawnNode,
                stack
            );
        }
    }
    void GenerateNPCsFromSave(LevelData levelData, SaveableLevelData saveData)
    {
        foreach (SaveableLevelData.NPCSaveData savedNPC in saveData.npcSaveData)
        {
            Vector2Int coords = savedNPC.coords;
            GridNode spawnNode = levelData.GetNodeAtCoords(coords);

            if (spawnNode == null)
                continue;

            NPCController prefab = GetNPCPrefabFromIdentifier(savedNPC.npcData.identifier);

            if (prefab == null)
                continue;

            SpawnNPC(
                levelData,
                spawnNode,
                prefab,
                savedNPC.rotation,
                savedNPC.movementBehaviour,
                savedNPC.currentHealth
            );
        }
    }
    void GenerateNPCs(LevelData levelData, EntityInstance entityInstance, GridNode spawnNode)
    {
        if (!spawnNPCs)
            return;

        NPCController prefab = GetNPCPrefabFromIdentifier(entityInstance.Identifier);

        if (prefab == null)
            return;

        NPCMovementBehaviour movementBehaviour =
            HelperFunctions.ToEnum<NPCMovementBehaviour>(entityInstance.FieldInstances[1].Value.ToString());

        SpawnNPC(
            levelData,
            spawnNode,
            prefab,
            DecideSpawnDir(entityInstance),
            movementBehaviour
        );
    }
    void GenerateInteractables(LevelData levelData, EntityInstance entityInstance, GridNode spawnNode, LevelGenerationContext context)
    {
        if (entityInstance.Grid[1] == -spawnNode.Coords.Pos.x && entityInstance.Grid[0] == spawnNode.Coords.Pos.y)
        {
            GridNodeOccupant newOccupant;
            IInteractable interactable = null;
            switch (entityInstance.Identifier)
            {
                case "Lever":
                    interactable = Instantiate(leverPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                    break;
                case "Button":
                    interactable = Instantiate(buttonPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                    break;
                case "Keycard_Reader":
                    KeycardReader keycardReader = Instantiate(keycardReaderPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                    keycardReader.SetRequiredKeycardType(LDtkFieldHelper.GetValue<string>(entityInstance.FieldInstances[4].Value));
                    interactable = keycardReader;
                    break;
                case "Lock":
                    Lock @lock = Instantiate(lockPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                    @lock.SetRequiredKeyType(LDtkFieldHelper.GetValue<string>(entityInstance.FieldInstances[4].Value));
                    interactable = @lock;
                    break;
                case "Pressure_Plate":
                    PressurePlate plate = Instantiate(pressurePlatePrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                    plate.SetTriggerOnExit(LDtkFieldHelper.GetValue<bool>(entityInstance.FieldInstances[4].Value));
                    interactable = plate;
                    newOccupant = new GridNodeOccupant(interactable.GetGameObject(), GridNodeOccupantType.PressurePlate);
                    spawnNode.SetBaseOccupant(newOccupant);
                    spawnNode.SetOccupant(newOccupant);
                    break;
                case "Tripwire":
                    Tripwire tripwire = Instantiate(tripwirePrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                    tripwire.InitTripwire();
                    interactable = tripwire;
                    break;
                case "Shootable_Target":
                    interactable = Instantiate(shootableTargetPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                    break;
                case "Trigger":
                    interactable = Instantiate(playerTriggerPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                    break;
                case "Keypad":
                    Keypad keypad = Instantiate(keypadPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                    string code = LDtkFieldHelper.GetValue<int>(entityInstance.FieldInstances[4].Value).ToString();
                    keypad.Init(code);
                    interactable = keypad;
                    break;
            }

            if (interactable == null)
                return;


            FieldInstance field = Array.Find(
                entityInstance.FieldInstances,
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

            if (entityInstance.FieldInstances.Length >= 3)
                interactable.SetTriggerOperation(LDtkFieldHelper.GetValue<string>(entityInstance.FieldInstances[2].Value));

            if (entityInstance.FieldInstances.Length >= 4)
                interactable.SetIsSingleUse(LDtkFieldHelper.GetValue<bool>(entityInstance.FieldInstances[3].Value));

            interactable.SetLevelIndex(levelData.LevelIndex);
            interactable.SetOccupyingNode(spawnNode);

            levelData.AddInteractable(interactable);

            if (context.IsLoading)
            {
                var savedInteractableData = context.SaveData.FindSavedInteractableData(interactable.GetCoords());

                if (savedInteractableData != null)
                    interactable.LoadData(savedInteractableData);
            }
        } 
    }
    void GenerateTriggerables(LevelData levelData, EntityInstance entityInstance, GridNode spawnNode, LevelGenerationContext context)
    {
        if (entityInstance.Grid[1] == -spawnNode.Coords.Pos.x && entityInstance.Grid[0] == spawnNode.Coords.Pos.y)
        {
            GridNodeOccupant newOccupant;
            ITriggerable triggerable = null;
            switch (entityInstance.Identifier)
            {
                case "Door":
                    Door spawnedDoor = null;
                    switch (entityInstance.FieldInstances[1].Value)
                    {
                        case "Door":
                            spawnedDoor = Instantiate(doorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                            break;
                        case "Secret_Door":
                            spawnedDoor = Instantiate(secretDoorPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance), 0)), spawnNode.transform);
                            break;
                    }
                    spawnedDoor.SetRequiredNumberOfTriggers(LDtkFieldHelper.GetValue<int>(entityInstance.FieldInstances[2].Value));
                    spawnedDoor.SetOccupyingNode(spawnNode);
                    newOccupant = new GridNodeOccupant(spawnedDoor.gameObject, GridNodeOccupantType.Obstacle);
                    spawnNode.SetBaseOccupant(newOccupant);
                    spawnNode.SetOccupant(newOccupant);
                    spawnedDoor.SetIsTriggered(LDtkFieldHelper.GetValue<bool>(entityInstance.FieldInstances[2].Value));
                    triggerable = spawnedDoor;
                    break;
                case "NPC_Spawner":
                    NPCSpawner npcSpawner = Instantiate(npcSpawnerPrefab, spawnNode.transform.position, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityInstance) + 180, 0)), spawnNode.transform);
                    npcSpawner.AssignSpawnedNPCsList(ref levelData.GetNPCsListRef());
                    npcSpawner.SetOccupyingNode(spawnNode);
                    npcSpawner.SetLevelIndex(levelData.LevelIndex);
                    npcSpawner.SetSpawnBehaviour(LDtkFieldHelper.GetValue<string>(entityInstance.FieldInstances[1].Value));

                    if(!context.IsLoading)
                        npcSpawner.SpawnNPC(LDtkFieldHelper.GetValue<string>(entityInstance.FieldInstances[0].Value));

                    triggerable = npcSpawner;
                    break;
            }

            if (triggerable == null)
                return;

            triggerable.SetEntityRef(entityInstance.Iid);
            triggerable.SetLevelIndex(levelData.LevelIndex);

            levelData.AddTriggerable(triggerable);

            if (context.IsLoading)
            {
                var savedTriggerableData = context.SaveData.FindSavedTriggerableData(triggerable.GetCoords());

                if (savedTriggerableData != null)
                    triggerable.LoadData(savedTriggerableData);
            }
        }
    }

    WorldItem SpawnWorldItem(LevelData levelData, GridNode spawnNode, ItemStack itemStack, float rotationY = 0)
    {
        WorldItem spawnedWorldItem = Instantiate(
            worldItemPrefab,
            spawnNode.transform.position + centeredEntitySpawnOffset + (Vector3.up * .5f),
            Quaternion.Euler(0, rotationY, 0),
            spawnNode.transform
        );

        spawnedWorldItem.InitWorldItem(levelData.LevelIndex, itemStack);
        levelData.AddWorldItem(spawnedWorldItem);

        return spawnedWorldItem;
    }

    WorldItem SpawnWorldItemFromSave(Vector3 position, Vector3 rotation, LevelData levelData, GridNode spawnNode, ItemStack itemStack)
    {
        WorldItem item = SpawnWorldItem(levelData, spawnNode, itemStack);
        item.transform.position = position;
        item.transform.rotation = Quaternion.Euler(rotation);

        return item;
    }
    ItemStack CreateItemStackFromLDtk(EntityInstance entityInstance)
    {
        ItemData worldItemData = itemDatabase.GetItemDataFromIdentifier(entityInstance.FieldInstances[1].Value.ToString());
        Item newItem;

        if (worldItemData is WeaponItemData weaponItemData)
        {
            newItem = new WeaponItem(
                weaponItemData,
                weaponItemData.defaultLoadedAmmoData,
                Convert.ToInt32(entityInstance.FieldInstances[3].Value)
            );
        }
        else
        {
            newItem = new Item(worldItemData);
        }

        return new ItemStack(newItem, Convert.ToInt32(entityInstance.FieldInstances[2].Value));
    }

    ItemStack CreateNoteItemStackFromLDtk(EntityInstance entityInstance)
    {
        ItemData worldItemData = itemDatabase.GetItemDataFromIdentifier("note");
        Item newItem;
        
        newItem = new Item(worldItemData);

        return new ItemStack(newItem);
    }

    NPCController SpawnNPC(LevelData levelData, GridNode spawnNode, NPCController npcPrefab, float rotationY, NPCMovementBehaviour movementBehaviour, int? healthOverride = null)
    {
        NPCController npc = Instantiate(
            npcPrefab,
            spawnNode.transform.position + centeredEntitySpawnOffset,
            Quaternion.Euler(0, rotationY, 0),
            spawnNode.transform
        );

        npc.InitNPC(levelData.LevelIndex, spawnNode);
        npc.SetMovementBehaviour(movementBehaviour);

        if (healthOverride.HasValue)
            npc.healthController.SetHealth(healthOverride.Value);

        levelData.AddNPC(npc);

        return npc;
    }
    NPCController GetNPCPrefabFromIdentifier(string identifier)
    {
        switch (identifier)
        {
            case "NPC_Zombie":
            case "zombie":
                return zombieNpcPrefab;

            case "NPC_Ranger":
            case "ranger":
                return rangerNpcPrefab;

            case "NPC_Bug":
            case "bug":
                return bugNpcPrefab;
        }

        return null;
    }

    void PrepareLevelForLoad(int levelIndexToLoad)
    {
        bool loadingSameLevel = currentLevelIndex == levelIndexToLoad;

        if (loadingSameLevel)
        {
            if (levelDataByIndex.TryGetValue(levelIndexToLoad, out LevelData sameLevel))
            {
                sameLevel.DestroyEntities();
                sameLevel.ClearRuntimeNodeOccupants();
            }

            return;
        }

        if (levelDataByIndex.TryGetValue(currentLevelIndex, out LevelData currentLevel))
        {
            currentLevel.DestroyEntities();
            currentLevel.ClearRuntimeNodeOccupants();
            currentLevel.SetLevelActive(false);
        }

        if (levelDataByIndex.TryGetValue(levelIndexToLoad, out LevelData newLevel))
        {
            newLevel.DestroyEntities();
            newLevel.ClearRuntimeNodeOccupants();
            newLevel.SetLevelActive(true);

            activeLevel = newLevel;
            currentLevelIndex = levelIndexToLoad;
        }
    }

    void ClearAllGeneratedEntities()
    {
        foreach (LevelData levelData in levelDataByIndex.Values)
        {
            levelData.DestroyEntities();
            levelData.ClearRuntimeNodeOccupants();
        }
    }

    #region LoadGame
    private void LoadGame(LevelSaveData data)
    {
        ClearAllGeneratedEntities();

        LoadLevels(data.levels);

        SetLevelActive(data.currentLevelIndex);

        SpawnPlayer();

        MovePlayer(data.playerCoords);

        Time.timeScale = 1;

        OnFinishedGeneratingLevel?.Invoke();
    }

    void LoadLevels(List<SaveableLevelData> loadableData)
    {
        Dictionary<int, SaveableLevelData> saveDataByLevelIndex = new Dictionary<int, SaveableLevelData>();

        foreach (SaveableLevelData saveData in loadableData)
        {
            saveDataByLevelIndex.Add(saveData.levelIndex, saveData);
        }

        for (int i = 0; i < LDtkLevels.Count; i++)
        {
            LevelData levelData = levelDataByIndex[i];

            if (saveDataByLevelIndex.TryGetValue(i, out SaveableLevelData saveData))
            {
                LevelGenerationContext context = LevelGenerationContext.Load(saveData);
                GenerateEntities(levelData, context);
            }
            else
            {
                LevelGenerationContext context = LevelGenerationContext.NewGame();
                GenerateEntities(levelData, context);
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

    public void BeginLevelTransition(int levelIndex, Vector2Int playerMoveToCoords)
    {
        SetLevelActive(levelIndex);

        MovePlayer(playerMoveToCoords);
    }

    void MovePlayer(Vector2Int coordsToMoveTo)
    {
        if (levelDataByIndex.TryGetValue(currentLevelIndex, out LevelData levelData))
        {
            GridNode node = levelData.GetNodeAtCoords(coordsToMoveTo);

            if (node == null)
            {
                Debug.LogWarning($"Could not move player. No node found at {coordsToMoveTo} on level {currentLevelIndex}.");
                return;
            }

            playerController.MoveToNode(node);
        }
    }

    void SetLevelActive(int levelIndex)
    {
        foreach (LevelData levelData in levelDataByIndex.Values)
        {
            levelData.SetLevelActive(false);
        }

        if (!levelDataByIndex.TryGetValue(levelIndex, out LevelData newActiveLevel))
            return;

        currentLevelIndex = levelIndex;

        newActiveLevel.SetLevelActive(true);

        activeLevel = newActiveLevel;
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
        foreach (LevelData levelData in levelDataByIndex.Values)
        {
            saveableLevelDatas.Add(new SaveableLevelData(levelData));
        }

        return saveableLevelDatas;
    }

    public void Save(ref SaveSystem.SaveData data)
    {
        data.gameTime = gameTime;
        data.LevelData.currentLevelIndex = currentLevelIndex;
        data.LevelData.currentLevelName = GetLevelNameFromIndex(currentLevelIndex);
        data.LevelData.playerCoords = playerController.currentOccupiedNode.Coords.Pos;
        data.LevelData.levels = GetSaveableLevelData();
    }

    public void Load(SaveSystem.SaveData data)
    {
        LoadGame(data.LevelData);
        gameTime = data.gameTime;
    }
}
