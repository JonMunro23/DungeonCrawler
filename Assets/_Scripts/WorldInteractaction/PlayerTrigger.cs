using UnityEngine;

public class PlayerTrigger : InteractableBase
{
    public override void Interact()
    {
        TriggerObjects();
        if(isSingleUse)
            GetComponent<BoxCollider>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Yeet");
            Interact();
        }
    }
}
