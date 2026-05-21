using UnityEngine;
using System;

[SelectionBase]
public class NPCController : MonoBehaviour
{
    public int levelIndex;

    public NPCHealthController healthController;
    public NPCAnimationController animController;
    public NPCMovementController movementController;
    public NPCAttackController attackController;
    public NPCFloatingTextController floatingTextController;

    public NPCData npcData;

    public AudioSource audioSource;
    public GridNode currentlyOccupiedGridnode;

    public static event Action<NPCController> onNPCDeath;

    private void OnEnable()
    {
        GridController.OnFinishedGeneratingLevel += OnLevelFinishedGenerating;
    }

    private void OnDisable()
    {
        GridController.OnFinishedGeneratingLevel -= OnLevelFinishedGenerating;
    }

    void OnLevelFinishedGenerating()
    {
        movementController.BeginMovement();
    }

    public void InitNPC(int _levelIndex, /*NPCData npcData, */GridNode spawnGridNode = null)
    {
        levelIndex = _levelIndex;
        InitControllers();

        if(spawnGridNode != null)
        {
            currentlyOccupiedGridnode = spawnGridNode;
            spawnGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.NPC));
        }
    }

    void InitControllers()
    {
        healthController?.Init(this);
        movementController?.Init(this);
        animController?.Init(this);
        attackController?.Init(this);
    }


    public void SetActive(bool isActive)
    {
        if(!isActive)
            SnapToNode(movementController.CurrentNavTargetNode);

        gameObject.SetActive(isActive);
    }

    public void SnapToNode(GridNode node)
    {
        movementController.SnapToNode(node);
    }

    public void SetMovementBehaviour(NPCMovementBehaviour spawnBehaviour)
    {
        movementController.SetSpawnBehaviour(spawnBehaviour);
    }

    public NPCMovementBehaviour GetMovementBehaviour()
    {
        return movementController.currentMovementBehaviour;
    }

    public void TryAttack()
    {
        if (attackController.CheckForPlayer())
        {
            attackController.TryAttack();
        }
        else
            movementController.FindNewPath();
    }

    public void OnDeath()
    {
        onNPCDeath?.Invoke(this);
        //movementController.OnDeath();
        currentlyOccupiedGridnode.ResetOccupant();
        gameObject.SetActive(false);
    }
}
