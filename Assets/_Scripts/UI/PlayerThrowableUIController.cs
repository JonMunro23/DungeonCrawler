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

        PlayerThrowableManager.onFirstThrowableCollected += OnFirstThrowableCollected;

        PlayerThrowableManager.onCurrentlySelectedThrowableAmountUpdated += OnCurrentlySelectedThrowableAmountUpdated;

    }

    private void OnDisable()
    {
        ThrowableSelectionButton.onThrowableSelected -= OnThrowableSelected;

        PlayerThrowableManager.onFirstThrowableCollected -= OnFirstThrowableCollected;

        PlayerThrowableManager.onCurrentlySelectedThrowableAmountUpdated -= OnCurrentlySelectedThrowableAmountUpdated;

    }

    void OnFirstThrowableCollected(ThrowableItemData firstThrowable, int throwableAmount)
    {
        throwableContainer.SetActive(true);
        UpdateThrowableUI(firstThrowable);
    }

    void OnThrowableSelected(ThrowableItemData throwableData, int throwableAmount)
    {
        UpdateThrowableUI(throwableData);
        UpdateThrowableAmount(throwableAmount);
    }

    void OnCurrentlySelectedThrowableAmountUpdated(int newAmount)
    {
        UpdateThrowableAmount(newAmount);
    }

    private void UpdateThrowableUI(ThrowableItemData throwableData)
    {
        throwableImage.sprite = throwableData.itemSprite;
    }

    void UpdateThrowableAmount(int newAmount)
    {
        throwableAmountText.text = newAmount.ToString();
    }
}
