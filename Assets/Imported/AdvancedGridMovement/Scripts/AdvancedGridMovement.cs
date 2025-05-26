/* Copyright 2021-2022 Lutz Großhennig

Use of this source code is governed by an MIT-style
license that can be found in the LICENSE file or at
https://opensource.org/licenses/MIT.
*/


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/*
 * This script adds animated grid based movement similar to Dungeon Master, Eye of the Beholder & Legend of Grimrock.
 * It overs advanced options like control over the movement and headbob. 
 */

public class AdvancedGridMovement : MonoBehaviour
{
    PlayerController controller;

    private const float RightHand = 90.0f;
    private const float LeftHand = -RightHand;
    private const float approximationThreshold = 0.025f;

    [SerializeField] private float gridSize = 3.0f;
    [SerializeField] private WeaponMotion weaponMotion; // Assign in Inspector

    [Header("Walk speed settings")]
    [SerializeField] private float walkSpeed = 1.0f;
    [SerializeField] private float turnSpeed = 5.0f;

    [Header("Rotation Settings")]
    [SerializeField] bool canRotate = true;
    [SerializeField] float rotationDelay = .5f;

    [Header("Walking animation curve")]
    [SerializeField] private AnimationCurve walkSpeedCurve;

    [Header("Walking head bob curve")]
    [SerializeField] private AnimationCurve walkHeadBobCurve;
    [SerializeField] private float bobFrequency = 1.0f;
    [SerializeField] private float bobAmplitude = 1.0f;

    [Header("Maximum step height")]
    [SerializeField] private float maximumStepHeight = 2.0f;

    [Header("Event when the path is blocked")]
    [SerializeField] private UnityEvent blockedEvent;

    [Header("Event when the player takes a step")]
    [SerializeField] private UnityEvent stepEvent;
    [SerializeField] private float stepFrequencyMultiplier = 0.5f; // lower = slower bobbing

    public static event Action<int> onPlayerTurned;
    public static Action onPlayerMoved;

    // Animation target values.
    private Vector3 moveTowardsPosition;
    private Quaternion rotateFromDirection;

    // Animation source values.
    private Vector3 moveFromPosition;
    private Quaternion rotateTowardsDirection;

    // Animation progress
    private float rotationTime = 0.0f;
    private float curveTime = 0.0f;
    private float bobTime = 0.0f;

    private float stepTime = 0.0f;
    private float stepTimeCounter = 0.0f;

    //Current settings
    private AnimationCurve currentAnimationCurve;
    private AnimationCurve currentHeadBobCurve;
    private float currentSpeed;

    void Start()
    {
        moveTowardsPosition = transform.position;
        rotateTowardsDirection = transform.rotation;
        currentAnimationCurve = walkSpeedCurve;
        currentHeadBobCurve = walkHeadBobCurve;
        currentSpeed = walkSpeed;
        stepTime = 1.0f / gridSize;
    }

    public void Init(PlayerController playerController)
    {
        controller = playerController;
    }

    void Update()
    {
        if (IsMoving())
        {
            AnimateMovement();
        }

        //if (IsRotating())
        //{
        //    AnimateRotation();
        //}
    }

    public void Teleport(Vector3 destination)
    {
        transform.position = Vector3.zero;
        transform.position = destination;
        moveTowardsPosition = transform.position;
        
    }

