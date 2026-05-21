using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LevelSaveData
{
    public int currentLevelIndex;
    public string currentLevelName;
    public Vector2Int playerCoords;
    public List<SaveableLevelData> levels;
}

[System.Serializable]
public class SaveableLevelData
{
    [System.Serializable]
    public class TriggerableSaveData
    {
        public Vector2 coords;
        public bool isTriggered;
        public int currentNumberOfTriggers;

        public TriggerableSaveData(Vector2 coords, bool isTriggered, int currentNumberOfTriggers)
        {
            this.coords = coords;
            this.isTriggered = isTriggered;
            this.currentNumberOfTriggers = currentNumberOfTriggers;
        }
    }

    [System.Serializable]
    public class InteractableSaveData
    {
        public Vector2 coords;
        public bool isActivated;

        public InteractableSaveData(Vector2 coords, bool isActivated)
        {
            this.coords = coords;
            this.isActivated = isActivated;
        }
    }

    [System.Serializable]
    public class WorldItemSaveData
    {
        public Vector2Int coords;
        public Vector3 position;
        public Vector3 rotation;
        public ItemStackSaveData itemStackSaveData;

        public WorldItemSaveData(Vector2Int coords, Vector3 position, Vector3 rotation, ItemStackSaveData itemStackSaveData)
        {
            this.coords = coords;
            this.position = position;
            this.rotation = rotation;
            this.itemStackSaveData = itemStackSaveData;
        }
    }

    [System.Serializable]
    public class ContainerSaveData
    {
        public Vector2 coords;
        public float rotation;
        public List<ItemStackSaveData> containedItemStackSaveDatas;

