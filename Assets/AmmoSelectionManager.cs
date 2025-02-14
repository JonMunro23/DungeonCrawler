using System.Collections.Generic;
using UnityEngine;

public class AmmoSelectionManager : MonoBehaviour
{
    [SerializeField] Transform ammoSelectionButtonSpawnParent;
    [SerializeField] AmmoSelectionButton ammoSelectionButtonPrefab;
    List<AmmoSelectionButton> spawnedAmmoSelectionButtons = new List<AmmoSelectionButton>();

    private void OnEnable()
    {
        PlayerWeaponManager.onWeaponAmmoSelectionMenuOpened += OpenAmmoSelectionMenu;
        PlayerWeaponManager.onWeaponAmmoSelectionMenuClosed += CloseAmmoSelectionMenu;
    }

    private void OnDisable()
    {
        PlayerWeaponManager.onWeaponAmmoSelectionMenuOpened -= OpenAmmoSelectionMenu;
        PlayerWeaponManager.onWeaponAmmoSelectionMenuClosed -= CloseAmmoSelectionMenu;
    }

    public void OpenAmmoSelectionMenu(IWeapon currentHeldWeapon)
    {
        HelperFunctions.SetCursorActive(true);
        GetHeldAmmoTypesForWeapon(currentHeldWeapon);
    }

    public void CloseAmmoSelectionMenu()
    {
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
    }

    void SpawnAmmoSelectionButton(AmmoItemData ammoItemDataToInitialise)
    {
        AmmoSelectionButton ammoSelectionButton = Instantiate(ammoSelectionButtonPrefab, ammoSelectionButtonSpawnParent);
        ammoSelectionButton.Init(ammoItemDataToInitialise);
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
