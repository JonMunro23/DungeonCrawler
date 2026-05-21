using LDtkUnity;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    [SerializeField] int levelIndex;
    LayerInstance intGridLayer;
    LayerInstance entityLayer;
    Dictionary<Vector2Int, GridNode> levelNodes = new Dictionary<Vector2Int, GridNode>();
    [SerializeField] int totalNumSecrets;

    [SerializeField] List<NPCController> npcs = new List<NPCController>();
    [SerializeField] List<WorldItem> worldItems = new List<WorldItem>();
    [SerializeField] List<IContainer> containers = new List<IContainer>();
    [SerializeField] List<ITriggerable> triggerables = new List<ITriggerable>();
    [SerializeField] List<IInteractable> interactables = new List<IInteractable>();

    public int LevelIndex => levelIndex;
    public LayerInstance IntGridLayer => intGridLayer;
    public LayerInstance EntityLayer => entityLayer;
    public List<NPCController> Npcs => npcs;
    public List<WorldItem> WorldItems => worldItems;
    public List<IContainer> Containers => containers;
    public List<ITriggerable> Triggerables => triggerables;
    public List<IInteractable> Interactables => interactables;

    public LevelData(int levelIndex, LayerInstance intGridLayer, LayerInstance entityLayer)
    {
        this.levelIndex = levelIndex;
        this.intGridLayer = intGridLayer;
        this.entityLayer = entityLayer;
    }

    public void SetLevelActive(bool isActive)
    {
        foreach (GridNode node in GetNodes())
        {
            node.SetActive(isActive);
        }
    }

    public void AssignNodesToLevel(Dictionary<Vector2Int, GridNode> newNodes) => levelNodes = newNodes;
    public List<GridNode> GetNodes()
    {
        return new List<GridNode>(levelNodes.Values);
    }
    public GridNode GetNodeAtCoords(Vector2Int coords)
    {
        if (levelNodes.TryGetValue(coords, out GridNode node))
            return node;

        return null;
    }
    public GridNode GetNodeByIndex(int index)
    {
        List<GridNode> nodes = GetNodes();
        return nodes[index];
    }
    public void CacheNodeNeighbours()
    {
        foreach (GridNode node in GetNodes())
        {
            node.CacheNeighbours();
        }
    }

    public void AddTriggerable(ITriggerable triggerable) => triggerables.Add(triggerable);

    public void AddInteractable(IInteractable interactable) => interactables.Add(interactable);
    public void LinkInteractablesToTriggerables()
    {
        foreach (IInteractable interactable in interactables)
        {
            foreach (string entityRef in interactable.GetEntityRefsToTrigger())
            {
                foreach (ITriggerable triggerable in triggerables)
                {
                    if (triggerable.GetEntityRef() == entityRef)
                    {
                        interactable.AddObjectToTrigger(triggerable);
                    }
                }
            }
        }
    }


    public ref List<NPCController> GetNPCsListRef() => ref npcs;
    public void AddNPC(NPCController npc) => npcs.Add(npc);
    public void RemoveNPC(NPCController npcToRemove)
    {
        if(npcs.Contains(npcToRemove))
            npcs.Remove(npcToRemove);
    }

    public void AddWorldItem(WorldItem item) => worldItems.Add(item);
    public void RemoveWorldItem(WorldItem item)
    {
        if(worldItems.Contains(item))
            worldItems.Remove(item);
    }

    public void AddContainer(IContainer container) => containers.Add(container);
    public void RemoveContainer(IContainer containerToRemove)
    {
        if(containers.Contains(containerToRemove))
            containers.Remove(containerToRemove);
    }

    public void AddSecret() => totalNumSecrets++;
    public int GetTotalNumberOfSecrets() => totalNumSecrets;
    public void ClearRuntimeNodeOccupants()
    {
        foreach (GridNode node in GetNodes())
        {
            node.ResetOccupant();
        }
    }
    public void DestroyEntities()
    {
        foreach (NPCController npc in npcs)
        {
            if (npc != null)
                Object.Destroy(npc.gameObject);
        }

        foreach (WorldItem worldItem in worldItems)
        {
            if (worldItem != null)
                Object.Destroy(worldItem.gameObject);
        }

        foreach (IContainer container in containers)
        {
            if (container != null)
                container.Destroy();
        }

        foreach (IInteractable interactable in interactables)
        {
            if (interactable != null)
                interactable.Destroy();
        }

        foreach (ITriggerable triggerable in triggerables)
        {
            if (triggerable != null)
                triggerable.Destroy();
        }

        npcs.Clear();
        worldItems.Clear();
        containers.Clear();
        interactables.Clear();
        triggerables.Clear();
    }

}