using UnityEngine;
using System;

[SelectionBase]
public class NPCController : MonoBehaviour
{
    public int levelIndex;

    [HideInInspector] public NPCHealthController healthController;
    [HideInInspector] public NPCAnimationController animController;
    [HideInInspector] public NPCMovementController movementController;
    [HideInInspector] public NPCAttackController attackController;
    [HideInInspector] public NPCFloatingTextController floatingTextController;

    public NPCData npcData;

    public AudioSource audioSource;
    public GridNode currentlyOccupiedGridnode;


    public static Action<NPCController> onNPCDeath;
    Coroutine fireDamageCoroutine, acidDamageCoroutine, armourReductionCoroutine;

    private void Awake()
    {
        healthController = GetComponent<NPCHealthController>();
        animController = GetComponent<NPCAnimationController>();
        movementController = GetComponent<NPCMovementController>();
        attackController = GetComponent<NPCAttackController>();
        floatingTextController = GetComponent<NPCFloatingTextController>();
        audioSource = GetComponent<AudioSource>();
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
        gameObject.SetActive(isActive);
    }

    public void SnapToNode(GridNode node)
    {
        movementController.SnapToNode(node);
    }

    void SnapToRotation(float newRot)
    {
        movementController.SnapToRotation(newRot);

    }

    public void TryAttack()
    {
        if (attackController.CheckForPlayer())
        {
            attackController.TryAttack();
        }
        else
            movementController.FindNewPathToPlayer();
    }

    public void OnDeath()
    {
        onNPCDeath?.Invoke(this);
        movementController.OnDeath();
        currentlyOccupiedGridnode.ResetOccupant();
        Destroy(gameObject);
    }
}
