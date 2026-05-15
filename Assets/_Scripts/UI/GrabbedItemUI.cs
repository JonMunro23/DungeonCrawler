using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GrabbedItemUI : MonoBehaviour
{
    [SerializeField] Image grabbedItemImg;
    [SerializeField] TMP_Text grabbedItemAmount;
    bool hasGrabbedItem;

    PlayerControls controls;
    private void OnEnable()
    {
        WorldInteractionManager.onNewItemAttachedToCursor += InitGrabbedItem;
        WorldInteractionManager.onCurrentItemDettachedFromCursor += ClearGrabbedItem;

        PlayerInputHandler.OnPlayerControlsInitialised += OnPlayerControlsInitialised;
    }

    private void OnDisable()
    {
        WorldInteractionManager.onNewItemAttachedToCursor -= InitGrabbedItem;
        WorldInteractionManager.onCurrentItemDettachedFromCursor -= ClearGrabbedItem;

        PlayerInputHandler.OnPlayerControlsInitialised -= OnPlayerControlsInitialised;
    }

    void OnPlayerControlsInitialised(PlayerControls controls)
    {
        this.controls = controls;
    }

    private void Start()
    {
        ClearGrabbedItem();
    }

    private void Update()
    {
        if (hasGrabbedItem)
            transform.position = controls.Player.MousePos.ReadValue<Vector2>();
    }

    public void InitGrabbedItem(ItemStack grabbedItem)
    {
        grabbedItemImg.enabled = true;
        grabbedItemImg.sprite = grabbedItem.Item.ItemData.itemSprite;
        if(grabbedItem.ItemAmount > 1)
            grabbedItemAmount.text = grabbedItem.ItemAmount.ToString();

        hasGrabbedItem = true;
    }

    public void ClearGrabbedItem()
    {
        grabbedItemImg.enabled = false;
        grabbedItemAmount.text = "";

        hasGrabbedItem = false;
    }
}
