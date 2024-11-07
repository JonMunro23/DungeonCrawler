using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldInteraction : MonoBehaviour
{
    UseEquipment useEquipment;
    Camera cam;

    public bool isClickable;

    [SerializeField] float maxInteractionDistance = 3f;

    public static Action OnWorldInteraction;

    private void Awake()
    {
        useEquipment = GetComponent<UseEquipment>();
        cam = transform.GetChild(0).GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
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
