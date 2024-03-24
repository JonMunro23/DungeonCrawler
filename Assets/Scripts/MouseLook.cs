using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float _xRotation = 0f;
    [SerializeField]
    Quaternion originalRot, originalBodyRot;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(2))
        {
            originalRot = transform.localRotation;
            originalBodyRot = playerBody.transform.rotation;
            _xRotation = 7.5f;
            
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -45f, 45f);

            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        else if(Input.GetMouseButtonUp(2))
        {
            transform.localRotation = originalRot;
            playerBody.transform.rotation = originalBodyRot;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
