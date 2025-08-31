using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerThrowableUIController : MonoBehaviour
{
    [SerializeField] GameObject throwableContainer;
    [SerializeField] Image throwableImage;
    [SerializeField] TMP_Text throwableAmountText;

    private void OnEnable()
    {
        ThrowableSelectionButton.onThrowableSelected += OnThrowableSelected;

        PlayerInventoryManager.onFirstThrowableCollected += OnFirstThrowableCollected;
        PlayerInventoryManager.onThrowableRemoved += OnThrowableRemoved;

    }

    private void OnDisable()
    {
        ThrowableSelectionButton.onThrowableSelected -= OnThrowableSelected;

        PlayerInventoryManager.onFirstThrowableCollected -= OnFirstThrowableCollected;
        PlayerInventoryManager.onThrowableRemoved -= OnThrowableRemoved;
    }

    void OnFirstThrowableCollected(ThrowableItemData firstThrowable)
    {
        throwableContainer.SetActive(true);
        UpdateThrowableUI(firstThrowable);
    }

    void OnThrowableSelected(ThrowableItemData throwableData)
    {
        UpdateThrowableUI(throwableData);
    }

    void OnThrowableRemoved(ThrowableItemData removedThrowable)
    {
        UpdateThrowableUI(removedThrowable);
    }

    private void UpdateThrowableUI(ThrowableItemData throwableData)
    {
        throwableImage.sprite = throwableData.itemSprite;
        throwableAmountText.text = PlayerInventoryManager.GetRemainingAmountOfItem(throwableData).ToString();
    }
}
