using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct PlayerSaveData
{
    //Movement Data
    public Vector2 coords;
    public float yRotation;

    //Health data
    public int currentHealth;

    //Inventory Data
    public List<ItemStack> storedItems;

    //Equipment Data
    public List<EquippedItem> equippedItems;

    //Weapon Data
    public int activeWeaponSlotIndex;
    public List<WeaponSlotData> weaponSlotData;

    //Skill Data
    public int availableSkillPoints;
    public List<UnlockedSKillData> unlockedSkills;
}

[SelectionBase]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    //[HideInInspector] public AdvancedGridMovement advGridMovement;
    [HideInInspector] public PlayerMovementManager playerMovementManager;
    [HideInInspector] public WorldInteractionManager itemPickupManager;
    [HideInInspector] public PlayerHealthManager playerHealthManager;
    [HideInInspector] public PlayerStatusEffectManager playerStatusEffectManager;
    [HideInInspector] public PlayerInventoryManager playerInventoryManager;
    [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
    [HideInInspector] public PlayerWeaponManager playerWeaponManager;
    [HideInInspector] public PlayerThrowableManager playerThrowableManager;
    [HideInInspector] public PlayerStatsManager playerStatsManager;
    [HideInInspector] public PlayerSkillsManager playerSkillsManager;
    [HideInInspector] public PlayerLevelManager playerLevelManager;
    [HideInInspector] public FreeCameraMovement cameraMovement;
    [HideInInspector] public Camera playerCamera;
    PlayerControls playerControls;

    [Header("Player Data")]
    public CharacterData playerCharacterData;
    public GridNode currentOccupiedNode;
    public Rigidbody rb;
    public static bool isPlayerAlive;
    Vector3 defaultCamPos;

    public static event Action<PlayerController> onPlayerInitialised;
    public static event Action onPlayerDeath;
    public static event Action<GridNode> onPlayerOccupiedNodeUpdated;

    private void OnEnable()
    {
        InventoryContextMenu.onHealSyringeUsedFromContextMenu += OnHealSyringeUsedFromContextMenu;
        PlayerInputHandler.OnPlayerControlsInitialised += OnPlayerControlsInitialised;
    }

    private void OnDisable()
    {
        InventoryContextMenu.onHealSyringeUsedFromContextMenu -= OnHealSyringeUsedFromContextMenu;
        PlayerInputHandler.OnPlayerControlsInitialised -= OnPlayerControlsInitialised;
    }

    private void Awake()
    {
        playerMovementManager = GetComponent<PlayerMovementManager>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
        playerStatusEffectManager = GetComponent<PlayerStatusEffectManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerWeaponManager = GetComponent<PlayerWeaponManager>();
        playerThrowableManager = GetComponent<PlayerThrowableManager>();
        itemPickupManager = GetComponent<WorldInteractionManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerSkillsManager = GetComponent<PlayerSkillsManager>();
        playerLevelManager = GetComponent<PlayerLevelManager>();
        cameraMovement = GetComponentInChildren<FreeCameraMovement>();
        playerCamera = GetComponentInChildren<Camera>();

        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        defaultCamPos = playerCamera.transform.localPosition;
    }

    private void Update()
    {
        InputHandling();
    }

    void InputHandling()
    {
        if (MapController.isMapOpen) return;

        playerMovementManager.HandleMovementInput();

        if (playerControls.Player.LeftClick.IsPressed())
            TryUseCurrentWeapon();

        if (playerControls.Player.RightClick.WasPressedThisFrame())
            TryReadyCurrentWeapon();
        else if (playerControls.Player.RightClick.WasReleasedThisFrame())
            TryUnreadyCurrentWeapon();

        if (playerControls.Player.Heal.WasPressedThisFrame())
            TryUseHealthSyringe(playerInventoryManager.FindSlotWithConsumableOfType(ConsumableType.HealSyringe));

        if (playerControls.Player.EquipThrowable.WasPressedThisFrame())
            TryEquipThrowable();

        if (playerControls.Player.SwapWeapon.WasPressedThisFrame())
            TrySwapWeapons();

        if (playerControls.Player.Reload.WasPerformedThisFrame())
        {
            TryOpenAmmoSelectionMenu();
        }
        else if (playerControls.Player.Reload.WasReleasedThisFrame())
        {
            if (playerWeaponManager.isAmmoSelectionMenuOpen)
            {
                TryCloseAmmoSelectionMenu();
            }
            else
                TryReloadCurrentWeapon();
        }

        if (playerControls.Player.EquipThrowable.WasPerformedThisFrame())
        {
            TryOpenThrowableSelectionMenu();
        }
        else if (playerControls.Player.EquipThrowable.WasReleasedThisFrame())
        {
            if (playerThrowableManager.isThrowableSelectionMenuOpen)
            {
                TryCloseThrowableSelectionMenu();
            }
            else
                TryEquipThrowable();
        }
    }

    void OnPlayerControlsInitialised(PlayerControls controls)
    {
        playerControls = controls;
    }

    void OnHealSyringeUsedFromContextMenu(ISlot slot)
    {
        TryUseHealthSyringe(slot);
    }

    public void OnDeath()
    {
        isPlayerAlive = false;
        onPlayerDeath?.Invoke();
    }

    public void InitPlayer(CharacterData playerCharData)
    {
        isPlayerAlive = true;

        playerCharacterData = playerCharData;
        //currentOccupiedNode = spawnGridNode;
        itemPickupManager.Init(this);
        playerInventoryManager.Init(this);
        playerEquipmentManager.Init(this);
        playerWeaponManager.Init(this);
        playerThrowableManager.Init(this);
        playerStatsManager.Init(playerCharacterData);
        playerHealthManager.Init(this);
        playerSkillsManager.Init(playerCharacterData);
        playerMovementManager.Init(this);
        cameraMovement.Init(this);
        onPlayerInitialised?.Invoke(this);
    }

    public void MoveToCoords(Vector2 newCoords)
    {
        //Debug.Log("Moving player to " + newCoords);

        GridNode nodeToMoveTo = GridController.Instance.GetNodeAtCoords(newCoords);
        if (!nodeToMoveTo)
            return;

        SetCurrentOccupiedNode(nodeToMoveTo);
        playerMovementManager.Teleport(nodeToMoveTo.moveToTransform.position);
    }

    async void TryEquipThrowable()
    {
        await playerThrowableManager.ToggleEquipThrowable();
    }

    async void TryUseHealthSyringe(ISlot slotToUse)
    {
        if (playerHealthManager.CanUseSyringe() && playerInventoryManager.HasHealthSyringe())
        {
            if (slotToUse == null)
                return;

            if(playerWeaponManager.currentWeapon == null)
                return;

            playerHealthManager.canUseSyringe = false;

            await playerWeaponManager.currentWeapon.HolsterWeapon();

            playerHealthManager.UseSyringeInSlot(slotToUse);
        }
    }

    void TryUseCurrentWeapon()
    {
        if (CharacterMenuUIController.isCharacterMenuOpen ||
            PlayerInventoryManager.isInContainer ||
            WorldInteractionManager.hasGrabbedItem ||
            ThrowableSelectionManager.isThrowableSelectionMenuOpen)
            return;

        if (playerThrowableManager.IsThrowableActive())
        {
            _ = playerThrowableManager.UseThrowable();
            return;
        }

         playerWeaponManager.UseCurrentWeapon();
    }

    void TryReadyCurrentWeapon()
    {
        if (PlayerInventoryManager.isInContainer) return;
        if (WorldInteractionManager.hasGrabbedItem) return;
        if (CharacterMenuUIController.isCharacterMenuOpen) return;
        //Return if map is open

        if (playerThrowableManager.IsThrowableActive())
        {
            playerThrowableManager.ReadyThrowable();
            return;
        }

        playerWeaponManager.ReadyWeapon();
    }

    void TryUnreadyCurrentWeapon()
    {
        if (playerThrowableManager.IsThrowableActive())
        {
            playerThrowableManager.UnreadyThrowable();
            return;
        }

        playerWeaponManager.UnreadyWeapon();
    }

    void TryReloadCurrentWeapon()
    {
        playerWeaponManager.ReloadCurrentWeapon();
    }

    void TryOpenAmmoSelectionMenu()
    {
        playerWeaponManager.OpenAmmoSelectionMenu();
    }

    void TryCloseAmmoSelectionMenu()
    {
        playerWeaponManager.CloseAmmoSelectionMenu();
    }
    void TrySwapWeapons()
    {
        playerWeaponManager.SwapWeapons();
    }

    void TryOpenThrowableSelectionMenu()
    {
        playerThrowableManager.OpenThrowableSelectionMenu();
    }

    void TryCloseThrowableSelectionMenu()
    {
        playerThrowableManager.CloseThrowableSelectionMenu();
    }

    void RemoveGrabbedItem()
    {
        playerInventoryManager.TryAddItem(itemPickupManager.currentGrabbedItem);
        itemPickupManager.DetachItemFromMouseCursor();
        HelperFunctions.SetCursorActive(false);
    }

    public void SetCurrentOccupiedNode(GridNode newGridNode)
    {
        if(currentOccupiedNode)
            currentOccupiedNode.ResetOccupant();

        currentOccupiedNode = newGridNode;
        currentOccupiedNode.SetSelfAndSurroundingNodesExplored();
        currentOccupiedNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Player));
        //playerMovementManager.currentNode = currentOccupiedNode;

        onPlayerOccupiedNodeUpdated?.Invoke(currentOccupiedNode);
    }

    public void ShakeScreen()
    {
        playerCamera.DOShakePosition(.35f, .5f).OnComplete(() =>
        {
            playerCamera.transform.DOLocalMove(defaultCamPos, .1f);
        });
    }

    public void MoveCameraPos(Vector3 newPos, float overDuration)
    {
        playerCamera.transform.DOLocalMove(newPos, overDuration);
    }

    public void RotCamera(Vector3 newRot, float overDuration)
    {
        playerCamera.transform.DOLocalRotate(newRot, overDuration);
    }

    public float GetCurrentYRotation()
    {
        return transform.localEulerAngles.y;
    }

    public PlayerControls GetPlayerControls() => playerControls;

    public void RemoveAudioSources()
    {
        AudioManager.Instance.RemoveSource("[AudioEmitter] Weapon");
        AudioManager.Instance.RemoveSource("[AudioEmitter] CharacterBody");
    }

    public void Save(ref PlayerSaveData data)
    {
        data.coords = currentOccupiedNode.Coords.Pos;
        data.yRotation = transform.localEulerAngles.y;

        if (playerHealthManager)
            playerHealthManager.Save(ref data);

        if (playerInventoryManager)
            playerInventoryManager.Save(ref data);

        if (playerEquipmentManager)
            playerEquipmentManager.Save(ref data);

        if (playerWeaponManager)
            playerWeaponManager.Save(ref data);

        if (playerSkillsManager)
            playerSkillsManager.Save(ref data);
    }

    public void Load(PlayerSaveData data)
    {
        isPlayerAlive = true;
        rb.isKinematic = true;
        playerMovementManager.enabled = true;

        MoveToCoords(data.coords);
        //advGridMovement.SetRotation(Mathf.RoundToInt(data.yRotation));

        if(playerStatsManager)
            playerStatsManager.Load();

        if(playerSkillsManager)
            playerSkillsManager.Load(data);

        if(playerHealthManager)
            playerHealthManager.Load(data);

        if(playerInventoryManager)
            playerInventoryManager.Load(data);

        if(playerEquipmentManager)
            playerEquipmentManager.Load(data);

        if(playerWeaponManager)
            playerWeaponManager.Load(data);

    }
}
