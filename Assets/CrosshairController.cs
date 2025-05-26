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

    [Header("Colors")]
    [SerializeField] Color defaultColor;

    static Image crosshairImage;
    static bool isCrosshairLocked = true;

    private void OnEnable()
    {
        WorldInteractionManager.onLookAtTargetChanged += CurrentLookAtTargetChanged;
        RangedWeapon.onRangedWeaponReadied += OnRangedWeaponReadied;
    }

    private void OnDisable()
    {
        WorldInteractionManager.onLookAtTargetChanged -= CurrentLookAtTargetChanged;
        RangedWeapon.onRangedWeaponReadied -= OnRangedWeaponReadied;
    }

    private void Awake()
    {
        crosshairImage = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        crosshairImage.color = defaultColor;
    }

    private void Update()
    {
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
