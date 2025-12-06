using UnityEngine;

public class BillboardFX : MonoBehaviour
{
    [SerializeField] Transform camTransform;
    Quaternion originalRotation;

    private void OnEnable()
    {
        PlayerController.onPlayerInitialised += OnPlayerInitalised;
    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitalised;

    }

    void OnPlayerInitalised(PlayerController initialisedPlayerController)
    {
        camTransform = initialisedPlayerController.playerCamera.transform;
    }

    private void Awake()
    {
        if (!camTransform)
            camTransform = Camera.main.transform;
    }

    void Start()
    {
        originalRotation = transform.rotation * Quaternion.Euler(new Vector3(0,180,0));
    }

    void Update()
    {
        transform.rotation = camTransform.rotation * originalRotation;
    }
}
