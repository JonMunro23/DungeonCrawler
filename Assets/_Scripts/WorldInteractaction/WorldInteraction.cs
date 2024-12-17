using System;
using UnityEngine;

public class WorldInteraction : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] float maxInteractionDistance = 3f;

    public static Action OnWorldInteraction;


    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        Ray ray = playerController.playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxInteractionDistance))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (hit.collider.TryGetComponent(out IPickup pickup))
                {
                    pickup.Pickup(true);
                    OnWorldInteraction?.Invoke();
                }
                else if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (interactable.GetIsPressurePlate())
                        return;

                    ItemData currentGrabbedItemData = playerController.itemPickupManager.currentGrabbedItem.itemData;
                    if (currentGrabbedItemData != null)
                    {
                        interactable.InteractWithItem(currentGrabbedItemData);
                    }
                    else
                        interactable.Interact();

                    OnWorldInteraction?.Invoke();
                }
            }
        }
    }
}
