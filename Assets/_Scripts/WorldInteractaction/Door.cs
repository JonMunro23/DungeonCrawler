using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

[SelectionBase]
public class Door : TriggerableBase
{
    [Header("Animation")]
    [SerializeField] Transform transformToMove;
    [SerializeField] Vector3 openedPos, closedPos;
    [SerializeField] float openDuration, closeDuration;

    List<IInteractable> activeInteractables = new List<IInteractable>();

    // Start is called before the first frame update
    void Start()
    {
        transformToMove.localPosition = isTriggered ? openedPos : closedPos;

        if(!isTriggered)
            if (occupyingGridNode)
                occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle));
    }

    public override void Trigger(IInteractable triggeredInteractable)
    {
        switch(triggeredInteractable.GetTriggerOperation())
        {
            case TriggerOperation.Toggle:
                if (activeInteractables.Contains(triggeredInteractable))
                {
                    if (requiredNumOfTriggers > 1)
                    {
                        currentNumOfTriggers--;
                        if (currentNumOfTriggers == requiredNumOfTriggers)
                        {
                            OpenDoor();
                        }
                        else if (currentNumOfTriggers < requiredNumOfTriggers)
                            CloseDoor();
                    }
                    else
                    {
                        ToggleDoor();
                    }

                    activeInteractables.Remove(triggeredInteractable);
                    return;
                }


                activeInteractables.Add(triggeredInteractable);

                if (requiredNumOfTriggers > 1)
                {
                    currentNumOfTriggers++;
                    if (currentNumOfTriggers == requiredNumOfTriggers)
                    {
                        OpenDoor();
                    }
                    else if (currentNumOfTriggers > requiredNumOfTriggers)
                    {
                        CloseDoor();
                    }
                }
                else
                {
                    ToggleDoor();
                }
                break;
            case TriggerOperation.Open:
                OpenDoor();
                break;
            case TriggerOperation.Close:
                CloseDoor();
                break;
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
        //Debug.Log("Opened");

        isTriggered = true;

        if(occupyingGridNode)
            occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.None));

        transformToMove.DOKill();
        transformToMove.DOLocalMove(openedPos, openDuration);
    }

    private void CloseDoor()
    {
        //Debug.Log("Closed");

        isTriggered = false;

        if (occupyingGridNode)
            occupyingGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle));

        transformToMove.DOKill();
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
