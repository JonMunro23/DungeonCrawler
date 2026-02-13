using LDtkUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public static float gridSize = 3;
    // Per-level index lookup (fast + correct)
    Dictionary<int, GridNode[]> nodesByIndexPerLevel = new Dictionary<int, GridNode[]>();


    [Header("Levels")]
    [SerializeField] int startingLevelIndex;
    [SerializeField] int currentLevelIndex;
    /// <summary>
    /// int = levelIndex
    /// </summary>
    Dictionary<int, LevelData> levelDataDictionary = new Dictionary<int, LevelData>();
    [SerializeField] List<Transform> levelParents = new List<Transform>();


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
    [SerializeField] PressurePlate pressurePlatePrefab;
    [SerializeField] Tripwire tripwirePrefab;
    [SerializeField] ShootableTarget shootableTargetPrefab;
    [SerializeField] PlayerTrigger playerTriggerPrefab;
    [SerializeField] Sign signPrefab;
    List<IInteractable> spawnedInteractables = new List<IInteractable>();

    [Header("Triggerables")]
    [SerializeField] Door doorPrefab;
    [SerializeField] Door secretDoorPrefab;
    [SerializeField] NPCSpawner npcSpawnerPrefab;
    List<ITriggerable> spawnedTriggerables = new List<ITriggerable>();

    [Header("Destructables")]
    [SerializeField] Destructable destructableWallPrefab;

    [Header("Spawn Offsets")]
    [SerializeField] Vector3 centeredEntitySpawnOffset;
    [SerializeField] Vector3 worldItemSpawnOffset;

    GridNode[] nodesByIndex;
    public GridNode GetNodeByIndex(int idx) => (nodesByIndex != null && idx >= 0 && idx < nodesByIndex.Length) ? nodesByIndex[idx] : null;
    public int CurrentNodeCount => nodesByIndex != null ? nodesByIndex.Length : 0;


    public static Action onQuickSave;
    public static event Action onLevelFinishedGenerating;

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

        onLevelFinishedGenerating?.Invoke();
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

        Transform levelParent = new GameObject($"Level {levelIndex}").transform;
        levelParent.SetParent(transform);
        levelParents.Add(levelParent);

        GenerateLevel(levelIndex);

        LinkInteractablesToTriggerables();
        CacheGridNodeNeighbours();
    }

    void GenerateLevel(int levelIndex)
    {
        int nodeIndex = 0;
        Vector2 spawnCoords = Vector2.zero;
        Transform nodeParent = levelParents[levelIndex];

        // allocate once per level
        GridNode[] levelNodesByIndex = new GridNode[intGridLayer.IntGridCsv.Length];

        for (int i = 0; i < intGridLayer.CWid; i++)
        {
            for (int j = 0; j < intGridLayer.CHei; j++)
            {
                GridNode clone = null;

                spawnCoords = new Vector2(-i, j);
                SquareCoords sqCoords = new SquareCoords { Pos = new Vector2(-i, j) };

                switch (intGridLayer.IntGridCsv[nodeIndex])
                {
                    case 1:
                        clone = Instantiate(wallPrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, nodeParent);
                        clone.transform.localPosition += new Vector3(-1.5f, 1.5f, -1.5f);
                        break;
                    case 2:
                        clone = Instantiate(walkablePrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, nodeParent);
                        break;
                    case 3:
                        clone = Instantiate(voidPrefab, grid.GetCellCenterLocal(new Vector3Int(-i, j)), Quaternion.identity, nodeParent);
                        clone.SetIsVoid(true);
                        break;
                }

                clone.InitNode(sqCoords, nodeIndex);

                // ✅ fill the per-level array
                levelNodesByIndex[nodeIndex] = clone;

                activeNodes.Add(spawnCoords, clone);

                Vector2 loopIndices = new Vector2(i, j);
                GenerateEntities(levelIndex, loopIndices, spawnCoords);

                nodeIndex++;
            }
        }

        // ✅ store this level’s node lookup
        nodesByIndexPerLevel[levelIndex] = levelNodesByIndex;
    }


    void GenerateEntities(int levelIndex, Vector2 loopIndices, Vector2 spawnCoords)
    {
        GridNodeOccupant newOccupant;
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
                        WorldItem spawnedWorldItem = Instantiate(worldItemPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        spawnedWorldItems.Add(spawnedWorldItem);
                        ItemData worldItemData = itemDataContainer.GetDataFromIdentifier(entityLayer.EntityInstances[k].FieldInstances[1].Value.ToString());
                        spawnedWorldItem.InitWorldItem(levelIndex, spawnCoords, new ItemStack(worldItemData, Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[2].Value), Convert.ToInt32(entityLayer.EntityInstances[k].FieldInstances[3].Value)));
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
                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(itemData, itemAmount)));
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
                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(itemData, itemAmount)));

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
                                    spawnedContainer.AddNewStoredItemStack(new ContainerItemStack(l, new ItemStack(itemData, itemAmount)));

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
                        GridNodeOccupant occupant = new GridNodeOccupant(spawnedDestructable.gameObject, GridNodeOccupantType.Obstacle);
                        spawnNode.SetOccupant(occupant);
                        break;
                    case "Sign":
                        Sign sign = Instantiate(signPrefab, spawnNode.transform.position, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]) + 180, 0)), spawnNode.transform);
                        sign.SetSignText(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[1].Value));
                        //interactable = sign;
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
                        interactable = Instantiate(keycardReaderPrefab, spawnNode.transform.position + centeredEntitySpawnOffset, Quaternion.Euler(new Vector3(0, DecideSpawnDir(entityLayer.EntityInstances[k]), 0)), spawnNode.transform);
                        interactable.SetRequiredKeycardType(LDtkFieldHelper.GetValue<string>(entityLayer.EntityInstances[k].FieldInstances[4].Value));
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
        GenerateLevel(levelIndex);
        //GenerateEntities(levelIndex);
        //GenerateNPCs(levelIndex);

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

        // switch lookup to this level
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
            NPC.SnapToNode(NPC.movementController.CurrentNavTargetNode);
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
