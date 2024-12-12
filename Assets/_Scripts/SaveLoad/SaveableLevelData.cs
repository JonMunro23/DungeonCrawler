using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LevelSaveData
{
    public int currentLevelIndex;
    public string currentLevelName;
    public List<SaveableLevelData> levels;
}

[System.Serializable]
public class LevelData
{
    public Dictionary<Vector2, GridNode> levelNodes = new Dictionary<Vector2, GridNode>();
    public List<NPCController> spawnedNPCs = new List<NPCController>();

    public LevelData(Dictionary<Vector2, GridNode> levelNodes, List<NPCController> spawnedNPCs)
    {
        this.levelNodes = new Dictionary<Vector2, GridNode>(levelNodes);
        this.spawnedNPCs = new List<NPCController>(spawnedNPCs);
    }

    public void UpdateLevelData(Dictionary<Vector2, GridNode> updatedNodes, List<NPCController> updatedNPCs)
    {
        levelNodes.Clear();
        levelNodes = new Dictionary<Vector2, GridNode>(updatedNodes);

        spawnedNPCs.Clear();
        spawnedNPCs = new List<NPCController>(updatedNPCs);
    }

    public Dictionary<Vector2, GridNode> GetNodes()
    {
        return levelNodes;
    }

    public List<NPCController> GetNPCs()
    {
        return spawnedNPCs;
    }
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
        public Vector2 coords;
        public float rotation;
        public ItemStack itemStack;

        public WorldItemSaveData(Vector2 coords, float rotation, ItemStack itemStack)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.itemStack = itemStack;
        }
    }

    [System.Serializable]
    public class ContainerSaveData
    {
        public Vector2 coords;
        public float rotation;
        public List<ContainerItemStack> containedItemStacks;

        public ContainerSaveData(Vector2 coords, float rotation, List<ContainerItemStack> containedItemStacks)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.containedItemStacks = new List<ContainerItemStack>(containedItemStacks);
        }
    }

    [System.Serializable]
    public class NPCSaveData
    {
        public Vector2 coords;
        public float rotation;
        public int currentHealth;
        public NPCData npcData;

        public NPCSaveData(Vector2 coords, float rotation, int currentHealth, NPCData npcData)
        {
            this.coords = coords;
            this.rotation = rotation;
            this.currentHealth = currentHealth;
            this.npcData = npcData;
        }
    }

    public int levelIndex;
    public List<InteractableSaveData> interactableSaveData = new List<InteractableSaveData>();
    public List<TriggerableSaveData> triggerableSaveData = new List<TriggerableSaveData>();
    public List<WorldItemSaveData> worldItems = new List<WorldItemSaveData>();
    public List<ContainerSaveData> containers = new List<ContainerSaveData>();
    public List<NPCSaveData> NPCs = new List<NPCSaveData>();

    public SaveableLevelData(int levelIndex, List<IInteractable> spawnedInteractables, List<ITriggerable> spawnedTriggerables, List<WorldItem> spawnedWorldItems, List<IContainer> spawnedContainers, List<NPCController> spawnedNPCs)
    {
        this.levelIndex = levelIndex;
        interactableSaveData = new List<InteractableSaveData>(GetInteractableSaveData(spawnedInteractables));
        triggerableSaveData = new List<TriggerableSaveData>(GetTriggerableSaveData(spawnedTriggerables));
        worldItems = new List<WorldItemSaveData>(GetWorldItemSaveData(spawnedWorldItems));
        containers = new List<ContainerSaveData>(GetContainerSaveData(spawnedContainers));
        NPCs = new List<NPCSaveData>(GetNPCSaveData(spawnedNPCs));
    }

    List<NPCSaveData> GetNPCSaveData(List<NPCController> spawnedNPCs)
    {
        List<NPCSaveData> NPCSaveData = new List<NPCSaveData>();
        foreach (NPCController npc in spawnedNPCs)
        {
            if (npc.levelIndex == levelIndex)
                NPCSaveData.Add(new NPCSaveData(npc.currentlyOccupiedGridnode.Coords.Pos, npc.transform.localRotation.eulerAngles.y, Mathf.RoundToInt(npc.currentGroupHealth), npc.NPCData));
        }

        return NPCSaveData;
    }
    List<ContainerSaveData> GetContainerSaveData(List<IContainer> spawnedContainers)
    {
        List<ContainerSaveData> containerSaveData = new List<ContainerSaveData>();
        foreach (IContainer container in spawnedContainers)
        {
            if (container.GetLevelIndex() == levelIndex)
                containerSaveData.Add(new ContainerSaveData(container.GetCoords(), container.GetRotation(), container.GetStoredItems()));
        }

        return containerSaveData;
    }
    List<WorldItemSaveData> GetWorldItemSaveData(List<WorldItem> spawnedWorldItems)
    {
        List<WorldItemSaveData> worldItemSaveData = new List<WorldItemSaveData>();
        foreach (WorldItem worldItem in spawnedWorldItems)
        {
            if (worldItem.levelIndex == levelIndex)
                worldItemSaveData.Add(new WorldItemSaveData(worldItem.coords, worldItem.transform.rotation.eulerAngles.y, worldItem.item));
        }

        return worldItemSaveData;
    }
    List<TriggerableSaveData> GetTriggerableSaveData(List<ITriggerable> spawnedTriggerables)
    {
        List<TriggerableSaveData> triggerableSaveData = new List<TriggerableSaveData>();
        foreach (ITriggerable triggerable in spawnedTriggerables)
        {
            if (triggerable.GetLevelIndex() == levelIndex)
                triggerableSaveData.Add(new TriggerableSaveData(triggerable.GetCoords(), triggerable.GetIsTriggered(), triggerable.GetCurrentNumberOfTriggers()));
        }

        return triggerableSaveData;
    }
    List<InteractableSaveData> GetInteractableSaveData(List<IInteractable> spawnedInteractables)
    {
        List<InteractableSaveData> interactableSaveData = new List<InteractableSaveData>();
        foreach (IInteractable interactable in spawnedInteractables)
        {
            if (interactable.GetLevelIndex() == levelIndex)
                interactableSaveData.Add(new InteractableSaveData(interactable.GetCoords(), interactable.GetIsActivated()));
        }

        return interactableSaveData;
    }
}
