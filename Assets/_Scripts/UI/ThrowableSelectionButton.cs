using System;
using UnityEngine;
using UnityEngine.UI;

public class ThrowableSelectionButton : MonoBehaviour
{
    public ThrowableItemData throwableItemData;
    [SerializeField] Image throwableImage;

    public Button button;

    public static Action<ThrowableItemData> OnThrowableSelected;

    public void Init(ThrowableItemData throwableItemData)
    {
        this.throwableItemData = throwableItemData;
        throwableImage.sprite = this.throwableItemData.itemSprite;
    }

    /// <summary>
    /// Called on button press
    /// </summary>
    public void SelectThrowable()
    {
        OnThrowableSelected?.Invoke(throwableItemData);
        Debug.Log("Selected " + throwableItemData.itemName);
    }
}
