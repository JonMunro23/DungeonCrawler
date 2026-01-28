using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCMovementBehaviour
{
    Idle,
    Roam,
    Pursue
}

public class NPCMovementController : MonoBehaviour
{
    NPCController controller;
    public const int GRID_SIZE = 3;

    [Header("Movement")]
    [SerializeField] bool canMove = true;
    public bool isMoving;
    [SerializeField] List<GridNode> pathToTarget = new List<GridNode>();
    public GridNode targetNode, previousTargetNode;
    public NPCMovementBehaviour currentBehaviour;
    [SerializeField] List<GridNode> walkableNodes = new List<GridNode>();


    [Space]
    [Header("Turning")]
    public Transform currentOrientation; 
    public bool isTurning;

    [SerializeField] GridNode playerGridNode;

    private void OnEnable()
    {
        PlayerMovementManager.onPlayerMoveEnded += OnPlayerMoveEnded;
        NPCController.onNPCDeath += OnNPCDeath;
        GridNode.onNodeOccupancyUpdated += OnNodeOccupancyUpdated;
    }

    private void OnDisable()
    {
        PlayerMovementManager.onPlayerMoveEnded -= OnPlayerMoveEnded;
        NPCController.onNPCDeath -= OnNPCDeath;
        GridNode.onNodeOccupancyUpdated -= OnNodeOccupancyUpdated;
    }

    void OnNPCDeath(NPCController deadNPC)
    {
        if (deadNPC == controller)
            return;

        FindNewPathToTarget();
    }
    void OnPlayerMoveEnded()
    {
        //FindNewPathToPlayer();
        playerGridNode = PlayerController.currentOccupiedNode;
    }

    void OnNodeOccupancyUpdated()
    {
        FindNewPathToTarget();
    }

    public void Init(NPCController controller)
    {
        this.controller = controller;
    }

    public void SetSpawnBehaviour(NPCMovementBehaviour spawnBehaviour)
    {
        currentBehaviour = spawnBehaviour;
        switch (spawnBehaviour)
        {
            case NPCMovementBehaviour.Idle:
                break;
            case NPCMovementBehaviour.Roam:
                SetTarget(GetRandomAdjacentWalkableNode());
                FindNewPathToTarget();
                break;
            case NPCMovementBehaviour.Pursue:
                SetTarget(playerGridNode);
                break;
            default:
                break;
        }
    }

    GridNode GetRandomAdjacentWalkableNode(GridNode previousTargetNode = null)
    {
        List<GridNode> neighbouringNodes = new List<GridNode>(controller.currentlyOccupiedGridnode.GetNeighbouringNodes(false));
        walkableNodes.Clear();
        foreach (GridNode node in neighbouringNodes)
        {
            if(node.nodeData.isWalkable)
                if(node.currentOccupant == null 
                    || node.currentOccupant.occupantType == GridNodeOccupantType.PressurePlate 
                    || node.currentOccupant.occupantType == GridNodeOccupantType.None)
                walkableNodes.Add(node);
        }
        if(previousTargetNode != null)
        {
            if(walkableNodes.Contains(previousTargetNode) && walkableNodes.Count > 1)
            {
                walkableNodes.Remove(previousTargetNode);
                Debug.Log("REMOVED PREVIOUS NODE");
            }
        }

        int randomIndex = Random.Range(0, walkableNodes.Count);
        return walkableNodes[randomIndex];
    }

    public void OnDeath()
    {
        RevertNodesOnPath();
    }

    public void SetTarget(GridNode targetNode)
    {
        this.targetNode = targetNode;
    }

    public void FindNewPathToTarget()
    {
        if(!canMove || targetNode == null) return;

        if (pathToTarget != null)
            RevertNodesOnPath();

        //Debug.Log("NPC coords: " + groupController.currentlyOccupiedGridnode.Coords.Pos);
        //Debug.Log("Player coords: " + (PlayerController.currentOccupiedNode ? PlayerController.currentOccupiedNode.Coords.Pos : "No Player Exists"));
        pathToTarget = Pathfinding_Custom.FindPath(controller.currentlyOccupiedGridnode, targetNode);
        NavigateToTarget();
    }

    private void RevertNodesOnPath()
    {
        foreach (GridNode node in pathToTarget)
        {
            node.RevertTile();
        }
    }

