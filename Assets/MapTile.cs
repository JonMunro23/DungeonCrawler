using UnityEngine;
using UnityEngine.UI;

public class MapTile : MonoBehaviour
{
    [SerializeField] Image floorImage;
    [SerializeField] Image NorthWallImage;
    [SerializeField] Image EastWallImage;
    [SerializeField] Image SouthWallImage;
    [SerializeField] Image WestWallImage;
    [SerializeField] Image PlayerImage;

    [SerializeField] Sprite floorSprite, voidSprite;

    public void InitTile(GridNode nodeToInit)
    {
        if (!nodeToInit.GetIsExplored())
            return;

        if(nodeToInit.nodeData.isWalkable)
        {
            floorImage.enabled = true;

            if (nodeToInit.GetIsVoid())
            {
                floorImage.sprite = voidSprite;
                return;
            }

            floorImage.sprite = floorSprite;

            if(nodeToInit.GetOccupantType() == GridNodeOccupantType.Player)
            {
                PlayerImage.enabled = true;
            }
        }
        else
        {
            if(nodeToInit.GetNodeInDirection(Vector3.forward))
            {
                if(nodeToInit.GetNodeInDirection(Vector3.forward).nodeData.isWalkable)
                    NorthWallImage.enabled = true;
            }
            if (nodeToInit.GetNodeInDirection(Vector3.left))
            {
                if (nodeToInit.GetNodeInDirection(Vector3.left).nodeData.isWalkable)
                    WestWallImage.enabled = true;
            }
            if (nodeToInit.GetNodeInDirection(Vector3.back))
            {
                if (nodeToInit.GetNodeInDirection(Vector3.back).nodeData.isWalkable)
                    SouthWallImage.enabled = true;
            }
            if (nodeToInit.GetNodeInDirection(Vector3.right))
            {
                if (nodeToInit.GetNodeInDirection(Vector3.right).nodeData.isWalkable)
                    EastWallImage.enabled = true;
            }
        }
    }
}
