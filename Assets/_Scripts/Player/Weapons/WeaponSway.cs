using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Settings")]
    public bool sway = true;
    public bool swayRotation = true;
    public bool bobOffset = true;
    public bool bobSway = true;

    [Header("Sway")]
    public float step = 0.01f;
    public float maxStepDistance = 0.06f;
    Vector3 swayPos;

    [Header("Sway Rotation")]
    public float rotationStep = 4f;
    public float maxRotationStep = 5f;
    Vector3 swayEulerRot;

    public float smooth = 10f;
    float smoothRot = 12f;

    [Header("Bobbing")]
    public float speedCurve;
    float curveSin { get => Mathf.Sin(speedCurve); }
    float curveCos { get => Mathf.Cos(speedCurve); }

    public Vector3 travelLimit = Vector3.one * 0.025f;
    public Vector3 bobLimit = Vector3.one * 0.01f;
    Vector3 bobPosition;

    public float bobExaggeration;

    [Header("Bob Rotation")]
    public Vector3 multiplier;
    Vector3 bobEulerRotation;
    private void OnEnable()
    {
        AdvancedGridMovement.turnEvent += DecideSwayDirection;
    }

    private void OnDisable()
    {
        AdvancedGridMovement.turnEvent -= DecideSwayDirection;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        Sway();
        SwayRotation();
        BobOffset();
       // BobRotation();

        CompositePositionRotation();
    }


    Vector2 walkInput;
    Vector2 lookInput;

    void GetInput()
    {
        //walkInput.x = Input.GetAxis("Horizontal");
        //walkInput.y = Input.GetAxis("Vertical");
        //walkInput = walkInput.normalized;

        //lookInput.x = Input.GetAxis("Mouse X");
        //lookInput.y = Input.GetAxis("Mouse Y");

        if (lookInput.x < 0)
        {
            lookInput.x += Time.deltaTime * rotationStep;
        }

        if (lookInput.x > 0)
        {
            lookInput.x -= Time.deltaTime * rotationStep;
        }
    }

    void DecideSwayDirection(int direction)
    {
        if (direction == 0)
        {
            SwayRight();
        }
        else
            SwayLeft();
    }

    void SwayLeft()
    {
        lookInput.x = -1;
    }

    void SwayRight()
    {
        lookInput.x = 1;
    }

    void Sway()
    {
        if(sway == false) { swayPos = Vector3.zero; return; }

        Vector3 invertLook = lookInput * -step;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

        swayPos = invertLook;
    }

    void SwayRotation()
    {
        if (swayRotation == false) { swayEulerRot = Vector3.zero; return; }

        Vector2 invertLook = lookInput * -rotationStep;
        invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
        swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
    }

    void CompositePositionRotation()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, swayPos + bobPosition, Time.deltaTime * smooth);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(swayEulerRot) * Quaternion.Euler(bobEulerRotation), Time.deltaTime * smoothRot);
    }

    void BobOffset()
    {
        speedCurve += Time.deltaTime + 0.001f;
        
        if (bobOffset == false) { bobPosition = Vector3.zero; return; }       

        bobPosition.x = (curveCos * bobLimit.x) - (walkInput.x * travelLimit.x);
        bobPosition.y = (curveSin * bobLimit.y) - (Input.GetAxis("Vertical") * travelLimit.y);
        bobPosition.z = -(walkInput.y * travelLimit.z);
    }

    void BobRotation()
    {
        if (bobSway == false) { bobEulerRotation = Vector3.zero; return; }

        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2));
        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    }

    //public void ToggleAiming(bool _isAiming, WeaponShooting weaponToToggle)
    //{
    //    if (_isAiming)
    //    {
    //        RestoreBob();
    //    }
    //    else if (!_isAiming)
    //    {
    //        ReduceBob();
    //    }
    //}
    public void ReduceBob()
    {
        bobLimit = Vector3.one * 0.001f; travelLimit = Vector3.one * 0.005f;
    }
    public void RestoreBob()
    {
        bobLimit = Vector3.one * 0.01f; travelLimit = Vector3.one * 0.025f;
    }
}