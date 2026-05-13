
using DG.Tweening;
using UnityEngine;

public class Button : InteractableBase
{
    [Header("Button Animation")]
    [SerializeField] Transform transformToAnimate;
    [SerializeField] float startingZPos, pressedZPos, pushDuration;
    public override void Interact()
    {
        PushButton();
    }

    public override bool TryInteractWithItem(ItemData item)
    {
        PushButton();

        return true;
    }

    void PushButton()
    {
        if (!canUse)
            return;

        PushAnimation();
    }

    public override void SetStartingActivationState(bool activatedState)
    {
        isActivated = activatedState;
    }

    void PushAnimation()
    {
        transformToAnimate.DOLocalMoveZ(pressedZPos, pushDuration).OnComplete(() =>
        {
            TriggerObjects();

            if(canUse)
                transformToAnimate.DOLocalMoveZ(startingZPos, pushDuration).SetDelay(.1f);
        });
    }
}
