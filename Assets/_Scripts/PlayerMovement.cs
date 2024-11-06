using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject player;

    RaycastHit hitForward;
    RaycastHit hitBackward;
    RaycastHit hitLeft;
    RaycastHit hitRight;

    int layerMask = 1 << 10;

    [SerializeField]
    GameObject _frontT, _rearT, _leftT, _rightT;

    [SerializeField]
    float _moveSpeed = 0.02f;
    //[SerializeField]
    //int _moveDistance = 5;

    bool _canMove = true, _canRotate = true;
    public bool isMoving = false;

    // Update is called once per frame
    void Update()
    {
        Raycasting();
        PlayerInput();

    }

    private void Raycasting()
    {
        if (Physics.Raycast(player.transform.position, player.transform.TransformDirection(Vector3.forward), out hitForward, 100, layerMask))
        {
            Debug.DrawRay(player.transform.position, player.transform.TransformDirection(Vector3.forward) * hitForward.distance, Color.yellow);
        }

        if (Physics.Raycast(player.transform.position, player.transform.TransformDirection(Vector3.back), out hitBackward, 100, layerMask))
        {
            //Debug.DrawRay(partyObject.transform.position, partyObject.transform.TransformDirection(Vector3.back) * hitBackward.distance, Color.yellow);
        }

        if (Physics.Raycast(player.transform.position, player.transform.TransformDirection(Vector3.left), out hitLeft, 100, layerMask))
        {
            //Debug.DrawRay(partyObject.transform.position, partyObject.transform.TransformDirection(Vector3.left) * hitLeft.distance, Color.yellow);
        }

        if (Physics.Raycast(player.transform.position, player.transform.TransformDirection(Vector3.right), out hitRight, 100, layerMask))
        {
            //Debug.DrawRay(partyObject.transform.position, partyObject.transform.TransformDirection(Vector3.right) * hitRight.distance, Color.yellow);
        }
    }

    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && _canRotate == true)
        {
            _canMove = false;
            _canRotate = false;
            isMoving = true;
            StartCoroutine(LookLeft());
        }

        IEnumerator LookLeft()
        {
            for (int i = 0; i < 10; i++)
            {
                player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, _leftT.transform.rotation, 9);
                yield return new WaitForSeconds(.01f);
            }
            _canRotate = true;
            _canMove = true;
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.E) && _canRotate == true)
        {
            _canMove = false;
            _canRotate = false;
            isMoving = true;
            StartCoroutine(LookRight());
        }

        IEnumerator LookRight()
        {
            for (int i = 0; i < 10; i++)
            {
                player.transform.rotation = Quaternion.RotateTowards(player.transform.rotation, _rightT.transform.rotation, 9);
                yield return new WaitForSeconds(.01f);
            }
            _canRotate = true;
            _canMove = true;
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.W) && hitForward.distance > 9 && _canMove == true)
        {
            _canMove = false;
            _canRotate = false;
            isMoving = true;

            StartCoroutine(MoveForwards());
        }

        IEnumerator MoveForwards()
        {
            for (int i = 0; i < 18; i++)
            {
                player.transform.localPosition = Vector3.MoveTowards(player.transform.position, _frontT.transform.position, .5f);
                yield return new WaitForSeconds(_moveSpeed);
            }
            _canMove = true;
            _canRotate = true;
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.S) && hitBackward.distance > 9 && _canMove == true)
        {
            _canMove = false;
            _canRotate = false;
            isMoving = true;

            StartCoroutine(MoveBackwards());
        }

        IEnumerator MoveBackwards()
        {
            for (int i = 0; i < 18; i++)
            {
                player.transform.localPosition = Vector3.MoveTowards(player.transform.position, _rearT.transform.position, .5f);
                yield return new WaitForSeconds(_moveSpeed);
            }
            _canMove = true;
            _canRotate = true;
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.A) && hitLeft.distance > 9 && _canMove == true)
        {
            _canMove = false;
            _canRotate = false;
            isMoving = true;

            StartCoroutine(MoveLeft());
        }

        IEnumerator MoveLeft()
        {
            for (int i = 0; i < 18; i++)
            {
                player.transform.localPosition = Vector3.MoveTowards(player.transform.position, _leftT.transform.position, .5f);
                yield return new WaitForSeconds(_moveSpeed);
            }
            _canMove = true;
            _canRotate = true;
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.D) && hitRight.distance > 9 && _canMove == true)
        {
            _canMove = false;
            _canRotate = false;
            isMoving = true;

            StartCoroutine(MoveRight());
        }

        IEnumerator MoveRight()
        {
            for (int i = 0; i < 18; i++)
            {
                player.transform.localPosition = Vector3.MoveTowards(player.transform.position, _rightT.transform.position, .5f);
                yield return new WaitForSeconds(_moveSpeed);
            }
            _canMove = true;
            _canRotate = true;
            isMoving = false;

        }

    }

    public bool CheckForward(string tag)
    {
        if(hitForward.distance <= 9)
        {
            if(hitForward.collider.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    public GameObject GetForwardObject()
    {
        return hitForward.collider.gameObject;
    }
}
