using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject mapBackground;
    [SerializeField] MapTile mapTile;
    [SerializeField] Transform mapContainerTransform;
    [SerializeField] TMP_Text currentLevelText;

    //[Header("Drag & Zoom")]
    //[SerializeField] RectTransform mapRootTransform;
    //[SerializeField] float zoomSpeed = 0.1f;
    //[SerializeField] float minZoom = 0.5f;
    //[SerializeField] float maxZoom = 2f;
    //[SerializeField] float dragSpeed = 1f;
    //Vector3 lastMousePos;
    //bool isDragging = false;

    public static bool isMapOpen;

    //void Update()
    //{
    //    if (!isMapOpen)
    //        return;

    //    HandleZoom();
    //    HandleDrag();
    //}

    //void HandleZoom()
    //{
    //    float scroll = Input.GetAxis("Mouse ScrollWheel");
    //    if (Mathf.Approximately(scroll, 0f))
    //        return;

    //    // Determine zoom direction
    //    float zoomDirection = scroll > 0 ? 1f : -1f;

    //    float currentScale = mapRootTransform.localScale.x;
    //    float targetScale = Mathf.Clamp(currentScale + zoomDirection * zoomSpeed, minZoom, maxZoom);

    //    if (Mathf.Approximately(currentScale, targetScale))
    //        return;

    //    float scaleFactor = targetScale / currentScale;

    //    // Determine zoom focus point: cursor (zoom in) or center (zoom out)
    //    Vector2 zoomFocus;
    //    if (zoomDirection > 0)
    //    {
    //        // Zoom toward mouse cursor
    //        RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //            mapRootTransform, Input.mousePosition, null, out zoomFocus);
    //    }
    //    else
    //    {
    //        // Zoom toward center
    //        zoomFocus = Vector2.zero; // Center of RectTransform in local space
    //    }

    //    // Apply scale
    //    mapRootTransform.localScale = Vector3.one * targetScale;

    //    // Adjust position so that zoomFocus point stays stable
    //    Vector2 pivotOffset = (mapRootTransform.anchoredPosition - zoomFocus) * (scaleFactor - 1f);
    //    mapRootTransform.anchoredPosition -= pivotOffset;

    //    ClampMapPosition();
    //}

    //void HandleDrag()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        isDragging = true;
    //        lastMousePos = Input.mousePosition;
    //    }

    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        isDragging = false;
    //    }

    //    if (isDragging)
    //    {
    //        Vector3 delta = Input.mousePosition - lastMousePos;
    //        mapRootTransform.anchoredPosition += new Vector2(delta.x, delta.y) * dragSpeed;
    //        lastMousePos = Input.mousePosition;

    //        ClampMapPosition();
    //    }
    //}

    //void ClampMapPosition()
    //{
    //    RectTransform canvasRect = mapRootTransform.root as RectTransform;
    //    if (canvasRect == null) return;

    //    Vector2 canvasSize = canvasRect.rect.size;
    //    Vector2 mapSize = mapRootTransform.rect.size * mapRootTransform.localScale;

    //    // Clamp boundaries
    //    float clampX = Mathf.Max((mapSize.x - canvasSize.x) / 2f, 0f);
    //    float clampY = Mathf.Max((mapSize.y - canvasSize.y) / 2f, 0f);

    //    Vector2 clampedPos = mapRootTransform.anchoredPosition;
    //    clampedPos.x = Mathf.Clamp(clampedPos.x, -clampX, clampX);
    //    clampedPos.y = Mathf.Clamp(clampedPos.y, -clampY, clampY);

    //    mapRootTransform.anchoredPosition = clampedPos;
    //}

    public void ToggleMap()
    {
        if (PauseMenu.isPaused || UIController.isTransitioningLevel) return;

        if (isMapOpen)
            CloseMap();
        else
            OpenMap();
    }

    public void CloseMap()
    {
        isMapOpen = false;
        mapBackground.SetActive(false);
        HelperFunctions.SetCursorActive(false);

        DestroyMap();
    }

    void OpenMap()
    {
        isMapOpen = true;
        mapBackground.SetActive(true);
        HelperFunctions.SetCursorActive(true);

        GenerateMap();
    }

    void GenerateMap()
    {
        currentLevelText.text = GridController.Instance.GetCurrentLevelName().ToUpper();

        Dictionary<Vector2, GridNode> activeNodes = GridController.Instance.GetCurrentActiveNodes();
        GridNode[] nodes = activeNodes.Values.ToArray();
        Vector2[] coords = activeNodes.Keys.ToArray();

        if (activeNodes.Count == 0)
            return;

        float minX = coords.Min(c => c.y);
        float maxX = coords.Max(c => c.y);
        float minY = coords.Min(c => c.x);
        float maxY = coords.Max(c => c.x);

        float tileSize = 50f;
        float totalWidth = (maxX - minX + 1) * tileSize;
        float totalHeight = (maxY - minY + 1) * tileSize;

        Vector2 centerOffset = new Vector2(totalWidth / 2f, -totalHeight / 2f);

        for (int i = 0; i < activeNodes.Count; i++)
        {
            GridNode node = nodes[i];
            Vector2 coord = coords[i];

            MapTile clone = Instantiate(mapTile, Vector2.zero, Quaternion.identity, mapContainerTransform);
            clone.InitTile(node);

            Vector2 localPos = new Vector2(coord.y * tileSize, coord.x * tileSize);
            localPos -= centerOffset;
            clone.transform.localPosition = localPos;
        }
    }

    void DestroyMap()
    {
        foreach (Transform child in mapContainerTransform)
        {
            Destroy(child.gameObject);
        }

    }
}
