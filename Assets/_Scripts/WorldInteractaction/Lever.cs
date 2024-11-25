using DG.Tweening;
using UnityEngine;

public class Lever : InteractableBase
{
    [Space]
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

        if(objectsToTrigger.Count > 0 )
        {
            foreach (ITriggerable item in objectsToTrigger)
            {
                if (!item.IsTriggerable())
                    return;

                item.Trigger();
            }
        }


        if (isSingleUse)
            canUse = false;
    }

    private void FlipLever()
    {
        if(isActivated)
            leverPivotPoint.DOLocalRotate(unflippedRotation, flipDuration);
        else
            leverPivotPoint.DOLocalRotate(flippedRotation, flipDuration);
    }
}
