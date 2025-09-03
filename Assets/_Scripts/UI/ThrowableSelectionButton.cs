using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThrowableSelectionButton : MonoBehaviour
{
    public ThrowableItemData throwableItemData;
    public int availableAmount;
    [SerializeField] Image throwableImage;
    [SerializeField] TMP_Text throwableAmountText;

    public Button button;

    public static Action<ThrowableItemData, int> onThrowableSelected;

    public void Init(ThrowableItemData throwableItemData, int amount)
    {
        this.throwableItemData = throwableItemData;
        availableAmount = amount;
        throwableImage.sprite = this.throwableItemData.itemSprite;
        throwableAmountText.text = amount.ToString();
    }

    /// <summary>
    /// Called on button press
    /// </summary>
    public void SelectThrowable()
    {
        onThrowableSelected?.Invoke(throwableItemData, availableAmount);
        //button.interactable = false;
    }
}
