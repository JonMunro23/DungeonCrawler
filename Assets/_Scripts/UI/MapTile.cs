using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapTile : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Floors")]
    [SerializeField] Image floorImage;
    [SerializeField] Sprite floorSprite, voidSprite;

    [Header("Walls")]
    [SerializeField] Image NorthWallImage;
    [SerializeField] Image EastWallImage;
    [SerializeField] Image SouthWallImage;
    [SerializeField] Image WestWallImage;

    [Header("Corners")]
    [SerializeField] Image NorthEastCornerImage;
    [SerializeField] Image SouthEastCornerImage;
    [SerializeField] Image SouthWestCornerImage;
    [SerializeField] Image NorthWestCornerImage;

    [Header("Icons")]
    [SerializeField] Image PinIcon;
    [SerializeField] Image PlayerIcon;
    [SerializeField] Image LevelTransitionIcon;
    [SerializeField] Image PressurePlateIcon;

    [Header("Map Pin")]
    [SerializeField] TMP_InputField pinTextInputField;
    bool hasPinPlaced;

    Vector2 tileCoords;


    public void InitTile(GridNode nodeToInit)
    {

        tileCoords = new Vector2(nodeToInit.Coords.Pos.y, -nodeToInit.Coords.Pos.x);

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
            Debug.Log(nodeToInit.GetOccupantType());
            switch (nodeToInit.GetOccupantType())
            {
                case GridNodeOccupantType.Player:
                    PlayerIcon.enabled = true;
                    UpdateIconFacingDirection(PlayerIcon, nodeToInit.GetOccupyingGameobject().GetComponent<PlayerController>().advGridMovement.GetTargetRot());
                    break;
                case GridNodeOccupantType.LevelTransition:
                    LevelTransitionIcon.enabled = true;
                    UpdateIconFacingDirection(LevelTransitionIcon, nodeToInit.GetOccupyingGameobject().transform.rotation.eulerAngles.y);
                    break;
                case GridNodeOccupantType.PressurePlate:
                    PressurePlateIcon.enabled = true;
                    break;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Right:
                RemovePin();
                break;
            case PointerEventData.InputButton.Left:
                PlacePin();
                break;
        }
    }

    public void PlacePin()
    {
        if (hasPinPlaced)
        {
            pinTextInputField.ActivateInputField();
            return;
        }

        PinIcon.enabled = true;
        hasPinPlaced = true;

        pinTextInputField.gameObject.SetActive(true);
        pinTextInputField.text = $"X: {tileCoords.x}, Y: {tileCoords.y}";
        pinTextInputField.ActivateInputField();
    }

    void RemovePin()
    {
        if (!hasPinPlaced) return;

        pinTextInputField.text = "";
        pinTextInputField.gameObject.SetActive(false);

        PinIcon.enabled = false;
        hasPinPlaced = false;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!hasPinPlaced) return;

        pinTextInputField.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!hasPinPlaced) return;

        pinTextInputField.gameObject.SetActive(false);
    }
}
