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
        isTriggered = true;

        if(occupyingGridNode)
        {
            GridNodeOccupant newOccupant = new GridNodeOccupant(gameObject, GridNodeOccupantType.None);
            occupyingGridNode.SetBaseOccupant(newOccupant);
            occupyingGridNode.SetOccupant(newOccupant);
        }

        transformToMove.DOKill();
        transformToMove.DOLocalMove(openedPos, openDuration);
    }

    private void CloseDoor()
    {
        isTriggered = false;

        if (occupyingGridNode)
        {
            GridNodeOccupant newOccupant = new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle);
            occupyingGridNode.SetBaseOccupant(newOccupant);
            occupyingGridNode.SetOccupant(newOccupant);
        }

        transformToMove.DOKill();
        transformToMove.DOLocalMove(closedPos, closeDuration);
    }

    public override void SetIsTriggered(bool _isTriggered)
    {
        isTriggered = _isTriggered;
        transformToMove.localPosition = isTriggered ? openedPos : closedPos;
        if(isTriggered)
        {
            if (occupyingGridNode)
            {
                GridNodeOccupant newOccupant = new GridNodeOccupant(gameObject, GridNodeOccupantType.None);
                occupyingGridNode.SetOccupant(newOccupant);
                occupyingGridNode.SetBaseOccupant(newOccupant);
            }
        }
        else
        {
            if (occupyingGridNode)
            {
                GridNodeOccupant newOccupant = new GridNodeOccupant(gameObject, GridNodeOccupantType.Obstacle);
                occupyingGridNode.SetOccupant(newOccupant);
                occupyingGridNode.SetBaseOccupant(newOccupant);
            }
        }
    }

    public override void LoadData(SaveableLevelData.TriggerableSaveData data)
    {
        SetIsTriggered(data.isTriggered);
        if (requiredNumOfTriggers > 0)
            currentNumOfTriggers = data.currentNumberOfTriggers;
    }
}