    public void SetRotation(float rotation)
    {
        transform.root.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, rotation, transform.rotation.z));
        rotateFromDirection = transform.root.rotation;
        rotateTowardsDirection = transform.root.rotation;
    }

    public float GetTargetRot()
    {
        return rotateTowardsDirection.eulerAngles.y;
    }

    public void SwitchToWalking()
    {
        var currentPosition = currentAnimationCurve.Evaluate(curveTime);
        var newPosition = walkSpeedCurve.Evaluate(curveTime);

        if (newPosition < currentPosition)
        {
            curveTime = FindTimeForValue(currentPosition, walkSpeedCurve);
        }

        currentSpeed = walkSpeed;
        currentAnimationCurve = walkSpeedCurve;
        currentHeadBobCurve = walkHeadBobCurve;
    }

    private float FindTimeForValue(float position, AnimationCurve curve)
    {
        float result = 1.0f;

        while ((position < curve.Evaluate(result)) && (result > 0.0f))
        {
            result -= approximationThreshold;
        }

        return result;
    }

    private void AnimateRotation()
    {
        rotationTime += Time.deltaTime;
        transform.rotation = Quaternion.Slerp(rotateFromDirection, rotateTowardsDirection, rotationTime * turnSpeed);
        CompensateRoundingErrors();
    }

    private void AnimateMovement()
    {
        curveTime += Time.deltaTime * (currentSpeed / 2f); // Used only for movement animation
        bobTime += Time.deltaTime * bobFrequency;          // Bobbing grows at a fixed rate
        stepTimeCounter += Time.deltaTime * (currentSpeed * stepFrequencyMultiplier);

        if (stepTimeCounter > stepTime)
        {
            stepTimeCounter = 0.0f;
            bobTime = 0.0f; //Resets bob sync with each step
            stepEvent?.Invoke();
        }

        var currentPositionValue = currentAnimationCurve.Evaluate(curveTime);
        var currentHeadBobValue = currentHeadBobCurve.Evaluate(bobTime * gridSize) * bobAmplitude;

        if (weaponMotion != null)
        {
            weaponMotion.SetWeaponBobOffset(currentHeadBobValue);
        }

        var targetHeading = Vector3.Normalize(HeightInvariantVector(moveTowardsPosition) - HeightInvariantVector(moveFromPosition));
        var newPosition = moveFromPosition + (targetHeading * (currentPositionValue * gridSize));
        newPosition.y = maximumStepHeight;
        
        RaycastHit hit;
        Ray downRay = new Ray(newPosition, -Vector3.up);

        // Cast a ray straight downwards.
        if (Physics.Raycast(downRay, out hit, .1f))
        {
            newPosition.y = (maximumStepHeight - hit.distance) + currentHeadBobValue;
        }
        else
        {
            newPosition.y = currentHeadBobValue;
        }

        transform.position = newPosition;
        CompensateRoundingErrors();
    }

    private void CompensateRoundingErrors()
    {
        // Bear in mind that floating point numbers are inaccurate by design. 
        // The == operator performs a fuzy compare which means that we are only approximatly near the target value.
        // We may not entirely reached the value yet or we may have slightly overshot it already (both within the margin of error).
        if (transform.rotation == rotateTowardsDirection)
        {
            // To compensate rounding errors we explictly set the transform to our desired rotation.
            transform.rotation = rotateTowardsDirection;
        }

        //mask out the head bobbing
        var currentPosition = HeightInvariantVector(transform.position);
        var target = HeightInvariantVector(moveTowardsPosition);

        if (currentPosition == target)
        {
            // To compensate rounding errors we explictly set the transform to our desired rotation.
            currentPosition = HeightInvariantVector(moveTowardsPosition);
            currentPosition.y = transform.position.y;

            transform.position = currentPosition;
            curveTime = 0.0f;
            stepTimeCounter = 0.0f;
        }

    }

    public void MoveForward()
    {
        if (UIController.isTransitioningLevel) return;

        CollisonCheckedMovement(transform.forward);
    }

    public void MoveBackward()
    {
        if (UIController.isTransitioningLevel) return;

        CollisonCheckedMovement(-transform.forward);
    }

    public void StrafeRight()
    {
        if (UIController.isTransitioningLevel) return;

        CollisonCheckedMovement(transform.right);
    }

    public void StrafeLeft()
    {
        if (UIController.isTransitioningLevel) return;

        CollisonCheckedMovement(-transform.right);
    }

    private void CollisonCheckedMovement(Vector3 movementDirection)
    {
        if (IsStationary())
        {
            if (PlayerInventoryManager.isInContainer)
            {
                WorldInteractionManager.CloseCurrentOpenContainer();
            }

            GridNode targetNode = PlayerController.currentOccupiedNode.GetNodeInDirection(movementDirection);
            if (IsNodeFree(targetNode))
            {
                moveFromPosition = transform.position;
                moveTowardsPosition = targetNode.moveToTransform.position;

                GridController.Instance.GetNodeFromWorldPos(moveFromPosition).ResetOccupant();
                controller.SetCurrentOccupiedNode(targetNode);
                onPlayerMoved?.Invoke();
            }
            else
            {
                blockedEvent?.Invoke();
            }
        }
    }

    private bool IsNodeFree(GridNode targetNode)
    {
        return (targetNode.nodeData.isPlayerWalkable && 
            (targetNode.currentOccupant.occupantType == GridNodeOccupantType.None || 
            targetNode.currentOccupant.occupantType == GridNodeOccupantType.LevelTransition || 
            targetNode.currentOccupant.occupantType == GridNodeOccupantType.PressurePlate ||
            targetNode.currentOccupant.occupantType == GridNodeOccupantType.NPCInaccessible));
    }

    public void TurnRight()
    {
        if (UIController.isTransitioningLevel) return;

        TurnEulerDegrees(RightHand);
    }

    public void TurnLeft()
    {
        if (UIController.isTransitioningLevel) return;

        TurnEulerDegrees(LeftHand);
    }

    private void TurnEulerDegrees(in float eulerDirectionDelta)
    {
        if (!IsRotating() && canRotate)
        {
            canRotate = false;

            if (eulerDirectionDelta > 0)
                onPlayerTurned?.Invoke(-1);
            else
                onPlayerTurned?.Invoke(1);

            // Trigger weapon turn lag
            if (weaponMotion != null)
            {
                float direction = eulerDirectionDelta > 0 ? 1f : -1f;
                weaponMotion.ApplyTurnLag(direction);
            }

            rotateFromDirection = transform.rotation;
            rotateTowardsDirection *= Quaternion.Euler(0.0f, eulerDirectionDelta, 0.0f);
            rotationTime = 0.0f;

            StartCoroutine(RotationDelay());
        }
    }

    IEnumerator RotationDelay()
    {
        yield return new WaitForSeconds(rotationDelay);
        canRotate = true;
    }

    public bool IsStationary()
    {
        return !(IsMoving() /*|| IsRotating()*/);
    }

    private bool IsMoving()
    {
        var current = HeightInvariantVector(transform.position);
        var target = HeightInvariantVector(moveTowardsPosition);
        if(current == target)
        {
            if (PlayerController.currentOccupiedNode.GetIsVoid())
            {
                enabled = false;
                controller.rb.isKinematic = false;
            }
        }
        return current != target;
    }

    private bool IsRotating()
    {
        return transform.rotation != rotateTowardsDirection;
    }

    private Vector3 HeightInvariantVector(Vector3 inVector)
    {
        return new Vector3(inVector.x, 0.0f, inVector.z);
    }

    //private Vector3 CalculateForwardPosition()
    //{
    //    return PlayerController.currentOccupiedNode.GetNodeInDirection(transform.forward); ;
    //}

    //private Vector3 CalculateStrafePosition()
    //{
    //    return transform.right * gridSize;
    //}
}
