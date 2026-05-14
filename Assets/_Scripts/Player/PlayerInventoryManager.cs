using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour, IInventory
{
    public PlayerController playerController;
    [SerializeField]
    List<ItemStack> startingItemStacks = new List<ItemStack>(); 

    [SerializeField] InventorySlot inventorySlotPrefab;
    static InventorySlot[] spawnedInventorySlots;
    [SerializeField] int totalNumInventorySlots;
    [SerializeField] int heldHealthSyringes;
    [Space]
    [Header("Camera Anim On Container Interaction")]
    [SerializeField] Vector3 openContainerCamPos;
    [SerializeField] Vector3 returnCamPos;
    [SerializeField] Vector3 openContainerCamRot;
    [SerializeField] Vector3 returnCamRot;
    [SerializeField] float openContainerCamMovementDuration, closeContainerCamMovementDuration;
    public static bool isInContainer { get; private set; }

    bool hasCollectedFirstThrowable;

    public static event Action onInventoryOpened;
    public static event Action onInventoryClosed;
    public static event Action<InventorySlot[]> onInventorySlotsSpawned;
    public static event Action<int> onSyringeCountUpdated;
    public static event Action<AmmoItemData> onAmmoAddedToInventory;
    public static event Action<ThrowableItemData> onThrowableRemoved;

    void OnEnable()
    {
        Container.onContainerOpened += OnContainerOpened;
        Container.onContainerClosed += OnContainerClosed;

        //WorldInteractionManager.onNearbyContainerUpdated += OnNearbyContainerUpdated;

        InventoryContextMenu.onInventorySlotWeaponUnloaded += OnInventorySlotWeaponUnloaded;

        PauseMenu.onQuit += RemoveInventorySlots;
    }

    void OnDisable()
    {
        Container.onContainerOpened -= OnContainerOpened;
        Container.onContainerClosed -= OnContainerClosed;

        //WorldInteractionManager.onNearbyContainerUpdated -= OnNearbyContainerUpdated;

        InventoryContextMenu.onInventorySlotWeaponUnloaded -= OnInventorySlotWeaponUnloaded;

        PauseMenu.onQuit -= RemoveInventorySlots;
    }

    //void OnNearbyContainerUpdated(IContainer nearbyContainer)
    //{
    //    if(nearbyContainer == null)
    //    {
    //        playerController.MoveCameraPos(defaultCamPos, closeContainerCamMovementDuration);
    //        playerController.RotCamera(defaultCamRot, closeContainerCamMovementDuration);
    //    }
    //}

    void OnContainerOpened()
    {
        returnCamPos = Camera.main.transform.localPosition;
        returnCamRot = Camera.main.transform.localEulerAngles;

        playerController.MoveCameraPos(openContainerCamPos, openContainerCamMovementDuration);
        playerController.RotCamera(openContainerCamRot, openContainerCamMovementDuration);
        isInContainer = true;

        //await Task.Delay((int)((openContainerCamMovementDuration / 2) * 1000));

        if (!CharacterMenuUIController.isCharacterMenuOpen)
            OpenInventory();
    }

    void OnContainerClosed()
    {
        playerController.MoveCameraPos(returnCamPos, closeContainerCamMovementDuration);
        playerController.RotCamera(returnCamRot, closeContainerCamMovementDuration);
        isInContainer = false;

        HelperFunctions.SetCursorActive(false);
    }

    void OnInventorySlotWeaponUnloaded(ISlot slot)
    {
        // Add ability to unload whilst weapon is within a weapon slot, need to add an animation for unloading the weapon
        //WeaponSlot weaponSlot = slot as WeaponSlot;
        //if(weaponSlot != null )
        //{
        //    weaponSlot.GetWeapon().GetRangedWeapon().UnloadWeapon();
        //}

        int unloadedAmmoAmount = 0;
        AmmoItemData unloadedAmmoType = null;
        WeaponItem slotWeaponItem = slot.GetItemStack().Item as WeaponItem;
        if(slotWeaponItem != null)
        {
            unloadedAmmoType = slotWeaponItem.LoadedAmmoData;
            unloadedAmmoAmount = slot.UnloadAmmo();
        }
        ItemStack unloadedAmmoItemStack = new ItemStack(new Item(unloadedAmmoType), unloadedAmmoAmount);

        TryAddItem(unloadedAmmoItemStack);

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
            InventorySlot spawnedSlot = Instantiate(inventorySlotPrefab);
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

        if (CharacterMenuUIController.isCharacterMenuOpen == true)
        {
            CloseInventory();
        }
        else if (CharacterMenuUIController.isCharacterMenuOpen == false)
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

    public void AddAmmo(AmmoItemData ammoData)
    {
        onAmmoAddedToInventory?.Invoke(ammoData);
    }

    public void RemoveThrowableOfType(ThrowableItemData throwableToRemove, int amountToRemove)
    {
        //reverse list so it takes from the last slot first 
        List<InventorySlot> slotsReversed = new List<InventorySlot>(spawnedInventorySlots.Reverse());

        foreach (ISlot slot in slotsReversed)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            ThrowableItemData throwableItemData = slotItemStack.Item.ItemData as ThrowableItemData;
            if (!throwableItemData)
                continue;

            if (throwableItemData != throwableToRemove)
                continue;

            slot.RemoveFromExistingStack(amountToRemove);
            onThrowableRemoved?.Invoke(throwableToRemove);
        }
    }

    public InventorySlot FindSlotWithConsumableOfType(ConsumableType typeToFind)
    {
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            Item item = slot.GetItemStack().Item;
            if (item == null)
                continue;

            if (!item.ItemData)
                continue;

            ConsumableItemData consumableData = item.ItemData as ConsumableItemData;
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

    InventorySlot[] GetSlotWithItemWithSpace(ItemData itemData)
    {
        List<InventorySlot> slotsWithItemAndSpace = new List<InventorySlot>();
        foreach (InventorySlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            if(slot.GetItemStack().Item.ItemData == itemData)
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

    public int TryAddItem(ItemStack itemToAdd)
    {
        InventorySlot[] slotsWithSpace = GetSlotWithItemWithSpace(itemToAdd.Item.ItemData);
        if(slotsWithSpace != null && slotsWithSpace.Length > 0)
        {
            int remainingAmountToAdd = itemToAdd.ItemAmount;
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
                     freeSlot.AddItem(new ItemStack(itemToAdd.Item, remainingAmountToAdd));

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

            return itemToAdd.ItemAmount;
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

            AmmoItemData ammoItemData = slotItemStack.Item.ItemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData != ammoTypeToGet)
                continue;

            ammoToReturn += slotItemStack.ItemAmount;
            //Debug.Log(ammoToReturn);
        }
        return ammoToReturn;
    }

    public void DecreaseAmmoOfType(AmmoItemData ammoTypeToRemove, int amountToRemove)
    {
        //reverse list so it takes from the last slot first 
        List<InventorySlot> slotsReversed = new List<InventorySlot>(spawnedInventorySlots.Reverse());
        int remainingAmountToRemove = amountToRemove;
        foreach (ISlot slot in slotsReversed)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            AmmoItemData ammoItemData = slotItemStack.Item.ItemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData != ammoTypeToRemove)
                continue;

            remainingAmountToRemove = slot.RemoveFromExistingStack(remainingAmountToRemove);

            if (remainingAmountToRemove == 0)
                return;

            //amountToRemove = remainingAmountToRemove;
        }
    }

    public void IncreaseAmmoOfType(AmmoItemData ammoTypeToAdd, int amountToAdd)
    {
        int remainingAmountToAdd = amountToAdd;
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            ItemStack slotItemStack = slot.GetItemStack();

            AmmoItemData ammoItemData = slotItemStack.Item.ItemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData != ammoTypeToAdd)
                continue;

            remainingAmountToAdd = slot.AddToCurrentItemStack(amountToAdd);
            if (remainingAmountToAdd == 0)
                return;
        }

        if(remainingAmountToAdd > 0)
        {
            InventorySlot freeSlot = GetNextFreeSlot();
            if (freeSlot)
            {
                freeSlot.AddItem(new ItemStack(new Item(ammoTypeToAdd), remainingAmountToAdd));
            }
        }
    }

    public static int GetRemainingAmountOfItem(ItemData itemData)
    {
        int itemAmount = 0;
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            Item slotItem = slot.GetItemStack().Item;
            if (slotItem == null)
                continue;

            if (slotItem.ItemData != itemData)
                continue;

            itemAmount += slot.GetItemStack().ItemAmount;
        }

        return itemAmount;
    }

    public void LockSlotsWithAmmoOfType(AmmoItemData ammoTypeToLock)
    {
        foreach(ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            AmmoItemData ammoItemData = slot.GetItemStack().Item.ItemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if(ammoItemData == ammoTypeToLock)
                slot.SetInteractable(false);
        }
    }

    public List<AmmoItemData> GetAllUseableAmmoTypesForWeapon(IWeapon weapon)
    {
        List<AmmoItemData> heldAmmoTypes = new List<AmmoItemData>();
        
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            AmmoItemData ammoItemData = slot.GetItemStack().Item.ItemData as AmmoItemData;
            if (!ammoItemData)
                continue;

            if (ammoItemData.weaponTypes.Contains(weapon.GetWeaponData().weaponType))
            {
                if(!heldAmmoTypes.Contains(ammoItemData))
                    heldAmmoTypes.Add(ammoItemData);
            }
        }

        AmmoItemData loadedAmmoData = playerController.playerWeaponManager.currentWeapon.GetRangedWeapon().GetCurrentLoadedAmmoData();
        if (!heldAmmoTypes.Contains(loadedAmmoData))
            heldAmmoTypes.Add(loadedAmmoData);

        return heldAmmoTypes;
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

    public List<ThrowableItemData> GetAllAvailableThrowables()
    {
        List<ThrowableItemData> heldThrowables = new List<ThrowableItemData>();
        foreach (ISlot slot in spawnedInventorySlots)
        {
            if (slot.IsSlotEmpty())
                continue;

            ThrowableItemData throwableItemData = slot.GetItemStack().Item.ItemData as ThrowableItemData;
            if (!throwableItemData)
                continue;

            if (!heldThrowables.Contains(throwableItemData))
                heldThrowables.Add(throwableItemData);
        }
        return heldThrowables;
    }

    #endregion
}
