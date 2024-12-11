using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovementController : MonoBehaviour
{
    NPCController NPCController;
    public const int GRID_SIZE = 3;

    [Header("Movement")]
    public bool isMoving;
    [SerializeField] List<GridNode> path = new List<GridNode>();
    public GridNode targetNode;

    [Space]
    [Header("Turning")]
    public Transform currentOrientation; 
    public bool isTurning;

    [SerializeField] GridNode playerGridNode;

    private void OnEnable()
    {
        AdvancedGridMovement.onPlayerMoved += OnPlayerMoved;
        NPCController.onNPCDeath += OnNPCDeath;
        GridNode.onNodeOccupancyUpdated += OnNodeOccupancyUpdated;
    }

    private void OnDisable()
    {
        AdvancedGridMovement.onPlayerMoved -= OnPlayerMoved;
        NPCController.onNPCDeath -= OnNPCDeath;
        GridNode.onNodeOccupancyUpdated -= OnNodeOccupancyUpdated;
    }

    void OnNPCDeath(NPCController deadNPC)
    {
        if (deadNPC == NPCController)
            return;

        FindNewPathToPlayer();
    }

    void OnPlayerMoved()
    {
        //FindNewPathToPlayer();
        playerGridNode = PlayerController.currentOccupiedNode;
    }

    void OnNodeOccupancyUpdated()
    {
        FindNewPathToPlayer();
    }

    public void FindNewPathToPlayer()
    {
        if(path != null)
            foreach (GridNode node in path)
            {
                node.RevertTile();
            }

        //Debug.Log("NPC coords: " + groupController.currentlyOccupiedGridnode.Coords.Pos);
        //Debug.Log("Player coords: " + (PlayerController.currentOccupiedNode ? PlayerController.currentOccupiedNode.Coords.Pos : "No Player Exists"));
        path = Pathfinding_Custom.FindPath(NPCController.currentlyOccupiedGridnode, PlayerController.currentOccupiedNode);
        NavigateToPlayer();
    }

    public void NavigateToPlayer()
    {
        if(path == null)
        {
            //Roam?
            //Debug.Log("NAE PATH");
            return;
        }

        if (isMoving || isTurning || NPCController.attackController.isAttacking)
            return;

        targetNode = path[path.Count - 1];
        Vector3 dirToTarget = Vector3.Normalize(currentOrientation.position - targetNode.moveToTransform.position);
        float leftOrRightDot = Vector3.Dot(currentOrientation.right, dirToTarget);
        float frontOrBackDot = Vector3.Dot(currentOrientation.forward, dirToTarget);

        //Debug.Log("Left/Right: " + Mathf.RoundToInt(leftOrRight));
        //Debug.Log("Front/Back: " + Mathf.RoundToInt(dot));

        if (targetNode.currentOccupant.occupantType == GridNodeOccupantType.None && Mathf.RoundToInt(frontOrBackDot) == -1)
        {
            MoveToTargetNode();
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

    public void Init(NPCController _groupController)
    {
        NPCController = _groupController;
    }

    AudioClip GetRandomAudioClip()
    {
        int rand = Random.Range(0, NPCController.NPCData.walkSFX.Length);
        return NPCController.NPCData.walkSFX[rand];
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

        NPCController.currentlyOccupiedGridnode.ClearOccupant();
        AnimateMovement();
        if(NPCController.NPCData.walkSFX.Length > 0)
            NPCController.audioSource.PlayOneShot(GetRandomAudioClip());

        StartCoroutine(LerpPos(transform.position, targetNode.moveToTransform.position, NPCController.NPCData.moveDuration));
        StartCoroutine(DelayBetweenMovement());

        NPCController.currentlyOccupiedGridnode = targetNode;
        NPCController.currentlyOccupiedGridnode.SetOccupant(new GridNodeOccupant(NPCController.gameObject, GridNodeOccupantType.NPC));
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
        NPCController.animController.PlayAnimation("Walk");
    }

    void AnimateTurning(int turnDir)
    {
        isTurning = true;
        if(turnDir  == -1)
        {
            NPCController.animController.PlayAnimation("TurnLeft", NPCController.NPCData.turnDuration);
            
        }
        else if(turnDir == 1)
            NPCController.animController.PlayAnimation("TurnRight", NPCController.NPCData.turnDuration);

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
        yield return new WaitForSeconds(NPCController.NPCData.moveDuration);
        NPCController.animController.PlayAnimation("Idle");
        yield return new WaitForSeconds(NPCController.NPCData.minDelayBetweenMovement);
        MovementEnded();
    }

    void MovementEnded()
    {
        NPCController.TryAttack();
        FindNewPathToPlayer();
    }


    IEnumerator DelayBetweenTurning()
    {
        yield return new WaitForSeconds(NPCController.NPCData.turnDuration + NPCController.NPCData.minDelayBetweenTurning);
        isTurning = false;
        TurningEnded();
    }

    void TurningEnded()
    {
        NPCController.TryAttack();
        NavigateToPlayer();
    }
}
