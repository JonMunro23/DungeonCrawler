using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour, IInventory
{
    public PlayerController playerController;
    [SerializeField]
    List<ItemData> startingItems = new List<ItemData>(); 

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

        InventoryContextMenu.onInventorySlotWeaponUnloaded += OnInventorySlotWeaponUnloaded;

        PauseMenu.onQuit += RemoveInventorySlots;
    }

    void OnDisable()
    {
        Container.onContainerOpened -= OnContainerOpened;
        Container.onContainerClosed -= OnContainerClosed;

        InventoryContextMenu.onInventorySlotWeaponUnloaded -= OnInventorySlotWeaponUnloaded;

        PauseMenu.onQuit -= RemoveInventorySlots;
    }

    void OnContainerOpened()
    {
        returnCamPos = Camera.main.transform.localPosition;
        returnCamRot = Camera.main.transform.localEulerAngles;

        playerController.MoveCameraPos(openContainerCamPos, openContainerCamMovementDuration);
        playerController.RotCamera(openContainerCamRot, openContainerCamMovementDuration);
        isInContainer = true;

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
        for (int i = 0; i < startingItems.Count; i++)
        {
            ItemData itemData = startingItems[i];
            if (itemData == null) return;

            //WeaponItemData weaponItemData = itemData as WeaponItemData;
            if(itemData is WeaponItemData weaponItemData)
                spawnedInventorySlots[i].AddItem(new ItemStack(new WeaponItem(weaponItemData, weaponItemData.defaultLoadedAmmoData, weaponItemData.magSize), itemData.maxItemStackSize));
            else
                spawnedInventorySlots[i].AddItem(new ItemStack(new Item(itemData), itemData.maxItemStackSize));
        }
    }

    //public void ToggleInventory()
    //{

    //    if (CharacterMenuUIController.isCharacterMenuOpen == true)
    //    {
    //        CloseInventory();
    //    }
    //    else if (CharacterMenuUIController.isCharacterMenuOpen == false)
    //    {
    //        OpenInventory();
    //    }
    //}

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
            if (slot.IsSlotEmpty())
                continue;

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

            if (slot.GetItemStack().Item != null)
            {
                if (slot.GetItemStack().Item.ItemData == itemData)
                {
                    if (slot.GetItemStack().GetRemainingSpaceInStack() > 0)
                    {
                        slotsWithItemAndSpace.Add(slot);
                    }
                }
            }
        }

        if(slotsWithItemAndSpace.Count > 0)
            return slotsWithItemAndSpace.ToArray();

        return null;
    }

    public int TryAddItem(ItemStack itemToAdd)
    {
        int remainingAmountToAdd = itemToAdd.ItemAmount;

        if(itemToAdd.Item.ItemData.maxItemStackSize > 1)
        {
            InventorySlot[] slotsWithSpace = GetSlotWithItemWithSpace(itemToAdd.Item.ItemData);
            if (slotsWithSpace != null && slotsWithSpace.Length > 0)
            {
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
            }
        }

        if (remainingAmountToAdd > 0)
        {
            InventorySlot freeSlot = GetNextFreeSlot();
            if (freeSlot)
            {
                freeSlot.AddItem(new ItemStack(itemToAdd.Item, remainingAmountToAdd));
                remainingAmountToAdd = 0;
            }
        }

        return remainingAmountToAdd;
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
            //Debug.Log("Added to existing stack");
            if (remainingAmountToAdd == 0)
                return;
        }

        if(remainingAmountToAdd > 0)
        {
            InventorySlot freeSlot = GetNextFreeSlot();
            if (freeSlot)
            {
                freeSlot.AddItem(new ItemStack(new Item(ammoTypeToAdd), remainingAmountToAdd));
                //Debug.Log($"Added item with amount: {remainingAmountToAdd}");
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

    public void LoadItems(List<ItemStackSaveData> itemsToLoad)
    {
        foreach (var slot in spawnedInventorySlots)
        {
            if (!slot.IsSlotEmpty())
                slot.RemoveItem();
        }

        ItemDatabase itemDatabase = GridController.Instance.itemDatabase;
        foreach (ItemStackSaveData savedItem in itemsToLoad)
        {
            ItemData itemData = itemDatabase.GetItemDataFromIdentifier(savedItem.itemID);

            if (itemData == null)
                continue;

            Item item;
            if (itemData is WeaponItemData weaponItemData)
            {
                AmmoItemData ammoItemData = null;

                if (!string.IsNullOrEmpty(savedItem.loadedAmmoType))
                    ammoItemData = itemDatabase.GetItemDataFromIdentifier(savedItem.loadedAmmoType) as AmmoItemData;

                item = new WeaponItem(weaponItemData, ammoItemData, savedItem.loadedAmmo);
            }
            else
            {
                item = new Item(itemData);
            }

            spawnedInventorySlots[savedItem.slotIndex].AddItem(new ItemStack(item, savedItem.amount));
        }
    }

    public void Save(ref PlayerSaveData data)
    {
        var storedItems = GetStoredItems();
        List<ItemStackSaveData> stackSaveData = new List<ItemStackSaveData>();

        foreach (ItemStack item in storedItems)
        {
            ItemStackSaveData saveData = new ItemStackSaveData
            {
                itemID = item.Item.ItemData.itemIdentifier,
                amount = item.ItemAmount,
                slotIndex = item.SlotIndex
            };

            if (item.Item is WeaponItem weaponItem)
            {
                saveData.isWeapon = true;
                saveData.loadedAmmoType = weaponItem.LoadedAmmoData != null
                    ? weaponItem.LoadedAmmoData.itemIdentifier
                    : "";
                saveData.loadedAmmo = weaponItem.LoadedAmmo;
            }

            stackSaveData.Add(saveData);
        }

        data.storedItems = stackSaveData;
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
