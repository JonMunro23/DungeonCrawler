using System.Collections.Generic;
using UnityEngine;

public class ThrowableSelectionManager : MonoBehaviour
{
    [SerializeField] Transform throwableSelectionButtonSpawnParent;
    [SerializeField] ThrowableSelectionButton throwableSelectionButtonPrefab;
    List<ThrowableSelectionButton> spawnedThrowableSelectionButtons = new List<ThrowableSelectionButton>();
    public static bool isThrowableSelectionMenuOpen;
    ThrowableItemData currentlySelectedThrowable;

    private void OnEnable()
    {
        PlayerThrowableManager.onThrowableSelectionMenuOpened += OpenThrowableSelectionMenu;
        PlayerThrowableManager.onThrowableSelectionMenuClosed += CloseThrowableSelectionMenu;

        ThrowableSelectionButton.onThrowableSelected += OnThrowableSelected;

        PauseMenu.onPause += OnPause;
    }

    private void OnDisable()
    {
        PlayerThrowableManager.onThrowableSelectionMenuOpened -= OpenThrowableSelectionMenu;
        PlayerThrowableManager.onThrowableSelectionMenuClosed -= CloseThrowableSelectionMenu;

        ThrowableSelectionButton.onThrowableSelected -= OnThrowableSelected;

        PauseMenu.onPause -= OnPause;
    }

    void OnPause()
    {
        CloseThrowableSelectionMenu();
    }

    public void OpenThrowableSelectionMenu(Dictionary<ThrowableItemData, int> availableThrowables, ThrowableItemData currentlySelectedThrowable) 
    {
        if (PauseMenu.isPaused || CharacterMenuUIController.isCharacterMenuOpen) return;

        this.currentlySelectedThrowable = currentlySelectedThrowable;
        GetHeldThrowableTypes(availableThrowables);
        isThrowableSelectionMenuOpen = true;
    }

    public void CloseThrowableSelectionMenu()
    {
        HelperFunctions.SetCursorActive(false);
        RemoveThrowableSelectionButtons();
        isThrowableSelectionMenuOpen = false;
    }

    public void OnThrowableSelected(ThrowableItemData selectedThrowable, int amountAvailable)
    {
        UpdateSpawnedButtonsInteractability(selectedThrowable);
    }

    private void UpdateSpawnedButtonsInteractability(ThrowableItemData currentlySelectedThrowable)
    {
        foreach (ThrowableSelectionButton button in spawnedThrowableSelectionButtons)
        {
            button.button.interactable = true;

            if (button.availableAmount == 0 || button.throwableItemData == currentlySelectedThrowable)
                button.button.interactable = false;
        }
    }

    void GetHeldThrowableTypes(Dictionary<ThrowableItemData, int> availableThrowables)
    {
        foreach (KeyValuePair<ThrowableItemData, int> availableThrowable in availableThrowables)
        {
            SpawnThrowableSelectionButton(availableThrowable.Key, availableThrowable.Value);
        }
        if (availableThrowables.Count > 0)
            HelperFunctions.SetCursorActive(true);

        UpdateSpawnedButtonsInteractability(currentlySelectedThrowable);
    }

    void SpawnThrowableSelectionButton(ThrowableItemData throwableDataToInitalise, int throwableAmount)
    {
        ThrowableSelectionButton throwableSelectionButton = Instantiate(throwableSelectionButtonPrefab, throwableSelectionButtonSpawnParent);
        throwableSelectionButton.Init(throwableDataToInitalise, throwableAmount);
        if (throwableDataToInitalise == currentlySelectedThrowable)
            throwableSelectionButton.button.interactable = false;
        spawnedThrowableSelectionButtons.Add(throwableSelectionButton);
    }

    void RemoveThrowableSelectionButtons()
    {
        foreach (ThrowableSelectionButton button in spawnedThrowableSelectionButtons)
        {
            Destroy(button.gameObject);
        }
        spawnedThrowableSelectionButtons.Clear();
    }
}
