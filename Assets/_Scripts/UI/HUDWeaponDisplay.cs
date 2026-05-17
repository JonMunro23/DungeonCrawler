using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDWeaponDisplay : MonoBehaviour
{
    [SerializeField] Sprite pistolAmmoSprite, shellsAmmoSprite, rifleAmmoSprite;
    [SerializeField] Image weaponImage, ammoTypeImage;
    [SerializeField] Image mainBackground, weaponImageBackground, weaponCooldownImage, ammoTypeImageBackground, ammoCounterBackground;
    [SerializeField] TMP_Text ammoText;

    WeaponItem displayedWeapon;

    [SerializeField] int loadedAmmo, reserveAmmo;

    [Header("Primary/Secondary Display Attributes")]
    [SerializeField] bool isPrimaryDisplay;
    RectTransform rectTransfrom;

    Animator animator;
    [SerializeField] float delayBeforeSiblingShift, animationDuration;

    [SerializeField] Vector2 primaryPos, secondaryPos;
    [SerializeField] Color primaryColour, primaryMainBackgroundColour, secondaryColour, secondaryMainBackgroundColour;

    Coroutine displayAnimationCoroutine;

    private void OnEnable()
    {
        Weapon.onWeaponCooldownActive += SetDisplayOnCooldown;
    }

    private void OnDisable()
    {
        Weapon.onWeaponCooldownActive -= SetDisplayOnCooldown;
    }

    private void Awake()
    {
        rectTransfrom = GetComponent<RectTransform>();
        animator = GetComponent<Animator>();
    }

    public void SetDisplayAsPrimary(bool _isPrimary)
    {
        isPrimaryDisplay = _isPrimary;
        animator.enabled = true;

        if(displayAnimationCoroutine != null)
        {
            StopCoroutine(displayAnimationCoroutine);
            displayAnimationCoroutine = null;
        }

        displayAnimationCoroutine = StartCoroutine(AnimateDisplays());
    }

    private IEnumerator AnimateDisplays()
    {
        if (isPrimaryDisplay)
        {
            animator.Play("ToFront");
            mainBackground.color = secondaryMainBackgroundColour;
            weaponImageBackground.color = secondaryColour;
            ammoTypeImageBackground.color = secondaryColour;
            ammoCounterBackground.color = secondaryColour;
            yield return new WaitForSeconds(delayBeforeSiblingShift);
            transform.SetAsLastSibling();
            yield return new WaitForSeconds(animationDuration - delayBeforeSiblingShift);
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
            yield return new WaitForSeconds(delayBeforeSiblingShift);
            transform.SetAsFirstSibling();
            yield return new WaitForSeconds(animationDuration - delayBeforeSiblingShift);
            animator.enabled = false;
            rectTransfrom.anchoredPosition = secondaryPos;

        }

        displayAnimationCoroutine = null;
    }

    public bool GetDisplayActive()
    {
        return isPrimaryDisplay;
    }

    public void UpdateDisplayedWeapon(WeaponItem weaponItem)
    {
        if(displayedWeapon == null || displayedWeapon != weaponItem)
            displayedWeapon = weaponItem;

        UpdateWeaponSprite(displayedWeapon.WeaponItemData.itemSprite);
        UpdateAmmoType(displayedWeapon.LoadedAmmoData);
    }

    void UpdateWeaponSprite(Sprite newSprite)
    {
        weaponImage.sprite = newSprite;
    }

    //public void UpdateAmmoText(int loaded, int reserve)
    //{
    //    reserveAmmo = reserve;
    //    loadedAmmo = loaded;
    //    ammoText.text = $"{loaded}/{reserve}";
    //}

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

    void UpdateAmmoType(AmmoItemData newAmmoData)
    {
        if (newAmmoData == null) return;

        ammoTypeImage.sprite = newAmmoData.itemSprite;
    }

    void SetDisplayOnCooldown(float cooldownLength)
    {
        if (!isPrimaryDisplay)
            return;

        StartCoroutine(Cooldown(cooldownLength));
    }

    IEnumerator Cooldown(float cooldownLength)
    {
        float elapsed = cooldownLength;
        weaponCooldownImage.fillAmount = 1;

        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            float fillAmount = Mathf.Clamp01(elapsed / cooldownLength);
            weaponCooldownImage.fillAmount = fillAmount;
            yield return null;
        }

        weaponCooldownImage.fillAmount = 0;
    }

    
}
