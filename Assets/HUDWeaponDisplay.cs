using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDWeaponDisplay : MonoBehaviour
{
    [SerializeField] Sprite pistolAmmoSprite, shellsAmmoSprite, rifleAmmoSprite;
    [SerializeField] Image weaponImage, ammoTypeImage;
    [SerializeField] Image mainBackground, weaponImageBackground, ammoTypeImageBackground, ammoCounterBackground;
    [SerializeField] TMP_Text ammoText;

    WeaponItemData displayedData;

    [SerializeField] int loadedAmmo, reserveAmmo;

    [Header("Primary/Secondary Display Attributes")]
    [SerializeField] bool isPrimaryDisplay;
    RectTransform rectTransfrom;

    Animator animator;
    [SerializeField] float delayBeforeSiblingShift, animationDuration;

    [SerializeField] Vector2 primaryPos, secondaryPos;
    [SerializeField] Color primaryColour, primaryMainBackgroundColour, secondaryColour, secondaryMainBackgroundColour;

    private void Awake()
    {
        rectTransfrom = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
    }

    public async void SetDisplayAsPrimary(bool _isPrimary)
    {
        isPrimaryDisplay = _isPrimary;
        animator.enabled = true;

        if (isPrimaryDisplay)
        {
            animator.Play("ToFront");
            mainBackground.color = secondaryMainBackgroundColour;
            weaponImageBackground.color = secondaryColour;
            ammoTypeImageBackground.color = secondaryColour;
            ammoCounterBackground.color = secondaryColour;
            await Task.Delay((int)(delayBeforeSiblingShift * 1000));
            transform.SetAsLastSibling();
            await Task.Delay((int)((animationDuration - delayBeforeSiblingShift) * 1000));
            animator.enabled = false;
            rectTransfrom.anchoredPosition = primaryPos;
            mainBackground.color = primaryMainBackgroundColour;
            weaponImageBackground.color = primaryColour;
            ammoTypeImageBackground.color = primaryColour;
            ammoCounterBackground.color = primaryColour;
        }
        else
        {
            animator.Play("ToBack");
            mainBackground.color = secondaryMainBackgroundColour;
            weaponImageBackground.color = secondaryColour;
            ammoTypeImageBackground.color = secondaryColour;
            ammoCounterBackground.color = secondaryColour;
            await Task.Delay((int)(delayBeforeSiblingShift * 1000)); 
            transform.SetAsFirstSibling();
            await Task.Delay((int)((animationDuration - delayBeforeSiblingShift) * 1000));
            animator.enabled = false;
            rectTransfrom.anchoredPosition = secondaryPos;
            
        }
    }

    public bool GetDisplayActive()
    {
        return isPrimaryDisplay;
    }

    public void UpdateWeaponData(WeaponItemData newWeaponData)
    {
        if(displayedData == null || displayedData != newWeaponData)
            displayedData = newWeaponData;

        UpdateWeaponSprite(displayedData.itemSprite);
        UpdateAmmoType(displayedData.ammoType);
    }

    void UpdateWeaponSprite(Sprite newSprite)
    {
        weaponImage.sprite = newSprite;
    }

    public void UpdateAmmoText(int loaded, int reserve)
    {
        reserveAmmo = reserve;
        loadedAmmo = loaded;
        ammoText.text = $"{loaded}/{reserve}";
    }

    public void UpdateLoadedAmmoText(int loaded)
    {
        loadedAmmo = loaded;
        ammoText.text = $"{loaded}/{reserveAmmo}";
    }

    public void UpdateReserveAmmoText(int reserve)
    {
        reserveAmmo = reserve;
        ammoText.text = $"{loadedAmmo}/{reserve}";
    }

    void UpdateAmmoType(AmmoType newAmmoType)
    {
        switch (newAmmoType)
        {
            case AmmoType.Pistol:
                ammoTypeImage.sprite = pistolAmmoSprite;
                break;
            case AmmoType.Rifle:
                ammoTypeImage.sprite = rifleAmmoSprite;
                break;
            case AmmoType.Shells:
                ammoTypeImage.sprite = shellsAmmoSprite;
                break;
        }
    }
}
