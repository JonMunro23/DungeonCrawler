using System;
using UnityEngine;
using UnityEngine.UI;

public class AmmoSelectionButton : MonoBehaviour
{
    public AmmoItemData ammoItemData;
    [SerializeField] Image ammoImage;

    public Button button;

    IWeapon weapon;

    public static Action<AmmoItemData> OnAmmoSelected;

    public void Init(AmmoItemData ammoItemData, IWeapon currentHeldWeapon)
    {
        this.weapon = currentHeldWeapon;
        this.ammoItemData = ammoItemData;
        ammoImage.sprite = this.ammoItemData.itemSprite;
    }

    /// <summary>
    /// Called on button press
    /// </summary>
    public void SelectAmmo()
    {
        if (!weapon.GetRangedWeapon().IsReloading())
            OnAmmoSelected?.Invoke(ammoItemData);
    }
}
