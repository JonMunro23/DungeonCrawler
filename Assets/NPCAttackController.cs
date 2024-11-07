using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAttackController : MonoBehaviour
{
    NPCGroupController groupController;

    public void Init(NPCGroupController newGroupController)
    {
        groupController = newGroupController;
    }

    // Update is called once per frame
    void Update()
    {
        var yeet = GridController.Instance.GetNodeInDirection(groupController.currentlyOccupiedGridnode, groupController.movementController.currentOrientation.forward);
        if (!yeet)
            return;

        if (yeet.currentOccupant == GridNodeOccupant.Player)
        {
            groupController.animController.PlayAnimation("Attack");
        }
    }
}
