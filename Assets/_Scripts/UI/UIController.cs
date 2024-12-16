using DG.Tweening;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] PlayerStatsUIController playerStatsUIController;
    [SerializeField] PlayerInventoryUIController playerInventoryUIController;
    [SerializeField] PlayerEquipmentUIManager PlayerEquipmentUIManager;
    [SerializeField] PlayerWeaponUIManager playerWeaponUIManager;
    [SerializeField] PlayerSkillsUIManager playerSkillsUIManager;


    [Header("Level Transition")]
    [SerializeField] GameObject levelTransitionParent;
    [SerializeField] TMP_Text levelTransitionText;
    [SerializeField] TMP_Text levelTransitionEnteringText;
    [SerializeField] Image levelTransitionDividingLine;
    [SerializeField] float levelTextLifetimeDuration = 5;
    [SerializeField] Image levelTransitionFadeOverlay;
    [SerializeField] float fadeOutDuration, fadeInDuration;

    [Header("Quick Saving")]
    [SerializeField] TMP_Text saveStatusText;
    [SerializeField] float saveStatusTextFadeDuration;

    PlayerController initialisedPlayer;

    Coroutine levelTextLifetime;

    WeaponItemData defaultWeaponData;

    private void OnEnable()
    {
        PlayerController.onPlayerInitialised += OnPlayerInitialised;

        ItemPickupManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor += OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault += OnWeaponSetToDefault;

        Weapon.onAmmoUpdated += OnWeaponAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive += OnWeaponSlotSetActive;
        PlayerWeaponManager.onNewWeaponInitialised += OnNewWeaponInitialised;

        LevelTransition.onLevelTransitionEntered += OnLevelTransitionEntered;

        GridController.onQuickSave += OnQuickSave;
    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitialised;

        ItemPickupManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault -= OnWeaponSetToDefault;

        Weapon.onAmmoUpdated -= OnWeaponAmmoUpdated;

        PlayerWeaponManager.onNewWeaponInitialised -= OnNewWeaponInitialised;
        PlayerWeaponManager.onWeaponSlotSetActive -= OnWeaponSlotSetActive;

        LevelTransition.onLevelTransitionEntered -= OnLevelTransitionEntered;

        GridController.onQuickSave -= OnQuickSave;
    }

    private void Start()
    {
        levelTransitionFadeOverlay.enabled = true;
    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        playerStatsUIController.InitStatsUI(initialisedPlayer.playerCharacterData);

        _ = FadeInScreen();
    }

    void OnNewItemAttachedToCursor(ItemStack item)
    {
        WeaponItemData handItemData = item.itemData as WeaponItemData;
        if (handItemData != null)
        {
            PlayerEquipmentUIManager.DisableAllSlots();
            return;
        }

        EquipmentItemData equipItemData = item.itemData as EquipmentItemData;
        if (equipItemData != null)
        {
            PlayerEquipmentUIManager.DisableSlotsNotOfType(equipItemData.EquipmentSlotType);
            playerWeaponUIManager.DisableSlots();
            return;
        }

        playerWeaponUIManager.DisableSlots();
        PlayerEquipmentUIManager.DisableAllSlots();
    }

    void OnCurrentItemRemovedFromCursor()
    {
        PlayerEquipmentUIManager.RenableSlots();
        playerWeaponUIManager.RenableSlots();
    }

    void OnNewWeaponInitialised(int slotIndex, WeaponItemData newItemData)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, newItemData);
    }

    void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData dataToSwapTo, int loadedAmmo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, dataToSwapTo);
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, defaultWeaponData);
    }

    void OnWeaponSetToDefault(int slotIndex, WeaponItemData _defaultWeaponData)
    {
        defaultWeaponData = _defaultWeaponData;
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, _defaultWeaponData);
    }

    void OnWeaponAmmoUpdated(int slotIndex, int loaded, int reserve)
    {
        playerWeaponUIManager.UpdateWeaponDisplayAmmoCount(slotIndex, loaded, reserve);
    }

    void OnWeaponSlotSetActive(int slotIndex)
    {
        playerWeaponUIManager.SetSlotActive(slotIndex);
    }

    async void OnLevelTransitionEntered(int levelIndex, Vector2 playerMoveToCoords)
    {
        await FadeOutScreen();
        await GridController.Instance.BeginLevelTransition(levelIndex, playerMoveToCoords);
        await FadeInScreen();
        ShowLevelName(levelIndex);
    }

    void OnQuickSave()
    {
        saveStatusText.color = new Color(1, 0.9529412f, 0, 1);
        saveStatusText.DOFade(0, saveStatusTextFadeDuration).SetDelay(3);
    }

    void ShowLevelName(int levelIndex)
    {
        levelTransitionParent.SetActive(true);
        levelTransitionText.color = new Color(1,1,1,1);
        levelTransitionEnteringText.color = new Color(1,1,1,1);
        levelTransitionDividingLine.color = new Color(.51f,.51f,.51f,1);
        levelTransitionText.text = GridController.Instance.GetLevelNameFromIndex(levelIndex);

        if(levelTextLifetime != null)
            StopCoroutine(levelTextLifetime);

        levelTextLifetime = StartCoroutine(LevelNameLifetime());
    }

    void HideLevelName()
    {
        levelTransitionText.DOFade(0, 1);
        levelTransitionEnteringText.DOFade(0, 1);
        levelTransitionDividingLine.DOFade(0, 1);
    }

    async Task FadeInScreen()
    {
        levelTransitionFadeOverlay.DOFade(0, fadeInDuration);
        await Task.Delay((int)(fadeInDuration * 1000));
    }

    async Task FadeOutScreen()
    {
        levelTransitionFadeOverlay.DOFade(1, fadeOutDuration);
        await Task.Delay((int)(fadeOutDuration * 1000));
    }

    IEnumerator LevelNameLifetime()
    {
        yield return new WaitForSeconds(levelTextLifetimeDuration);
        HideLevelName();
    }
}