        public ContainerSaveData(Vector2 coords, float rotation, List<ItemStackSaveData> containedItemStacks)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.containedItemStackSaveDatas = new List<ItemStackSaveData>(containedItemStacks);
        }
    }

    [System.Serializable]
    public class NPCSaveData
    {
        public Vector2Int coords;
        public float rotation;
        public int currentHealth;
        public NPCData npcData;
        public NPCMovementBehaviour movementBehaviour;

        public NPCSaveData(Vector2Int coords, float rotation, int currentHealth, NPCData npcData, NPCMovementBehaviour movementBehaviour)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.currentHealth = currentHealth;
            this.npcData = npcData;
            this.movementBehaviour = movementBehaviour;
        }
    }

    public int levelIndex;
    public List<InteractableSaveData> interactableSaveData = new List<InteractableSaveData>();
    public List<TriggerableSaveData> triggerableSaveData = new List<TriggerableSaveData>();
    public List<WorldItemSaveData> worldItemSaveData = new List<WorldItemSaveData>();
    public List<ContainerSaveData> containerSaveData = new List<ContainerSaveData>();
    public List<NPCSaveData> npcSaveData = new List<NPCSaveData>();

    public SaveableLevelData(LevelData levelData)
    {
        levelIndex = levelData.LevelIndex;

        interactableSaveData = GetInteractableSaveData(levelData.Interactables);
        triggerableSaveData = GetTriggerableSaveData(levelData.Triggerables);
        worldItemSaveData = GetWorldItemSaveData(levelData.WorldItems);
        containerSaveData = GetContainerSaveData(levelData.Containers);
        npcSaveData = GetNPCSaveData(levelData.Npcs);

    }

    List<NPCSaveData> GetNPCSaveData(List<NPCController> spawnedNPCs)
    {
        List<NPCSaveData> NPCSaveData = new List<NPCSaveData>();
        
        foreach (NPCController npc in spawnedNPCs)
        {
            NPCSaveData.Add(new NPCSaveData(npc.currentlyOccupiedGridnode.Coords.Pos, npc.transform.localRotation.eulerAngles.y, Mathf.RoundToInt(npc.healthController.CurrentHealth), npc.npcData, npc.GetMovementBehaviour()));
        }

        return NPCSaveData;
    }
    List<ContainerSaveData> GetContainerSaveData(List<IContainer> spawnedContainers)
    {
        List<ContainerSaveData> containerSaveData = new List<ContainerSaveData>();
        foreach (IContainer container in spawnedContainers)
        {
            containerSaveData.Add(new ContainerSaveData(container.GetCoords(), 
                container.GetRotation(), 
                container.GetStoredItemsSaveData()));
        }

        return containerSaveData;
    }
    List<WorldItemSaveData> GetWorldItemSaveData(List<WorldItem> spawnedWorldItems)
    {
        List<WorldItemSaveData> worldItemSaveData = new List<WorldItemSaveData>();

        foreach (WorldItem worldItem in spawnedWorldItems)
        {
            ItemStackSaveData saveData = new ItemStackSaveData
            {
                itemID = worldItem.itemStack.Item.ItemData.itemIdentifier,
                amount = worldItem.itemStack.ItemAmount
            };

            if (worldItem.itemStack.Item is WeaponItem weaponItem)
            {
                saveData.isWeapon = true;
                saveData.loadedAmmoType = weaponItem.LoadedAmmoData != null
                    ? weaponItem.LoadedAmmoData.itemIdentifier
                    : "";
                saveData.loadedAmmo = weaponItem.LoadedAmmo;
            }

            worldItemSaveData.Add(new WorldItemSaveData(
                worldItem.GetCoords(),
                worldItem.transform.position,
                worldItem.transform.eulerAngles,
                saveData
            ));
        }

        return worldItemSaveData;
    }
    List<TriggerableSaveData> GetTriggerableSaveData(List<ITriggerable> spawnedTriggerables)
    {
        List<TriggerableSaveData> triggerableSaveData = new List<TriggerableSaveData>();
        foreach (ITriggerable triggerable in spawnedTriggerables)
        {
            triggerableSaveData.Add(new TriggerableSaveData(triggerable.GetCoords(), triggerable.GetIsTriggered(), triggerable.GetCurrentNumberOfTriggers()));
        }

        return triggerableSaveData;
    }
    List<InteractableSaveData> GetInteractableSaveData(List<IInteractable> spawnedInteractables)
    {
        List<InteractableSaveData> interactableSaveData = new List<InteractableSaveData>();
        foreach (IInteractable interactable in spawnedInteractables)
        {
            interactableSaveData.Add(new InteractableSaveData(interactable.GetCoords(), interactable.GetIsActivated()));
        }

        return interactableSaveData;
    }

    public InteractableSaveData FindSavedInteractableData(Vector2Int coords)
    {
        foreach (InteractableSaveData interactableData in interactableSaveData)
        {
            if (interactableData.coords == coords)
                return interactableData;
        }

        return null;
    }

    public ContainerSaveData FindSavedContainerData(Vector2Int coords)
    {
        foreach (ContainerSaveData containerData in containerSaveData)
        {
            if (containerData.coords == coords)
                return containerData;
        }

        return null;
    }

    public TriggerableSaveData FindSavedTriggerableData(Vector2Int coords)
    {
        foreach (TriggerableSaveData triggerableData in triggerableSaveData)
        {
            if (triggerableData.coords == coords)
                return triggerableData;
        }

        return null;
    }
}

[System.Serializable]
public class ItemStackSaveData
{
    public string itemID;
    public int amount;
    public int slotIndex; //Used to determine slot in inventory/container
    public bool isWeapon;
    public string loadedAmmoType;
    public int loadedAmmo;
}

[System.Serializable]
public struct PlayerSaveData
{
    //Movement Data
    public Vector2Int coords;
    public Vector3 rotation;

    //Health data
    public int currentHealth;

    //Inventory Data
    public List<ItemStackSaveData> storedItems;

    //Equipment Data
    public List<EquippedItem> equippedItems;

    //Weapon Data
    public int activeWeaponSlotIndex;
    public List<ItemStackSaveData> weaponItems;

    //Skill Data
    public int availableSkillPoints;
    public List<UnlockedSKillData> unlockedSkills;

    //Level Data
    public int currentPlayerLevel;
    public int currentExperiencePoints;
    public int requiredExperiencePoints;

    //Throwable Data
    public ThrowableItemData selectedThrowable;
}
