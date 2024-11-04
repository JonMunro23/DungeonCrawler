using UnityEngine;
using UnityEngine.EventSystems;

public class WorldInteraction : MonoBehaviour
{
    UseEquipment useEquipment;
    Camera cam;

    public bool isClickable;

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
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.TryGetComponent(out IPickup pickup))
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    pickup.GrabPickup();

                    //move this somewhere else
                    if (useEquipment.currentWeapon != null)
                        useEquipment.currentWeapon.GetComponent<Animator>().Play("Interact");
                }
            }
            else if(hit.collider.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();

                //move this somewhere else
                if (useEquipment.currentWeapon != null)
                    useEquipment.currentWeapon.GetComponent<Animator>().Play("Interact");
            }


        }
    }
}
