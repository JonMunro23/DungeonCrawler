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
    public override void InteractWithItem(ItemData item)
    {
        TryFlipLever();
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

    public override void SetIsActivated(bool _isActivated)
    {
        isActivated = _isActivated;

        if(isActivated)
        {
            if (isSingleUse)
                canUse = false;

            leverPivotPoint.localRotation = Quaternion.Euler(flippedRotation);
        }
    }

    public override void SetTriggerOnExit(bool triggerOnExit)
    {
    }

    public override bool GetTriggerOnExit()
    {
        throw new System.NotImplementedException();
    }
}
