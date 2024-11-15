using System;
using UnityEngine;

public class WorldInteraction : MonoBehaviour
{
    Camera playerCam;

    [SerializeField] float maxInteractionDistance = 3f;

    public static Action OnWorldInteraction;

    private void Awake()
    {
        playerCam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxInteractionDistance))
        {
            if (hit.collider.TryGetComponent(out IPickup pickup))
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    pickup.GrabPickup();
                    OnWorldInteraction?.Invoke();
                }
            }
            else if(hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
        }
    }
}
