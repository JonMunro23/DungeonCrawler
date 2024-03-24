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

        if (Physics.Raycast(ray, out hit) && !IsPointerOverUI())
        {
            if (DialogueManager.isInDialogue == false)
            {
                if(hit.distance < 10)
                {
                    IInteractive interactive = hit.transform.GetComponent<IInteractive>();
                    if( interactive != null )
                    {
                        isClickable = true;
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            interactive.Interact();
                            if (useEquipment.currentWeapon != null)
                                useEquipment.currentWeapon.GetComponent<Animator>().Play("Interact");
                        }
                    }
                    else
                    {
                        isClickable = false;
                    }
                }




                //Debug.Log(hit.distance);
                //if (hit.transform.CompareTag("Lever") && hit.distance < 5)
                //{
                //    isClickable = true;
                //    if (Input.GetKeyDown(KeyCode.Mouse0))
                //    {
                //        Lever lever = hit.transform.GetComponent<Lever>();
                //        lever.Pulled();
                //    }
                //}
                //else if (hit.transform.CompareTag("Container") && hit.distance < 5)
                //{
                //    isClickable = true;
                //    if (Input.GetKeyDown(KeyCode.Mouse0))
                //    {
                //        hit.transform.GetComponentInParent<Container>().OpenContainer();
                //    }
                //}
                //else if (hit.transform.CompareTag("TorchSconce") && hit.distance < 10)
                //{
                //    isClickable = true;
                //    if (Input.GetKeyDown(KeyCode.Mouse0) && itemPickup.canPickUpItem == true)
                //    {
                //        hit.transform.GetComponent<TorchSconce>().Interact();
                //    }
                //}
                //else if (hit.transform.CompareTag("NPC") && hit.distance < 10)
                //{
                //    isClickable = true;
                //    if (Input.GetKeyDown(KeyCode.Mouse0))
                //    {
                //        hit.transform.GetComponent<NPC>().TriggerDialogue();
                //    }
                //}
            }
        }
    }
    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
