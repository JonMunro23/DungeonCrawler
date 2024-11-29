using DG.Tweening;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public AdvancedGridMovement advGridMovement;
    public ItemPickupManager itemPickupManager;
    public PlayerHealthController playerHealthController;
    public PlayerInventoryManager playerInventoryManager;
    public PlayerEquipmentManager playerEquipmentManager;
    public PlayerWeaponManager playerWeaponManager;
    public PlayerStatsManager playerStatsManager;
    [HideInInspector] public Camera playerCamera;

    [Header("Player Data")]
    public CharacterData playerCharacterData;
    public static GridNode currentOccupiedNode;

    [SerializeField] float fadeOutDuration, fadeInDuration;
    
    Vector3 defaultCamPos;

    public static Action<PlayerController> onPlayerInitialised;
    public static Action<float> fadeOutScreen, fadeInScreen;

    private void Awake()
    {
        advGridMovement = GetComponent<AdvancedGridMovement>();
        playerHealthController = GetComponent<PlayerHealthController>();
        playerInventoryManager = GetComponent<PlayerInventoryManager>();
        playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        playerWeaponManager = GetComponent<PlayerWeaponManager>();
        itemPickupManager = GetComponent<ItemPickupManager>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        defaultCamPos = playerCamera.transform.localPosition;
    }

    public void InitPlayer(CharacterData playerCharData, GridNode spawnGridNode)
    {
        playerCharacterData = playerCharData;
        currentOccupiedNode = spawnGridNode;

        playerInventoryManager.InitInventory(this);
        playerWeaponManager.Init(this);
        playerStatsManager.InitPlayerStats(playerCharacterData);
        playerHealthController.InitHealthController(this);
        advGridMovement.InitMovement(this);

        onPlayerInitialised?.Invoke(this);
    }

    public void MoveToCoords(Vector2 newCoords)
    {
        //Debug.Log("Moving player to " + newCoords);

        GridNode nodeToMoveTo = GridController.Instance.GetNodeAtCoords(newCoords);
        if (!nodeToMoveTo)
            return;

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
        if(!playerInventoryManager.isInventoryOpen && !playerInventoryManager.isInContainer && !itemPickupManager.hasGrabbedItem)
        {
            playerWeaponManager.UseCurrentWeapon();
        }
    }

    public void TryUseCurrentWeaponSpecial()
    {
        if (!playerInventoryManager.isInventoryOpen && !playerInventoryManager.isInContainer && !itemPickupManager.hasGrabbedItem)
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

    async public Task FadeOutScreen()
    {
        fadeOutScreen?.Invoke(fadeOutDuration);
        await Task.Delay((int)(fadeOutDuration * 1000));
    }

    async public Task FadeInScreen()
    {
        fadeInScreen?.Invoke(fadeInDuration);
        await Task.Delay((int)(fadeInDuration * 1000));
    }

    public void MoveCameraPos(Vector3 newPos, float overDuration)
    {
        playerCamera.transform.DOLocalMove(newPos, overDuration);
    }

    public void RotCamera(Vector3 newRot, float overDuration)
    {
        playerCamera.transform.DOLocalRotate(newRot, overDuration);
    }
}
