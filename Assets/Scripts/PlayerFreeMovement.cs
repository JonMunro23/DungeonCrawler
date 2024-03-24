using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeMovement : MonoBehaviour
{

    public CharacterController playerController;
    public float speed = 12f;
    Vector3 velocity;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    bool _isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        //If holding SHIFT, initiates a sprint
        if (Input.GetButton("Sprint"))
        {
            playerController.Move(move * (speed * 1.6f) * Time.deltaTime);
        }//If holding LEFT CTRL, initiates a crouch
        else if (Input.GetButton("Crouch"))
        {
            playerController.Move(move * (speed*0.6f) * Time.deltaTime);
        }
        else
        {
            playerController.Move(move * speed * Time.deltaTime);
        }
        //If SPACE is pressed, initiates a jump
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        playerController.Move(velocity * Time.deltaTime);
    }
}
