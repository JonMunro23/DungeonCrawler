using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class NPCMovementController : MonoBehaviour
{
    NPCGroupController groupController;
    public float gridSize = 3f;

    [Header("Movement")]
    public float moveDuration;
    [SerializeField] bool isMoving;
    [SerializeField] float minDelayBetweenMovement;

    [Space]
    [Header("Turning")]
    public float turnDuration;
    //0 = North, 1 = East, 2 = South, 3 = West
    [SerializeField] Vector3 currentLookDir = Vector3.zero; 
    [SerializeField] bool isTurning;
    [SerializeField] float mindDelayBetweenTurning;

    public void Init(NPCGroupController _groupController)
    {
        groupController = _groupController;
        currentLookDir = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(transform.forward);
        }
        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //{
        //    Move(-transform.forward);
        //}
        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        //{
        //    Move(-transform.right);
        //}
        //if (Input.GetKeyDown(KeyCode.RightArrow))
        //{
        //    Move(transform.right);
        //}
        if (Input.GetKeyDown(KeyCode.P))
        {
            //turn right
            Turn(1);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            //turn left
            Turn(-1);
        }
    }

    void Move(Vector3 moveDir)
    {
        if (isMoving)
            return;

        isMoving = true;
        StartCoroutine(LerpPos(transform.localPosition, transform.localPosition + moveDir * gridSize, moveDuration));
        StartCoroutine(DelayBetweenMovement());
        AnimateMovement(moveDir);
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

    void AnimateMovement(Vector3 moveDir)
    {
        groupController.animController.PlayAnimation("Walk");
    }

    void AnimateTurning(int turnDir)
    {
        if(turnDir  == -1)
        {
            groupController.animController.PlayAnimation("TurnLeft");
            
        }
        else if(turnDir == 1)
            groupController.animController.PlayAnimation("TurnRight");

        UpdateLookDir(turnDir);
    }

    void UpdateLookDir(int turnDir)
    {
        currentLookDir = Vector3.up * 90 * turnDir;
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
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = endPos;
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
    }

    IEnumerator DelayBetweenTurning()
    {
        yield return new WaitForSeconds(turnDuration + minDelayBetweenMovement);
        isTurning = false;
    }
}
