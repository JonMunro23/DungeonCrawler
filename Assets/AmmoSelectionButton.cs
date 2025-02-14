using System;
using UnityEngine;
using UnityEngine.UI;

public class AmmoSelectionButton : MonoBehaviour
{
    [SerializeField] AmmoItemData ammoItemData;
    [SerializeField] Image ammoImage;

    public static Action<AmmoItemData> OnAmmoSelected;

    public void Init(AmmoItemData ammoItemData)
    {
        this.ammoItemData = ammoItemData;
        ammoImage.sprite = this.ammoItemData.itemSprite;
    }

    /// <summary>
    /// Called on button press
    /// </summary>
    public void SelectAmmo()
    {
        OnAmmoSelected?.Invoke(ammoItemData);
    }
}
