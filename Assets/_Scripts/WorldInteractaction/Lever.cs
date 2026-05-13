using DG.Tweening;
using UnityEngine;

[SelectionBase]
public class Lever : InteractableBase
{
    [Header("Animation")]
    [SerializeField] Transform leverPivotPoint;
    [SerializeField] Vector3 flippedRotation, unflippedRotation;
    [SerializeField] float flipDuration;

    public override void Interact()
    {
        TryFlipLever();
    }
    public override bool TryInteractWithItem(ItemData item)
    {
        TryFlipLever();

        return true;
    }

    private void TryFlipLever()
    {
        if (!canUse)
            return;

        FlipLever();

        TriggerObjects();
    }

    private void FlipLever()
    {
        if(isActivated)
        {
            //isActivated = false;
            leverPivotPoint.DOLocalRotate(unflippedRotation, flipDuration);
        }
        else
        {
            //isActivated = true;
            leverPivotPoint.DOLocalRotate(flippedRotation, flipDuration);
        }
    }

    public override void SetStartingActivationState(bool _isActivated)
    {
        isActivated = _isActivated;

        FlipLever();
    }
}
