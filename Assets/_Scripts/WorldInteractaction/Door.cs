using UnityEngine;
using DG.Tweening;

public class Door : TriggerableBase
{
    bool isOpen;
    
    [Space]
    [Header("Animation")]
    [SerializeField] Transform transformToMove;
    [SerializeField] Vector3 openedPos, closedPos;
    [SerializeField] float openDuration, closeDuration;

    // Start is called before the first frame update
    void Start()
    {
        transformToMove.localPosition = isOpen ? openedPos : closedPos;
        if(!isOpen)
            if (occupyingGridNode)
                occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle));
    }

    public override void Trigger()
    {
        base.Trigger();
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        if(requiredNumOfTriggers > 1)
        {
            currentNumOfTriggers++;
            if(currentNumOfTriggers == requiredNumOfTriggers)
            {
                if (isOpen)
                    CloseDoor();
                else
                    OpenDoor();

                currentNumOfTriggers = 0;
            }
        }
        else
        {
            if (isOpen)
                CloseDoor();
            else
                OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        if(occupyingGridNode)
            occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.None));
        transformToMove.DOLocalMove(openedPos, openDuration);
    }

    private void CloseDoor()
    {
        isOpen = false;
        if (occupyingGridNode)
            occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle));
        transformToMove.DOLocalMove(closedPos, closeDuration);
    }


}
