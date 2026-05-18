using UnityEngine;

public class FreeCameraMovement : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;
    private float xRotation = 0f;

    bool isWeaponAmmoSelectionMenuOpen;

    PlayerController controller;
    PlayerControls controls;

    private void OnEnable()
    {
        PlayerWeaponManager.onWeaponAmmoSelectionMenuOpened += OnWeaponAmmoSelectionMenuOpened;
        PlayerWeaponManager.onWeaponAmmoSelectionMenuClosed += OnWeaponAmmoSelectionMenuClosed;
    }

    private void OnDisable()
    {
        PlayerWeaponManager.onWeaponAmmoSelectionMenuOpened -= OnWeaponAmmoSelectionMenuOpened;
        PlayerWeaponManager.onWeaponAmmoSelectionMenuClosed -= OnWeaponAmmoSelectionMenuClosed;
    }

    void OnWeaponAmmoSelectionMenuOpened(IWeapon equippedWeapon)
    {
        isWeaponAmmoSelectionMenuOpen = true;
    }

    void OnWeaponAmmoSelectionMenuClosed()
    {
        isWeaponAmmoSelectionMenuOpen = false;
    }

    public void Init(PlayerController controller)
    {
        this.controller = controller;
        controls = controller.GetPlayerControls();
    }

    void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Tick()
    {
        if (CharacterMenuUIController.isCharacterMenuOpen ||
            MapController.isMapOpen ||
            ThrowableSelectionManager.isThrowableSelectionMenuOpen ||
            isWeaponAmmoSelectionMenuOpen)
            return;

        Vector2 lookInput = controls.Player.Look.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Vertical rotation
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Camera rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Player body rotation
        playerBody.Rotate(Vector3.up * mouseX);
    }

    public Vector3 GetRotation() { return new Vector3(transform.eulerAngles.x, playerBody.eulerAngles.y, 0); }

    public void SetRoation(Vector3 newRotation)
    {
        transform.localRotation = Quaternion.Euler(newRotation.x, 0, 0);
        playerBody.localRotation = Quaternion.Euler(0, newRotation.y, 0);
    }
}
