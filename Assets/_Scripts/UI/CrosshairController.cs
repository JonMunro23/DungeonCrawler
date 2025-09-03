using UnityEngine;
using UnityEngine.UI;

public enum LookAtTarget
{
    Pickup,
    Interactable,
    Container,
    None
}

public class CrosshairController : MonoBehaviour
{

    [Header("Sprites")]
    [SerializeField] Sprite defaultSprite;
    [SerializeField] Sprite pickupSprite;
    [SerializeField] Sprite interactSprite;
    [SerializeField] Sprite containerSprite;

    [Header("Crosshair Arms")]
    [SerializeField] Image[] crosshairArms;
    [SerializeField] float minSpreadDistance = 25f;
    RangedWeapon currentActiveRangedWeapon;

    [Header("Colors")]
    [SerializeField] Color defaultColor;

    static Image crosshairImage;
    static bool isCrosshairLocked = true;


    private void OnEnable()
    {
        WorldInteractionManager.onLookAtTargetChanged += CurrentLookAtTargetChanged;

        RangedWeapon.onRangedWeaponReadied += OnRangedWeaponReadied;

        WeaponSlot.onWeaponDrawn += OnWeaponDrawn;
    }

    private void OnDisable()
    {
        WorldInteractionManager.onLookAtTargetChanged -= CurrentLookAtTargetChanged;

        RangedWeapon.onRangedWeaponReadied -= OnRangedWeaponReadied;

        WeaponSlot.onWeaponDrawn -= OnWeaponDrawn;
    }

    void OnWeaponDrawn(IWeapon drawnWeapon)
    {
        if(drawnWeapon.GetRangedWeapon() != null)
        {
            currentActiveRangedWeapon = drawnWeapon.GetRangedWeapon();
        }
    }

    private void Awake()
    {
        crosshairImage = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        SetCrosshairToDefault();
    }

    private void Update()
    {
        // Only spread if we have a weapon
        if (currentActiveRangedWeapon != null && currentActiveRangedWeapon.isWeaponReady)
        {
            float spreadMultipler = currentActiveRangedWeapon.GetBulletSpreadMultiplier();
            if (spreadMultipler > 0)
                SetArmsSpreadAmount(spreadMultipler);
        }

        if (isCrosshairLocked)
            return;

        crosshairImage.rectTransform.position = Input.mousePosition;
    }

    void OnRangedWeaponReadied(bool isReadied)
    {
        if(isReadied)
        {
            SetArmsEnabled(true);
        }
        else
        {
            SetArmsEnabled(false);
        }
    }

    private void SetArmsEnabled(bool enabled)
    {
        foreach (Image arm in crosshairArms)
        {
            arm.enabled = enabled;
        }

        crosshairImage.enabled = !enabled;
    }

    private void SetArmsSpreadAmount(float spreadMultiplier)
    {
        // Spread is always at least minSpreadDistance
        float finalSpread = minSpreadDistance + (spreadMultiplier * minSpreadDistance);

        // upper and lower arms move in Y
        crosshairArms[0].rectTransform.anchoredPosition = new Vector2(0, finalSpread); // upper
        crosshairArms[1].rectTransform.anchoredPosition = new Vector2(0, -finalSpread); // lower

        // left and right arms move in X
        crosshairArms[2].rectTransform.anchoredPosition = new Vector2(-finalSpread, 0); // left
        crosshairArms[3].rectTransform.anchoredPosition = new Vector2(finalSpread, 0); // right
    }

    void CurrentLookAtTargetChanged(LookAtTarget newLookAtTarget)
    {
        switch (newLookAtTarget)
        {
            case LookAtTarget.Pickup:
                SetCrosshairSprite(pickupSprite);
                break;
            case LookAtTarget.Interactable:
                SetCrosshairSprite(interactSprite);
                break;
            case LookAtTarget.Container:
                SetCrosshairSprite(containerSprite);
                break;
            case LookAtTarget.None:
                SetCrosshairToDefault();
                break;
        }
    }

    private void SetCrosshairSprite(Sprite newSprite)
    {
        crosshairImage.sprite = newSprite;
        crosshairImage.color = Color.white;
        crosshairImage.rectTransform.sizeDelta = new Vector2(50, 50);
    }

    private void SetCrosshairToDefault()
    {
        crosshairImage.sprite = defaultSprite;
        crosshairImage.color = defaultColor;
        crosshairImage.rectTransform.sizeDelta = new Vector2(10, 10);
    }
    public static void SetCrosshairLocked(bool isLocked)
    {
        if(isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isCrosshairLocked = true;
            crosshairImage.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
            isCrosshairLocked = false;
        }
    }
}
