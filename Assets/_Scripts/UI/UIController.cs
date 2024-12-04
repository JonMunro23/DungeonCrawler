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


    [Header("Level Transition")]
    [SerializeField] GameObject levelTransitionParent;
    [SerializeField] TMP_Text levelTransitionText;
    [SerializeField] TMP_Text levelTransitionEnteringText;
    [SerializeField] Image levelTransitionDividingLine;
    [SerializeField] float levelTextLifetimeDuration = 5;
    [SerializeField] Image levelTransitionFadeOverlay;
    [SerializeField] float fadeOutDuration, fadeInDuration;

    PlayerController initialisedPlayer;

    Coroutine levelTextLifetime;

    private void OnEnable()
    {
        PlayerController.onPlayerInitialised += OnPlayerInitialised;
        //PlayerController.fadeInScreen += FadeInScreen;
        //PlayerController.fadeOutScreen += FadeOutScreen;

        ItemPickupManager.onNewItemAttachedToCursor += OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor += OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponAddedToSlot += OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot += OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot += OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault += OnWeaponSetToDefault;

        Weapon.onAmmoUpdated += OnWeaponAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive += OnWeaponSlotSetActive;

        LevelTransition.onLevelTransitionEntered += OnLevelTransitionEntered;
    }

    private void OnDisable()
    {
        PlayerController.onPlayerInitialised -= OnPlayerInitialised;
        //PlayerController.fadeInScreen -= FadeInScreen;
        //PlayerController.fadeOutScreen -= FadeOutScreen;

        ItemPickupManager.onNewItemAttachedToCursor -= OnNewItemAttachedToCursor;
        ItemPickupManager.onCurrentItemDettachedFromCursor -= OnCurrentItemRemovedFromCursor;

        WeaponSlot.onWeaponAddedToSlot -= OnWeaponAddedToSlot;
        WeaponSlot.onWeaponRemovedFromSlot -= OnWeaponRemovedFromSlot;
        WeaponSlot.onWeaponSwappedInSlot -= OnWeaponSwappedInSlot;
        WeaponSlot.onWeaponSetToDefault -= OnWeaponSetToDefault;

        Weapon.onAmmoUpdated -= OnWeaponAmmoUpdated;

        PlayerWeaponManager.onWeaponSlotSetActive -= OnWeaponSlotSetActive;

        LevelTransition.onLevelTransitionEntered -= OnLevelTransitionEntered;
    }

    private void Start()
    {
        levelTransitionFadeOverlay.enabled = true;
    }

    void OnPlayerInitialised(PlayerController playerInitialised)
    {
        initialisedPlayer = playerInitialised;

        playerStatsUIController.InitStatsUI(initialisedPlayer.playerCharacterData);

        FadeInScreen();
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

    void OnWeaponAddedToSlot(int slotIndex, WeaponItemData newItemData, int loadedAmmo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, newItemData);
    }

    void OnWeaponSwappedInSlot(int slotIndex, WeaponItemData dataToSwapTo, int loadedAmmo)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, dataToSwapTo);
    }

    void OnWeaponRemovedFromSlot(int slotIndex)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, null);
    }

    void OnWeaponSetToDefault(int slotIndex, WeaponItemData defaultWeaponData)
    {
        playerWeaponUIManager.UpdateWeaponDisplayImages(slotIndex, defaultWeaponData);
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
