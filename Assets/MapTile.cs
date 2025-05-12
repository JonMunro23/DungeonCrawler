using UnityEngine;
using UnityEngine.UI;

public class MapTile : MonoBehaviour
{
    [SerializeField] Image floorImage;
    [SerializeField] Image NorthWallImage;
    [SerializeField] Image EastWallImage;
    [SerializeField] Image SouthWallImage;
    [SerializeField] Image WestWallImage;

    [SerializeField] Image NorthEastCornerImage;
    [SerializeField] Image SouthEastCornerImage;
    [SerializeField] Image SouthWestCornerImage;
    [SerializeField] Image NorthWestCornerImage;

    [SerializeField] Image PlayerIcon;
    [SerializeField] Image LevelTransitionIcon;
    [SerializeField] Image PressurePlateIcon;

    [SerializeField] Sprite floorSprite, voidSprite;

    public void InitTile(GridNode nodeToInit)
    {
        if (!nodeToInit.GetIsExplored())
            return;

        if(nodeToInit.nodeData.isWalkable)
        {
            floorImage.enabled = true;

            CheckForSurroundingWalls(nodeToInit);

            if (nodeToInit.GetIsVoid())
            {
                floorImage.sprite = voidSprite;
                return;
            }

            floorImage.sprite = floorSprite;

            switch (nodeToInit.GetOccupantType())
            {
                case GridNodeOccupantType.Player:
                    PlayerIcon.enabled = true;
                    UpdateIconFacingDirection(PlayerIcon, nodeToInit.GetOccupyingGameobject().GetComponent<PlayerController>().advGridMovement.GetTargetRot());
                    break;
                case GridNodeOccupantType.LevelTransition:
                    LevelTransitionIcon.enabled = true;
                    UpdateIconFacingDirection(LevelTransitionIcon, nodeToInit.GetOccupyingGameobject().transform.localRotation.y);
                    break;
            }
        }
    }

    void CheckForSurroundingWalls(GridNode nodeToCheck)
    {
        bool hasNWall = false, hasEWall = false, hasSWall = false, hasWWall = false;
        if (nodeToCheck.GetNodeInDirection(Vector3.forward))
        {
            if (!nodeToCheck.GetNodeInDirection(Vector3.forward).nodeData.isWalkable)
            {
                NorthWallImage.enabled = true;
                hasNWall = true;
            }
        }
        if (nodeToCheck.GetNodeInDirection(Vector3.left))
        {
            if (!nodeToCheck.GetNodeInDirection(Vector3.left).nodeData.isWalkable)
            {
                WestWallImage.enabled = true;
                hasWWall = true;
            }
        }
        if (nodeToCheck.GetNodeInDirection(Vector3.back))
        {
            if (!nodeToCheck.GetNodeInDirection(Vector3.back).nodeData.isWalkable)
            {
                SouthWallImage.enabled = true;
                hasSWall = true;
            }
        }
        if (nodeToCheck.GetNodeInDirection(Vector3.right))
        {
            if (!nodeToCheck.GetNodeInDirection(Vector3.right).nodeData.isWalkable)
            {
                EastWallImage.enabled = true;
                hasEWall = true;
            }
        }

        if(hasNWall)
        {
            if(hasEWall)
                NorthEastCornerImage.enabled = true;

            if(hasWWall)
                NorthWestCornerImage.enabled = true;
        }

        if(hasSWall)
        {
            if(hasEWall)
                SouthEastCornerImage.enabled = true;

            if(hasWWall)
                SouthWestCornerImage.enabled = true;
        }
    }

    void UpdateIconFacingDirection(Image icon, float targetDir)
    {
        icon.transform.Rotate(new Vector3(0, 0, -targetDir));
    }
}
