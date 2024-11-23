using DG.Tweening;
using System;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] bool isSingleUse;
    bool canUse = true;
    bool isFlipped = false;

    public GameObject[] objectsToTrigger;
    [Space]
    [Header("Animation")]
    [SerializeField] Transform leverPivotPoint;
    [SerializeField] Vector3 flippedRotation, unflippedRotation;
    [SerializeField] float flipDuration;
    public void Interact()
    {
        TryFlipLever();
    }
    public void InteractWithItem(ItemData item)
    {
        TryFlipLever();
    }

    private void TryFlipLever()
    {
        if (!canUse)
            return;

        FlipLever();

        foreach (GameObject item in objectsToTrigger)
        {
            if (!item.TryGetComponent(out ITriggerable triggerable))
                return;

            if (!triggerable.IsTriggerable())
                return;

            triggerable.Trigger();
        }

        if (isSingleUse)
            canUse = false;
    }

    private void FlipLever()
    {
        if(isFlipped)
            leverPivotPoint.DOLocalRotate(unflippedRotation, flipDuration);
        else
            leverPivotPoint.DOLocalRotate(flippedRotation, flipDuration);
    }

}
