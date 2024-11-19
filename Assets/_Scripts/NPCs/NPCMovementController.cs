using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovementController : MonoBehaviour
{
    NPCController groupController;
    public float gridSize = 3f;

    PlayerController playerController;

    [Header("Movement")]
    public float moveDuration;
    public bool isMoving;
    [SerializeField] float minDelayBetweenMovement;
    [SerializeField] GridNode currentOccupiedGridNode;
    [SerializeField] List<GridNode> path = new List<GridNode>();
    [SerializeField] AudioClip[] walkSFx;

    [Space]
    [Header("Turning")]
    public float turnDuration;
    //0 = North, 1 = East, 2 = South, 3 = West
    public Transform currentOrientation; 
    public bool isTurning;
    [SerializeField] float minDelayBetweenTurning;

    [SerializeField] GridNode playerGridNode;

    private void OnEnable()
    {
        AdvancedGridMovement.onPlayerMoved += OnPlayerMoved;
        NPCController.onNPCDeath += OnNPCDeath;
    }

    private void OnDisable()
    {
        AdvancedGridMovement.onPlayerMoved -= OnPlayerMoved;
        NPCController.onNPCDeath -= OnNPCDeath;
    }

    void OnNPCDeath()
    {
        FindNewPathToPlayer();
    }

    void OnPlayerMoved()
    {
        FindNewPathToPlayer();
        playerGridNode = PlayerController.currentOccupiedNode;
    }

    public void FindNewPathToPlayer()
    {
        if(path != null)
            foreach (GridNode node in path)
            {
                node.RevertTile();
            }

        path = Pathfinding_Custom.FindPath(groupController.currentlyOccupiedGridnode, PlayerController.currentOccupiedNode);
        NavigateToPlayer();
    }

    public void NavigateToPlayer()
    {
        if(path == null)
        {
            Debug.Log("NAE PATH");
            return;
        }

        if (isMoving || isTurning || groupController.attackController.isAttacking)
            return;

        GridNode nodeToMoveTo = path[path.Count - 1];
        Vector3 dirToTarget = Vector3.Normalize(currentOrientation.position - nodeToMoveTo.moveToTransform.position);
        float leftOrRightDot = Vector3.Dot(currentOrientation.right, dirToTarget);
        float frontOrBackDot = Vector3.Dot(currentOrientation.forward, dirToTarget);

        //Debug.Log("Left/Right: " + Mathf.RoundToInt(leftOrRight));
        //Debug.Log("Front/Back: " + Mathf.RoundToInt(dot));

        if (nodeToMoveTo.currentOccupant.occupantType == GridNodeOccupantType.None && Mathf.RoundToInt(frontOrBackDot) == -1)
        {
            MoveToGridNode(nodeToMoveTo);
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
        groupController = _groupController;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            FindNewPathToPlayer();
        }

        //if(Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    Turn(-1);
        //}
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    Turn(1);
        //}
    }

    AudioClip GetRandomAudioClip()
    {
        int rand = Random.Range(0, walkSFx.Length);
        return walkSFx[rand];
    }

    void MoveToGridNode(GridNode targetNode)
    {
        if (isMoving)
            return;

        groupController.currentlyOccupiedGridnode.ClearOccupant();
        AnimateMovement();
        groupController.audioSource.PlayOneShot(GetRandomAudioClip());
        StartCoroutine(LerpPos(transform.position, targetNode.moveToTransform.position, moveDuration));
        StartCoroutine(DelayBetweenMovement());

        groupController.currentlyOccupiedGridnode = targetNode;
        groupController.currentlyOccupiedGridnode.SetOccupant(new GridNodeOccupant(groupController.gameObject, GridNodeOccupantType.Enemy));
    }

    /// <summary>
    /// 
    /// </summary>
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
        groupController.animController.PlayAnimation("Walk");
    }

    void AnimateTurning(int turnDir)
    {
        isTurning = true;
        if(turnDir  == -1)
        {
            groupController.animController.PlayAnimation("TurnLeft", turnDuration);
            
        }
        else if(turnDir == 1)
            groupController.animController.PlayAnimation("TurnRight", turnDuration);

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

    IEnumerator LerpRot(Quaternion startRot, Quaternion endRot, float lerpDuration)
    {
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            float t = timeElapsed / lerpDuration;
            transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = endRot;
    }

    IEnumerator DelayBetweenMovement()
    {
        yield return new WaitForSeconds(moveDuration);
        groupController.animController.PlayAnimation("Idle");
        yield return new WaitForSeconds(minDelayBetweenMovement);
        MovementEnded();
    }

    void MovementEnded()
    {
        groupController.TryAttack();
        FindNewPathToPlayer();
    }


    IEnumerator DelayBetweenTurning()
    {
        yield return new WaitForSeconds(turnDuration + minDelayBetweenTurning);
        isTurning = false;
        TurningEnded();
    }

    void TurningEnded()
    {
        groupController.TryAttack();
        NavigateToPlayer();
    }
}
