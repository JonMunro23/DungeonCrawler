using UnityEngine;
using DG.Tweening;

public class Door : TriggerableBase
{
    [Header("Animation")]
    [SerializeField] Transform transformToMove;
    [SerializeField] Vector3 openedPos, closedPos;
    [SerializeField] float openDuration, closeDuration;

    // Start is called before the first frame update
    void Start()
    {
        transformToMove.localPosition = isTriggered ? openedPos : closedPos;

        if(!isTriggered)
            if (occupyingGridNode)
                occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle));
    }

    public override void Trigger()
    {
        if (requiredNumOfTriggers > 1)
        {
            currentNumOfTriggers++;
            if (currentNumOfTriggers == requiredNumOfTriggers)
            {
                ToggleDoor();

                currentNumOfTriggers = 0;
            }
        }
        else
        {
            ToggleDoor();
        }
        
    }

    public void ToggleDoor()
    {
        if (isTriggered)
            CloseDoor();
        else
            OpenDoor();
    }

    private void OpenDoor()
    {
        isTriggered = true;
        if(occupyingGridNode)
            occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.None));

        transformToMove.DOLocalMove(openedPos, openDuration);
    }

    private void CloseDoor()
    {
        isTriggered = false;

        if (occupyingGridNode)
            occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle));

        transformToMove.DOLocalMove(closedPos, closeDuration);
    }

    public override void SetIsTriggered(bool _isTriggered)
    {
        isTriggered = _isTriggered;

        if(isTriggered)
        {
            transformToMove.localPosition = openedPos;
            if (occupyingGridNode)
                occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.None));
        }
    }

    public override void LoadData(SaveableLevelData.TriggerableSaveData data)
    {
        SetIsTriggered(data.isTriggered);
        if (requiredNumOfTriggers > 0)
            currentNumOfTriggers = data.currentNumberOfTriggers;
    }
}
