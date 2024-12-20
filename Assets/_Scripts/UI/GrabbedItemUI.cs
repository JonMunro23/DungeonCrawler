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
        WorldInteractionManager.onNewItemAttachedToCursor += InitGrabbedItem;
        WorldInteractionManager.onCurrentItemDettachedFromCursor += ClearGrabbedItem;
    }

    private void OnDisable()
    {
        WorldInteractionManager.onNewItemAttachedToCursor -= InitGrabbedItem;
        WorldInteractionManager.onCurrentItemDettachedFromCursor -= ClearGrabbedItem;
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

    public void InitGrabbedItem(ItemStack grabbedItem)
    {
        grabbedItemImg.enabled = true;
        grabbedItemImg.sprite = grabbedItem.itemData.itemSprite;
        if(grabbedItem.itemAmount > 1)
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
