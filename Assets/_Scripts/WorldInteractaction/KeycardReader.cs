using DG.Tweening;
using System.Collections;
using System.Security.Policy;
using UnityEngine;

public enum KeycardType
{
    Red,
    Blue,
    Green,
    Yellow
}

public class KeycardReader : InteractableBase
{
    [SerializeField] KeycardType requiredKeycard;
    bool isReadingCard;
    [SerializeField] float cardReadingDuration, errorIndicatorDuration;
    [SerializeField] MeshRenderer indicatorMesh;
    [SerializeField] Material successMat, errorMat, inProgressMat, defaultMat;

    [Header("Card Animation")]
    [SerializeField] Transform card;

    private void Start()
    {
        defaultMat = indicatorMesh.material;
    }

    public override bool RequiresItem()
    {
        return true;
    }

    public override bool TryInteractWithItem(ItemData item)
    {
        if (!canUse)
            return false;

        if (isReadingCard)
            return false;

        if(item == null)
            return false;

        KeycardItemData keycardData = item as KeycardItemData;
        if (!keycardData)
            return false;

        return TryUseKeycard(keycardData);
    }

    public void SetRequiredKeycardType(string requiredType)
    {
        requiredKeycard = HelperFunctions.ToEnum<KeycardType>(requiredType);
    }

    bool TryUseKeycard(KeycardItemData keyData)
    {
        if (keyData.keycardType == requiredKeycard)
        {
            StartCoroutine(ReadCard());
            return true;
        }

        StartCoroutine(Error());
        return false;
    }

    void SetIndicatorMaterial(Material newMat)
    {
        indicatorMesh.material = newMat;
    }

    IEnumerator ReadCard()
    {
        isReadingCard = true;
        card.gameObject.SetActive(true);
        card.DOLocalMoveZ(-0.058f, .2f).OnComplete(() =>
        {
            SetIndicatorMaterial(inProgressMat);
        });

        yield return new WaitForSeconds(cardReadingDuration);

        SetIndicatorMaterial(successMat);
        isReadingCard = false;
        TriggerObjects();

        card.DOLocalMoveZ(-0.383f, .2f).SetDelay(.2f).OnComplete(() =>
        {
            card.gameObject.SetActive(false);
        });
    }

    IEnumerator Error()
    {
        SetIndicatorMaterial(errorMat);
        yield return new WaitForSeconds(errorIndicatorDuration);
        SetIndicatorMaterial(defaultMat);
    }

    public override void SetStartingActivationState(bool _isActivated)
    {
        isActivated = _isActivated;

        if(isActivated)
        {
            if (isSingleUse)
                canUse = false;

            SetIndicatorMaterial(successMat);
        }
    }
}
