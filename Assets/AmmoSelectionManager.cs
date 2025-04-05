using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AmmoSelectionManager : MonoBehaviour
{
    [SerializeField] Transform ammoSelectionButtonSpawnParent;
    [SerializeField] AmmoSelectionButton ammoSelectionButtonPrefab;
    List<AmmoSelectionButton> spawnedAmmoSelectionButtons = new List<AmmoSelectionButton>();

    IWeapon currentHeldWeapon;

    private void OnEnable()
    {
        PlayerWeaponManager.onWeaponAmmoSelectionMenuOpened += OpenAmmoSelectionMenu;
        PlayerWeaponManager.onWeaponAmmoSelectionMenuClosed += CloseAmmoSelectionMenu;

        AmmoSelectionButton.OnAmmoSelected += OnAmmoSelected;
    }

    private void OnDisable()
    {
        PlayerWeaponManager.onWeaponAmmoSelectionMenuOpened -= OpenAmmoSelectionMenu;
        PlayerWeaponManager.onWeaponAmmoSelectionMenuClosed -= CloseAmmoSelectionMenu;

        AmmoSelectionButton.OnAmmoSelected -= OnAmmoSelected;
    }

    public void OnAmmoSelected(AmmoItemData ammoTypeSelected)
    {
        foreach (AmmoSelectionButton button in spawnedAmmoSelectionButtons)
        {
            button.button.interactable = true;
            if (button.ammoItemData == ammoTypeSelected)
            {
                button.button.interactable = false;
            }
        }
    }

    public void OpenAmmoSelectionMenu(IWeapon currentHeldWeapon)
    {
        this.currentHeldWeapon = currentHeldWeapon;

        HelperFunctions.SetCursorActive(true);
        if(!currentHeldWeapon.IsMeleeWeapon())
            GetHeldAmmoTypesForWeapon(currentHeldWeapon);
    }

    public void CloseAmmoSelectionMenu()
    {
        if(!CharacterMenuUIController.isCharacterMenuOpen)
            HelperFunctions.SetCursorActive(false);
        RemovedAmmoSelectionButtons();
    }

    void GetHeldAmmoTypesForWeapon(IWeapon weaponToCheck)
    {
        List<AmmoItemData> availableAmmoTypes = weaponToCheck.GetRangedWeapon().GetAllUseableHeldAmmo();
        foreach (AmmoItemData ammoData in availableAmmoTypes)
        {
            SpawnAmmoSelectionButton(ammoData);
        }

        foreach (AmmoSelectionButton button in spawnedAmmoSelectionButtons)
        {
            if(button.ammoItemData == weaponToCheck.GetRangedWeapon().GetCurrentLoadedAmmoData())
            {
                button.button.interactable = false;
            }
        }
    }

    void SpawnAmmoSelectionButton(AmmoItemData ammoItemDataToInitialise)
    {
        AmmoSelectionButton ammoSelectionButton = Instantiate(ammoSelectionButtonPrefab, ammoSelectionButtonSpawnParent);
        ammoSelectionButton.Init(ammoItemDataToInitialise, currentHeldWeapon);
        spawnedAmmoSelectionButtons.Add(ammoSelectionButton);
    }

    void RemovedAmmoSelectionButtons()
    {
        foreach (AmmoSelectionButton button in spawnedAmmoSelectionButtons)
        {
            Destroy(button.gameObject);
        }
        spawnedAmmoSelectionButtons.Clear();
    }
}
