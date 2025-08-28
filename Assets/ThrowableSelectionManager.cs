using System.Collections.Generic;
using UnityEngine;

public class ThrowableSelectionManager : MonoBehaviour
{
    [SerializeField] Transform throwableSelectionButtonSpawnParent;
    [SerializeField] ThrowableSelectionButton throwableSelectionButtonPrefab;
    List<ThrowableSelectionButton> spawnedThrowableSelectionButtons = new List<ThrowableSelectionButton>();

    IInventory playerInventory;
    ThrowableItemData currentlySelectedThrowable;

    private void OnEnable()
    {
        PlayerThrowableManager.OnThrowableSelectionMenuOpened += OpenThrowableSelectionMenu;
        PlayerThrowableManager.OnThrowableSelectionMenuClosed += CloseThrowableSelectionMenu;

        ThrowableSelectionButton.OnThrowableSelected += OnThrowableSelected;


    }

    private void OnDisable()
    {
        PlayerThrowableManager.OnThrowableSelectionMenuOpened -= OpenThrowableSelectionMenu;
        PlayerThrowableManager.OnThrowableSelectionMenuClosed -= CloseThrowableSelectionMenu;

        ThrowableSelectionButton.OnThrowableSelected -= OnThrowableSelected;

    }



    public void OnThrowableSelected(ThrowableItemData throwableTypeSelected)
    {
        foreach (ThrowableSelectionButton button in spawnedThrowableSelectionButtons)
        {
            button.button.interactable = true;
            if (button.throwableItemData == throwableTypeSelected)
            {
                button.button.interactable = false;
            }
        }
    }

    public void OpenThrowableSelectionMenu(IInventory playerInventory, ThrowableItemData currentlySelectedThrowable)
    {
        this.playerInventory = playerInventory;
        this.currentlySelectedThrowable = currentlySelectedThrowable;
        GetHeldThrowableTypes();
    }

    public void CloseThrowableSelectionMenu()
    {
        HelperFunctions.SetCursorActive(false);
        RemoveThrowableSelectionButtons();
    }

    void GetHeldThrowableTypes()
    {
        List<ThrowableItemData> availableThrowableTypes = playerInventory.GetAllAvailableThrowables();
        foreach (ThrowableItemData throwableData in availableThrowableTypes)
        {
            SpawnThrowableSelectionButton(throwableData);
        }
        if (availableThrowableTypes.Count > 0)
            HelperFunctions.SetCursorActive(true);

        if (currentlySelectedThrowable == null) return;
        foreach (ThrowableSelectionButton button in spawnedThrowableSelectionButtons)
        {
            if (button.throwableItemData == currentlySelectedThrowable)
            {
                button.button.interactable = false;
            }
        }
    }

    void SpawnThrowableSelectionButton(ThrowableItemData throwableDataToInitalise)
    {
        ThrowableSelectionButton throwableSelectionButton = Instantiate(throwableSelectionButtonPrefab, throwableSelectionButtonSpawnParent);
        throwableSelectionButton.Init(throwableDataToInitalise);
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
