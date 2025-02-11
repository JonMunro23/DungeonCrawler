using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    [HideInInspector] public AdvancedGridMovement advGridMovement;
    [HideInInspector] public WorldInteractionManager itemPickupManager;
    [HideInInspector] public PlayerHealthManager playerHealthManager;
    [HideInInspector] public PlayerInventoryManager playerInventoryManager;
    [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
    [HideInInspector] public PlayerWeaponManager playerWeaponManager;
    [HideInInspector] public PlayerStatsManager playerStatsManager;
    [HideInInspector] public PlayerSkillsManager playerSkillsManager;
    [HideInInspector] public Camera playerCamera;

    [Header("Player Data")]
    public CharacterData playerCharacterData;
    public static GridNode currentOccupiedNode;
    public Rigidbody rb;
    public static bool isPlayerAlive;

    Vector3 defaultCamPos;

    public static Action<PlayerController> onPlayerInitialised;
    public static Action onPlayerDeath;

    private void OnEnable()
    {
        InventoryContextMenu.onHealSyringeUsed += OnHealSyringeUsed;
    }

    private void OnDisable()
    {
        InventoryContextMenu.onHealSyringeUsed -= OnHealSyringeUsed;
    }

    private void Awake()
    {
        advGridMovement = GetComponent<AdvancedGridMovement>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerWeaponManager = GetComponent<PlayerWeaponManager>();
        itemPickupManager = GetComponent<WorldInteractionManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerSkillsManager = GetComponent<PlayerSkillsManager>();
        playerCamera = GetComponentInChildren<Camera>();

        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        defaultCamPos = playerCamera.transform.localPosition;
    }

    void OnHealSyringeUsed(ISlot slot)
    {
        TryUseHealthSyringe(slot);
    }

    public void InitPlayer(CharacterData playerCharData/*, GridNode spawnGridNode*/)
    {
        isPlayerAlive = true;

        playerCharacterData = playerCharData;
        //currentOccupiedNode = spawnGridNode;
        itemPickupManager.Init(this);
        playerInventoryManager.Init(this);
        playerEquipmentManager.Init(this);
        playerWeaponManager.Init(this);
        playerStatsManager.Init(playerCharacterData);
        playerHealthManager.Init(this);
        playerSkillsManager.Init(playerCharacterData);
        advGridMovement.Init(this);

        onPlayerInitialised?.Invoke(this);
    }

    public void OnDeath()
    {
        isPlayerAlive = false;
        onPlayerDeath?.Invoke();
    }

    public void MoveToCoords(Vector2 newCoords)
    {
        //Debug.Log("Moving player to " + newCoords);

        GridNode nodeToMoveTo = GridController.Instance.GetNodeAtCoords(newCoords);
        if (!nodeToMoveTo)
            return;

        if(currentOccupiedNode)
            currentOccupiedNode.ClearOccupant();

        advGridMovement.Teleport(nodeToMoveTo.moveToTransform.position);
        nodeToMoveTo.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Player));
        SetCurrentOccupiedNode(nodeToMoveTo);
    }

    public async void TryUseHealthSyringe(ISlot slotToUse = null)
    {
        if (playerHealthManager.CanUseSyringe() && playerInventoryManager.HasHealthSyringe())
        {
            ISlot slotWithSyringe;
            if (slotToUse == null)
                slotWithSyringe = playerInventoryManager.FindSlotWithConsumableOfType(ConsumableType.HealSyringe);
            else
                slotWithSyringe = slotToUse;

            if (slotWithSyringe == null)
                return;

            if(playerWeaponManager.currentWeapon == null)
                return;

            playerHealthManager.canUseSyringe = false;

            await playerWeaponManager.currentWeapon.HolsterWeapon();

            playerHealthManager.UseSyringeInSlot(slotWithSyringe);
        }
    }

    public void TryUseCurrentWeapon()
    {
        if(!PlayerInventoryUIController.isInventoryOpen && !PlayerInventoryManager.isInContainer && !WorldInteractionManager.hasGrabbedItem)
        {
            playerWeaponManager.UseCurrentWeapon();
        }
    }

    public void TryUseCurrentWeaponSpecial()
    {
        if(WorldInteractionManager.hasGrabbedItem)
        {
            RemoveGrabbedItem();
            return;
        }

        if (!PlayerInventoryUIController.isInventoryOpen && !PlayerInventoryManager.isInContainer)
        {
            playerWeaponManager.UseCurrentWeaponSpecial();
        }
    }

    void RemoveGrabbedItem()
    {
        playerInventoryManager.TryAddItemToInventory(itemPickupManager.currentGrabbedItem);
        itemPickupManager.DetachItemFromMouseCursor();
        HelperFunctions.SetCursorActive(false);
    }

    public void TryReloadCurrentWeapon()
    {
        playerWeaponManager.ReloadCurrentWeapon();
    }


    public void SetCurrentOccupiedNode(GridNode newGridNode)
    {
        currentOccupiedNode = newGridNode;
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

    public void RemoveAudioSources()
    {
        AudioManager.Instance.RemoveSource("[AudioEmitter] Weapon");
        AudioManager.Instance.RemoveSource("[AudioEmitter] CharacterBody");
    }

    public void Save(ref PlayerSaveData data)
    {
        data.coords = currentOccupiedNode.Coords.Pos;
        data.yRotation = Mathf.RoundToInt(advGridMovement.GetTargetRot());

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
        advGridMovement.enabled = true;

        MoveToCoords(data.coords);
        advGridMovement.SetRotation(Mathf.RoundToInt(data.yRotation));

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
