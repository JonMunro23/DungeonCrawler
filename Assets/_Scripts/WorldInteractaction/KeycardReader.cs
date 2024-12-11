using DG.Tweening;
using System;
using System.Collections;
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

    public override void InteractWithItem(ItemData item)
    {
        if (!canUse)
            return;

        if (isReadingCard)
            return;

        KeyItemData keyData = item as KeyItemData;
        if (!keyData)
            return;

        TryUseKeycard(keyData);
    }

    public override void SetRequiredKeycardType(string requiredType)
    {
        if(Enum.TryParse(requiredType, out KeycardType type))
        {
            requiredKeycard = type;
        }
    }

    void TryUseKeycard(KeyItemData keyData)
    {
        if (keyData.keycardType == requiredKeycard)
        {
            StartCoroutine(ReadCard());
            return;
        }

        StartCoroutine(Error());
    }

    void SetIndicatorMaterial(Material newMat)
    {
        indicatorMesh.material = newMat;
    }

    IEnumerator ReadCard()
    {
        isReadingCard = true;
        card.gameObject.SetActive(true);
        card.DOLocalMoveZ(-0.058f, 1).OnComplete(() =>
        {
            SetIndicatorMaterial(inProgressMat);
            card.DOLocalMoveZ(-0.383f, 1).SetDelay(3).OnComplete(() =>
            {
                card.gameObject.SetActive(false);
            });
        });
        yield return new WaitForSeconds(cardReadingDuration);
        SetIndicatorMaterial(successMat);
        isReadingCard = false;
        TriggerObjects();
    }

    IEnumerator Error()
    {
        SetIndicatorMaterial(errorMat);
        yield return new WaitForSeconds(errorIndicatorDuration);
        SetIndicatorMaterial(defaultMat);
    }

    public override void Interact()
    {
        //somehow show player that a keycard is required
    }

    public override void SetIsActivated(bool _isActivated)
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
