using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour, IInventory
{
    PlayerController playerController;
    [SerializeField]
    List<ItemStack> startingItemStacks = new List<ItemStack>(); 

    [SerializeField] InventorySlot slotToSpawn;
    public InventorySlot[] spawnedInventorySlots;
    [SerializeField] int totalNumInventorySlots;
    public static bool isInContainer { get; private set; }
    [SerializeField] int heldHealthSyringes;
    [Space]
    [Header("Camera Anim On Container Interaction")]
    [SerializeField] Vector3 openContainerCamPos, defaultCamPos;
    [SerializeField] Vector3 openContainerCamRot, defaultCamRot;
    [SerializeField] float openContainerCamMovementDuration, closeContainerCamMovementDuration;

    public static Action onInventoryOpened;
    public static Action onInventoryClosed;
    public static Action<InventorySlot[]> onInventorySlotsSpawned;
    public static Action<int> onSyringeCountUpdated;

    public static Action<AmmoItemData> onAmmoAddedToInventory;

    void OnEnable()
    {
        Container.onContainerOpened += OnContainerOpened;
        Container.onContainerClosed += OnContainerClosed;

        WorldInteractionManager.onNearbyContainerUpdated += OnNearbyContainerUpdated;

        InventoryContextMenu.onInventorySlotWeaponUnloaded += OnInventorySlotWeaponUnloaded;

        PauseMenu.onQuit += RemoveInventorySlots;
    }

    void OnDisable()
    {
        Container.onContainerOpened -= OnContainerOpened;
        Container.onContainerClosed -= OnContainerClosed;

        WorldInteractionManager.onNearbyContainerUpdated -= OnNearbyContainerUpdated;

        InventoryContextMenu.onInventorySlotWeaponUnloaded -= OnInventorySlotWeaponUnloaded;

        PauseMenu.onQuit -= RemoveInventorySlots;
    }

    void OnNearbyContainerUpdated(IContainer nearbyContainer)
    {
        if(nearbyContainer == null)
        {
            playerController.MoveCameraPos(defaultCamPos, closeContainerCamMovementDuration);
            playerController.RotCamera(defaultCamRot, closeContainerCamMovementDuration);
        }
    }

    async void OnContainerOpened()
    {
        playerController.MoveCameraPos(openContainerCamPos, openContainerCamMovementDuration);
        playerController.RotCamera(openContainerCamRot, openContainerCamMovementDuration);
        isInContainer = true;

        await Task.Delay((int)((openContainerCamMovementDuration / 2) * 1000));

        if (!PlayerInventoryUIController.isInventoryOpen)
            OpenInventory();
    }

    void OnContainerClosed()
    {
        playerController.MoveCameraPos(defaultCamPos, closeContainerCamMovementDuration);
        playerController.RotCamera(defaultCamRot, closeContainerCamMovementDuration);

        isInContainer = false;

        if(!PlayerInventoryUIController.isInventoryOpen)
            HelperFunctions.SetCursorActive(false);
    }

    void OnInventorySlotWeaponUnloaded(ISlot slot)
    {
        WeaponSlot weaponSlot = slot as WeaponSlot;
        if (!weaponSlot)
            return;

        ItemStack slotAmmo = new ItemStack(weaponSlot.GetWeapon().GetRangedWeapon().GetCurrentLoadedAmmoData(), slot.UnloadAmmo());
        TryAddItemToInventory(slotAmmo);

        
    }

    public void Init(PlayerController newPlayerController)
    {
        playerController = newPlayerController;

        SpawnInventorySlots();
        //HelperFunctions.SetCursorActive(false);
    }

    void SpawnInventorySlots()
    {
        spawnedInventorySlots = new InventorySlot[totalNumInventorySlots];

        for (int i = 0; i < totalNumInventorySlots; i++)
        {
            InventorySlot spawnedSlot = Instantiate(slotToSpawn);
            spawnedInventorySlots[i] = spawnedSlot;
            spawnedSlot.InitSlot(this, i);
        }

        AddStartingItems();

        onInventorySlotsSpawned?.Invoke(spawnedInventorySlots);
    }

    void RemoveInventorySlots()
    {
        foreach (var slot in spawnedInventorySlots)
        {
            Destroy(slot.gameObject);
        }

        Array.Clear(spawnedInventorySlots, 0, totalNumInventorySlots);
    }

    private void AddStartingItems()
    {
        for (int i = 0; i < startingItemStacks.Count; i++)
        {
            spawnedInventorySlots[i].AddItem(startingItemStacks[i]);
        }
    }

    public void ToggleInventory()
    {
        if (PlayerInventoryUIController.isInventoryOpen == true)
        {
            CloseInventory();
        }
        else if (PlayerInventoryUIController.isInventoryOpen == false)
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        onInventoryOpened?.Invoke();
    }

    public void CloseInventory()
    {
        onInventoryClosed?.Invoke();
    }

    public bool HasHealthSyringe()
    {
        if(heldHealthSyringes > 0)
            return true;
        else
            return false;
    }

    public void AddHealthSyringe(int amountToAdd)
    {
        heldHealthSyringes += amountToAdd;
        onSyringeCountUpdated?.Invoke(heldHealthSyringes);
    }

    private void RemoveAllSyringes()
    {
        RemoveHealthSyringe(heldHealthSyringes);
    }

    public void RemoveHealthSyringe(int amountToRemove)
    {
        heldHealthSyringes -= amountToRemove;
        onSyringeCountUpdated?.Invoke(heldHealthSyringes);
    }

    public InventorySlot FindSlotWithConsumableOfType(ConsumableType typeToFind)
    {
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.GetItemStack().itemData)
                continue;

            ConsumableItemData consumableData = slot.GetItemStack().itemData as ConsumableItemData;
            if (!consumableData)
                continue;

            if (consumableData.consumableType == typeToFind)
            {
                return slot;
            }
        }

        return null;
    }

    public InventorySlot GetNextFreeSlot()
    {
        foreach(InventorySlot slot in spawnedInventorySlots)
        {
            if(slot.IsSlotEmpty())
                return slot;
        }

        return null;
    }

    InventorySlot[] GetSlotOfTypeWithSpace(ItemData itemData)
    {
        List<InventorySlot> slotsWithItemAndSpace = new List<InventorySlot>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (!slot.IsSlotEmpty())
                continue;

            if(slot.GetItemStack().itemData == itemData)
            {
                if(slot.GetItemStack().GetRemainingSpaceInStack() > 0)
                {
                    slotsWithItemAndSpace.Add(slot);
                }
            }
        }

        if(slotsWithItemAndSpace.Count > 0)
            return slotsWithItemAndSpace.ToArray();

        return null;
    }

    public int TryAddItemToInventory(ItemStack itemToAdd)
    {
        InventorySlot[] slotsWithSpace = GetSlotOfTypeWithSpace(itemToAdd.itemData);
        if(slotsWithSpace != null && slotsWithSpace.Length > 0)
        {
            int remainingAmountToAdd = itemToAdd.itemAmount;
            foreach (InventorySlot slot in slotsWithSpace)
            {
                int spaceInSlot = slot.GetItemStack().GetRemainingSpaceInStack();
                if (spaceInSlot > remainingAmountToAdd)
                {
                    slot.AddToCurrentItemStack(remainingAmountToAdd);
                    remainingAmountToAdd = 0;
                    return remainingAmountToAdd;
                }

                int amountToAdd = spaceInSlot;
                slot.AddToCurrentItemStack(amountToAdd);
                remainingAmountToAdd -= amountToAdd;
            }

            if(remainingAmountToAdd > 0)
            {
                InventorySlot freeSlot = GetNextFreeSlot();
                if (freeSlot)
                {
                    freeSlot.AddItem(new ItemStack(itemToAdd.itemData, remainingAmountToAdd, itemToAdd.loadedAmmo));
                    return 0;
                }

                return remainingAmountToAdd;
            }

            return 0;
        }
        else
        {
            InventorySlot freeSlot = GetNextFreeSlot();
            if (freeSlot)
            {
                freeSlot.AddItem(itemToAdd);
                return 0;
            }

            return itemToAdd.itemAmount;
        }
    }

    public int TryGetRemainingAmmoOfType(AmmoItemData ammoTypeToGet)
    {
        int ammoToReturn = 0;
        //reverse list so it takes from the last slot first 
        List<InventorySlot> slotsReversed = new List<InventorySlot>(spawnedInventorySlots.Reverse());

        foreach (ISlot slot in slotsReversed)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            AmmoItemData ammoItemData = slotItemStack.itemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData != ammoTypeToGet)
                continue;

            ammoToReturn += slotItemStack.itemAmount;
            //Debug.Log(ammoToReturn);
        }
        return ammoToReturn;
    }

    public void DecreaseAmmoOfType(AmmoItemData ammoTypeToRemove, int amountToRemove)
    {
        //reverse list so it takes from the last slot first 
        List<InventorySlot> slotsReversed = new List<InventorySlot>(spawnedInventorySlots.Reverse());

        foreach (ISlot slot in slotsReversed)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            AmmoItemData ammoItemData = slotItemStack.itemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData != ammoTypeToRemove)
                continue;

            int remainingAmountToRemove = slot.RemoveFromExistingStack(amountToRemove);

            if (remainingAmountToRemove == 0)
                return;

            amountToRemove = remainingAmountToRemove;
        }
    }

    public void IncreaseAmmoOfType(AmmoItemData ammoTypeToAdd, int amountToAdd)
    {
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            AmmoItemData ammoItemData = slotItemStack.itemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData != ammoTypeToAdd)
                continue;

            int remainingAmountToAdd = slot.AddToCurrentItemStack(amountToAdd);
            if (remainingAmountToAdd > 0)
            {
                InventorySlot freeSlot = GetNextFreeSlot();
                if (freeSlot)
                {
                    freeSlot.AddItem(new ItemStack(ammoItemData, remainingAmountToAdd, 0));
                }
            }
            return;
        }
    }

    public void LockSlotsWithAmmoOfType(AmmoItemData ammoTypeToLock)
    {
        foreach(ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            AmmoItemData ammoItemData = slot.GetItemStack().itemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if(ammoItemData == ammoTypeToLock)
                slot.SetInteractable(false);
        }
    }

    public List<AmmoItemData> GetAllUseableAmmoForWeapon(IWeapon weapon)
    {
        List<AmmoItemData> heldAmmo = new List<AmmoItemData>();
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            AmmoItemData ammoItemData = slot.GetItemStack().itemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData.weaponTypes.Contains(weapon.GetWeaponData().weaponType))
            {
                if(!heldAmmo.Contains(ammoItemData))
                    heldAmmo.Add(ammoItemData);
            }
        }
        return heldAmmo;
    }

    public void UnlockSlots()
    {
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (!slot.IsInteractable())
                slot.SetInteractable(true);
        }
    }



    #region Save/Load

    public List<ItemStack> GetStoredItems()
    {
        List<ItemStack> items = new List<ItemStack>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            items.Add(slot.GetItemStack());
        }
        return items;
    }

    public void LoadItems(List<ItemStack> items)
    {
        RemoveAllSyringes();
        //RemoveAllAmmo();

        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if(!slot.IsSlotEmpty())
            {
                slot.RemoveItem();
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            spawnedInventorySlots[i].AddItem(items[i]);
        }
    }

    public void Save(ref PlayerSaveData data)
    {
        data.storedItems = GetStoredItems();
    }

    public void Load(PlayerSaveData data)
    {
        LoadItems(data.storedItems);
    }

    #endregion
}
