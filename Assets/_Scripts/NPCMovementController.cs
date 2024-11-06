using System.Collections;
using System.Collections.Generic;
using Tarodev_Pathfinding._Scripts;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using UnityEngine;

public class NPCMovementController : MonoBehaviour
{
    NPCGroupController groupController;
    public float gridSize = 3f;

    PlayerController playerController;

    [Header("Movement")]
    public float moveDuration;
    [SerializeField] bool isMoving;
    [SerializeField] float minDelayBetweenMovement;
    [SerializeField] GridNode currentOccupiedGridNode;
    [SerializeField] List<GridNode> path = new List<GridNode>();

    [Space]
    [Header("Turning")]
    public float turnDuration;
    //0 = North, 1 = East, 2 = South, 3 = West
    [SerializeField] Transform currentOrientation; 
    [SerializeField] bool isTurning;
    [SerializeField] float minDelayBetweenTurning;

    [SerializeField] GridNode playerGridNode;

    private void OnEnable()
    {
        AdvancedGridMovement.onPlayerMoved += OnPlayerMoved;
        NPCGroupController.onNPCDeath += OnNPCDeath;
    }

    private void OnDisable()
    {
        AdvancedGridMovement.onPlayerMoved -= OnPlayerMoved;
        NPCGroupController.onNPCDeath -= OnNPCDeath;
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

    void FindNewPathToPlayer()
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

        if (isMoving || isTurning)
            return;

        GridNode nodeToMoveTo = path[path.Count - 1];
        Vector3 dirToTarget = Vector3.Normalize(currentOrientation.position - nodeToMoveTo.moveToTransform.position);
        float leftOrRight = Vector3.Dot(currentOrientation.right, dirToTarget);
        float dot = Vector3.Dot(currentOrientation.forward, dirToTarget);

        //Debug.Log("Left/Right: " + Mathf.RoundToInt(leftOrRight));
        //Debug.Log("Front/Back: " + Mathf.RoundToInt(dot));

        if (nodeToMoveTo.currentOccupant == GridNodeOccupant.None && Mathf.RoundToInt(dot) == -1)
        {
            MoveToGridNode(nodeToMoveTo);
        }
        else
        {
            if (Mathf.RoundToInt(leftOrRight) == -1 || Mathf.RoundToInt(leftOrRight) == 0 && Mathf.RoundToInt(dot) == 1)
            {
                Turn(1);
            }
            else if(Mathf.RoundToInt(leftOrRight) == 1)
            {
                Turn(-1);
            }
        }

    }

    public void Init(NPCGroupController _groupController)
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

    void MoveToGridNode(GridNode targetNode)
    {
        if (isMoving)
            return;

        groupController.currentlyOccupiedGridnode.ClearOccupant();

        isMoving = true;
        StartCoroutine(LerpPos(transform.position, targetNode.moveToTransform.position, moveDuration));
        StartCoroutine(DelayBetweenMovement());
        groupController.currentlyOccupiedGridnode = targetNode;
        groupController.currentlyOccupiedGridnode.SetOccupant(GridNodeOccupant.Enemy);
        AnimateMovement();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="turnDir"> -1 = left, 1 = right </param>
    void Turn(int turnDir)
    {
        if (isTurning)
            return;

        isTurning = true;

        AnimateTurning(turnDir);
        StartCoroutine(DelayBetweenTurning());
    }

    void AnimateMovement()
    {
        groupController.animController.PlayAnimation("Walk");
    }

    void AnimateTurning(int turnDir)
    {
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
        //Debug.Log("Turning orientation");
        currentOrientation.Rotate(new Vector3(0, turnDir * 90, 0));
        //if(currentLookDir > 3)
        //{
        //    currentLookDir = 0;
        //}else if(currentLookDir < 0)
        //{
        //    currentLookDir = 3;
        //}
    }

    IEnumerator LerpPos(Vector3 startPos, Vector3 endPos, float lerpDuration)
    {
        float timeElapsed = 0;

        while (timeElapsed < lerpDuration)
        {
            float t = timeElapsed / lerpDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
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
        yield return new WaitForSeconds(moveDuration + minDelayBetweenMovement);
        isMoving = false;
        FindNewPathToPlayer();
    }

    IEnumerator DelayBetweenTurning()
    {
        yield return new WaitForSeconds(turnDuration + minDelayBetweenTurning);
        isTurning = false;
        NavigateToPlayer();
    }
}