    public void NavigateToTarget()
    {
        if(pathToTarget == null)
        {
            //Roam?
            //Debug.Log("NAE PATH");
            return;
        }

        if (isMoving || isTurning || controller.attackController.isAttacking)
            return;

        //if (currentBehaviour == NPCMovementBehaviour.Roam)
        //    SetTarget(GetRandomAdjacentWalkableNode());
        //else
        if (pathToTarget.Count > 0)
            SetTarget(pathToTarget[pathToTarget.Count - 1]);
        else
        {
            SetTarget(GetRandomAdjacentWalkableNode(previousTargetNode));
            FindNewPathToTarget();
        }


            Vector3 dirToTarget = Vector3.Normalize(currentOrientation.position - targetNode.moveToTransform.position);
        float leftOrRightDot = Vector3.Dot(currentOrientation.right, dirToTarget);
        float frontOrBackDot = Vector3.Dot(currentOrientation.forward, dirToTarget);

        //Debug.Log("Left/Right: " + Mathf.RoundToInt(leftOrRight));
        //Debug.Log("Front/Back: " + Mathf.RoundToInt(dot));

        if ((targetNode.currentOccupant.occupantType == GridNodeOccupantType.None || 
            targetNode.currentOccupant.occupantType == GridNodeOccupantType.PressurePlate ) && Mathf.RoundToInt(frontOrBackDot) == -1)
        {
            MoveToTargetNode();
        }
        else if(targetNode.currentOccupant.occupantType == GridNodeOccupantType.Player && Mathf.RoundToInt(frontOrBackDot) == -1)
        {
            controller.TryAttack();
        }
        else
        {
            if (Mathf.RoundToInt(leftOrRightDot) == -1 || Mathf.RoundToInt(leftOrRightDot) == 0 && Mathf.RoundToInt(frontOrBackDot) == 1)
            {
                Turn(1);
            }
            else if(Mathf.RoundToInt(leftOrRightDot) == 1)
            {
                Turn(-1);
            }
        }

    }


    AudioClip GetRandomAudioClip()
    {
        int rand = Random.Range(0, controller.npcData.walkSFX.Length);
        return controller.npcData.walkSFX[rand];
    }
    public void SnapToNode(GridNode node)
    {
        if (!isMoving)
            return;

        transform.position = node.moveToTransform.position;
        isMoving = false;
        //cancel any active coroutines
        //snap rotation
    }

    public void SnapToRotation(float newRot)
    {
        transform.Rotate(new Vector3(0, newRot, 0));
        currentOrientation.Rotate(new Vector3(0, newRot, 0));
    }

    void MoveToTargetNode()
    {
        if (isMoving)
            return;

        controller.currentlyOccupiedGridnode.ResetOccupant();
        AnimateMovement();
        if(controller.npcData.walkSFX.Length > 0)
            controller.audioSource.PlayOneShot(GetRandomAudioClip());

        StartCoroutine(LerpPos(transform.position, targetNode.moveToTransform.position, controller.npcData.moveDuration));
        StartCoroutine(DelayBetweenMovement());
        previousTargetNode = controller.currentlyOccupiedGridnode;
        controller.currentlyOccupiedGridnode = targetNode;
        controller.currentlyOccupiedGridnode.SetOccupant(new GridNodeOccupant(controller.gameObject, GridNodeOccupantType.NPC));
    }

    /// <param name="turnDir"> -1 = left, 1 = right </param>
    void Turn(int turnDir)
    {
        if (isTurning)
            return;

        AnimateTurning(turnDir);
        StartCoroutine(DelayBetweenTurning());
    }

    void AnimateMovement()
    {
        controller.animController.PlayAnimation("Walk");
    }

    void AnimateTurning(int turnDir)
    {
        isTurning = true;
        if(turnDir  == -1)
        {
            controller.animController.PlayAnimation("TurnLeft", controller.npcData.turnDuration);
            
        }
        else if(turnDir == 1)
            controller.animController.PlayAnimation("TurnRight", controller.npcData.turnDuration);

        UpdateLookDir(turnDir);
    }

    void UpdateLookDir(int turnDir)
    {
        currentOrientation.Rotate(new Vector3(0, turnDir * 90, 0));
    }

    IEnumerator LerpPos(Vector3 startPos, Vector3 endPos, float lerpDuration)
    {
        isMoving = true;
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            float t = timeElapsed / lerpDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        isMoving = false;
    }

    IEnumerator DelayBetweenMovement()
    {
        yield return new WaitForSeconds(controller.npcData.moveDuration);
        controller.animController.PlayAnimation("Idle");
        yield return new WaitForSeconds(controller.npcData.minDelayBetweenMovement);
        MovementEnded();
    }

    void MovementEnded()
    {
        controller.TryAttack();
        FindNewPathToTarget();
    }


    IEnumerator DelayBetweenTurning()
    {
        yield return new WaitForSeconds(controller.npcData.turnDuration + controller.npcData.minDelayBetweenTurning);
        isTurning = false;
        TurningEnded();
    }

    void TurningEnded()
    {
        controller.TryAttack();
        NavigateToTarget();
    }
}
