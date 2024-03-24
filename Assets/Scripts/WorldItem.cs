using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractive
{
    ItemPickup itemPickup;

    public ItemObject itemObject;
    public int amount;

    public bool isOnPressurePlate;

    private void Awake()
    {
        itemPickup = GameObject.FindGameObjectWithTag("Player").GetComponent<ItemPickup>();
    }

    public void Interact()
    {
        PickupItem();
    }

    void PickupItem()
    {
        itemPickup.WorldItemToMouse(gameObject);
    }
}
