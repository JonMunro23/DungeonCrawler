using UnityEngine;

public class WeaponMotion : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float breathingSpeed = 3;
    public float breathingAmplitudeX = 0.001f;
    public float breathingAmplitudeY = 0.0015f;
    public float breathingSmoothing = 5f;

    [Header("Weapon Bob Settings")]
    public float weaponBobMultiplier = .5f;
    private Vector3 weaponBobOffset;

    [Header("Turn Lag Settings")]
    public float turnLagAngle = 5f; //Max rotation offset in degrees	
    public float turnLagInSpeed = 10f; //How quickly it tilts into the lag	
    public float turnLagOutSpeed = 5f; //How quickly it returns to center	
    private float turnLagLerp = 0f;
    private float turnLagDirection = 0f;

    private Quaternion baseRotation;
    private Quaternion targetTurnRotation = Quaternion.identity;
    private Quaternion currentTurnRotation = Quaternion.identity;

    private Vector3 basePosition;
    private Vector3 breathingOffset;

    private void OnEnable()
    {
        RangedWeapon.onRangedWeaponFired += OnRangedWeaponFired;

    }

    private void OnDisable()
    {
        RangedWeapon.onRangedWeaponFired -= OnRangedWeaponFired;
    }

    void OnRangedWeaponFired(WeaponItemData weaponItemData)
    {
        WeaponRecoil(weaponItemData.recoilData);
    }

    private void Start()
    {
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
    }

    private void Update()
    {
        if (PauseMenu.isPaused || !PlayerController.isPlayerAlive)
            return;

        float time = Time.time;

        // ===== BREATHING MOTION =====
        float breathX = Mathf.Sin(time * breathingSpeed) * breathingAmplitudeX;
        float breathY = Mathf.Cos(time * breathingSpeed * 0.75f) * breathingAmplitudeY;
        breathingOffset = new Vector3(breathX, breathY, 0);

        // Smooth turn lag rotation
        if (turnLagLerp < 1f)
        {
            turnLagLerp += Time.deltaTime * turnLagInSpeed;
            float angle = Mathf.Lerp(0, turnLagAngle * turnLagDirection, turnLagLerp);
            targetTurnRotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            // Once full lag reached, start returning to center
            targetTurnRotation = Quaternion.Slerp(targetTurnRotation, Quaternion.identity, Time.deltaTime * turnLagOutSpeed);
        }

        currentTurnRotation = Quaternion.Slerp(currentTurnRotation, targetTurnRotation, Time.deltaTime * turnLagInSpeed);
        transform.localRotation = baseRotation * currentTurnRotation;

        // ===== FINAL POSITION =====
        Vector3 targetPosition = basePosition + breathingOffset + (weaponBobOffset * weaponBobMultiplier);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * breathingSmoothing);
    }

    public void WeaponRecoil(WeaponRecoilData recoilData)
    {
        Vector3 recoil = recoilData.GetRandomPrimaryFireRecoilValue();
        transform.localPosition += recoil;
    }

    public void SetWeaponBobOffset(float bobAmount)
    {
        weaponBobOffset = new Vector3(0, -bobAmount, 0);
    }

    public void ApplyTurnLag(float direction)
    {
        // direction = -1 (left), 1 (right)
        turnLagDirection = -direction;
        turnLagLerp = 0f; // Reset interpolation
    }
}
