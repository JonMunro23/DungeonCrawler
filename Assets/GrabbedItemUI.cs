using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrabbedItemUI : MonoBehaviour
{
    [SerializeField] Image grabbedItemImg;
    [SerializeField] TMP_Text grabbedItemAmount;
    bool hasGrabbedItem;
    private void OnEnable()
    {
        ItemPickupManager.onNewItemAttachedToCursor += InitGrabbedItem;
        ItemPickupManager.onCurrentItemDettachedFromCursor += ClearGrabbedItem;
    }

    private void OnDisable()
    {
        ItemPickupManager.onNewItemAttachedToCursor -= InitGrabbedItem;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= ClearGrabbedItem;
    }

    private void Start()
    {
        ClearGrabbedItem();
    }

    private void Update()
    {
        if (hasGrabbedItem)
            transform.position = Input.mousePosition;
    }

    public void InitGrabbedItem(Item grabbedItem)
    {
        grabbedItemImg.enabled = true;
        grabbedItemImg.sprite = grabbedItem.itemData.itemSprite;
        grabbedItemAmount.text = grabbedItem.itemAmount.ToString();

        hasGrabbedItem = true;
    }

    public void ClearGrabbedItem()
    {
        grabbedItemImg.enabled = false;
        grabbedItemAmount.text = "";

        hasGrabbedItem = false;
    }
}
