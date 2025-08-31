using System;
using UnityEngine;
using UnityEngine.UI;

public class ThrowableSelectionButton : MonoBehaviour
{
    public ThrowableItemData throwableItemData;
    [SerializeField] Image throwableImage;

    public Button button;

    public static Action<ThrowableItemData> onThrowableSelected;

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
        onThrowableSelected?.Invoke(throwableItemData);
        Debug.Log("Selected " + throwableItemData.itemName);
    }
}
