using DG.Tweening;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public AdvancedGridMovement advGridMovement;
    public ItemPickupManager itemPickupManager;
    public PlayerHealthManager playerHealthController;
    public PlayerInventoryManager playerInventoryManager;
    public PlayerEquipmentManager playerEquipmentManager;
    public PlayerWeaponManager playerWeaponManager;
    public PlayerStatsManager playerStatsManager;
    public PlayerSkillsManager playerSkillsManager;
    [HideInInspector] public Camera playerCamera;

    [Header("Player Data")]
    public CharacterData playerCharacterData;
    public static GridNode currentOccupiedNode;

    [SerializeField] float fadeOutDuration, fadeInDuration;
    
    Vector3 defaultCamPos;

    public static Action<PlayerController> onPlayerInitialised;
    public static Action onPlayerDeath;

    private void Awake()
    {
        advGridMovement = GetComponent<AdvancedGridMovement>();
        playerHealthController = GetComponent<PlayerHealthManager>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerWeaponManager = GetComponent<PlayerWeaponManager>();
        itemPickupManager = GetComponent<ItemPickupManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerSkillsManager = GetComponent<PlayerSkillsManager>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        defaultCamPos = playerCamera.transform.localPosition;
    }

    public void InitPlayer(CharacterData playerCharData/*, GridNode spawnGridNode*/)
    {
        playerCharacterData = playerCharData;
        //currentOccupiedNode = spawnGridNode;

        playerInventoryManager.Init(this);
        playerEquipmentManager.Init(this);
        playerWeaponManager.Init(this);
        playerStatsManager.Init(playerCharacterData);
        playerHealthController.Init(this);
        playerSkillsManager.Init();
        advGridMovement.Init(this);

        onPlayerInitialised?.Invoke(this);
    }

    public void OnDeath()
    {
        //play death anim
        //show game over UI
        //disable all player input

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

    public async void TryUseHealthSyringe()
    {
        if (playerHealthController.CanUseSyringe() && playerInventoryManager.HasHealthSyringe())
        {
            InventorySlot slotWithSyringe = playerInventoryManager.FindSlotWithConsumableOfType(ConsumableType.HealSyringe);
            if (!slotWithSyringe)
                return;

            if(playerWeaponManager.currentWeapon == null)
                return;

            await playerWeaponManager.currentWeapon.HolsterWeapon();

            playerHealthController.UseSyringeInSlot(slotWithSyringe);
        }
    }

    public void TryUseCurrentWeapon()
    {
        if(!PlayerInventoryUIController.isInventoryOpen && !PlayerInventoryManager.isInContainer && !itemPickupManager.hasGrabbedItem)
        {
            playerWeaponManager.UseCurrentWeapon();
        }
    }

    public void TryUseCurrentWeaponSpecial()
    {
        if (!PlayerInventoryUIController.isInventoryOpen && !PlayerInventoryManager.isInContainer && !itemPickupManager.hasGrabbedItem)
        {
            playerWeaponManager.UseCurrentWeaponSpecial();
        }
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

    public void Save(ref PlayerSaveData data)
    {
        data.coords = currentOccupiedNode.Coords.Pos;
        data.yRotation = Mathf.RoundToInt(advGridMovement.GetTargetRot());

        if (playerHealthController)
            playerHealthController.Save(ref data);

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
        MoveToCoords(data.coords);
        advGridMovement.SetRotation(Mathf.RoundToInt(data.yRotation));

        if(playerStatsManager)
            playerStatsManager.Load();

        if(playerSkillsManager)
            playerSkillsManager.Load(data);

        if(playerHealthController)
            playerHealthController.Load(data);

        if(playerInventoryManager)
            playerInventoryManager.Load(data);

        if(playerEquipmentManager)
            playerEquipmentManager.Load(data);

        if(playerWeaponManager)
            playerWeaponManager.Load(data);

    }
}
