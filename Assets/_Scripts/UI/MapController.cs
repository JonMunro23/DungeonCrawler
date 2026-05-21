using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject mapBackground;
    [SerializeField] MapTile mapTilePrefab;
    [SerializeField] TMP_Text currentLevelText;

    [Header("Pan / Zoom")]
    [SerializeField] float minZoom = 0.5f;
    [SerializeField] float maxZoom = 2.5f;
    [SerializeField] float defaultZoom = .75f;
    [SerializeField] float zoomSpeed = 0.15f;
    [SerializeField] float panSpeed = 1f;
    [SerializeField] bool resetMapViewOnOpen = true;

    [Header("Clamp Settings")]
    [SerializeField] bool clampPanning = true;
    [SerializeField] bool useViewportBasedClampPadding = true;
    [SerializeField] Vector2 viewportClampPaddingMultiplier = new Vector2(0.5f, 0.5f);
    [SerializeField] Vector2 clampPadding = Vector2.zero;
    [SerializeField] Vector2 clampOffset = Vector2.zero;
    [SerializeField] bool allowHorizontalPanWhenMapSmallerThanView;
    [SerializeField] bool allowVerticalPanWhenMapSmallerThanView;
    [SerializeField] float fallbackHorizontalPanRange = 100f;
    [SerializeField] float fallbackVerticalPanRange = 100f;

    [Header("Debug")]
    [SerializeField] bool showPanClampDebug;
    [SerializeField] Color panClampDebugColor = Color.green;
    [SerializeField] Color currentMapPositionDebugColor = Color.red;
    [SerializeField] float debugSphereSize = 10f;
    [SerializeField] float debugCenterCrossSize = 15f;

    List<GameObject> mapParents = new List<GameObject>();
    Dictionary<int, List<MapTile>> generatedMaps = new Dictionary<int, List<MapTile>>();
    Dictionary<int, Vector2> mapSizes = new Dictionary<int, Vector2>();
    int currentLevelIndex;
    float currentZoom = 1f;
    bool isPanning;
    Vector2 lastMousePosition;
    PlayerControls controls;
    public static bool isMapOpen;

    private void OnEnable()
    {
        GridController.OnLevelNodesGenerated += OnLevelGenerated;
    }

    private void OnDisable()
    {
        GridController.OnLevelNodesGenerated -= OnLevelGenerated;
    }

    public void Init(PlayerControls controls)
    {
        this.controls = controls;
        ResetMapView();
    }

    void OnLevelGenerated(LevelData levelData)
    {
        GenerateMap(levelData);
        currentLevelText.transform.SetAsLastSibling();
    }

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

        HideMap(currentLevelIndex);
    }

    void OpenMap()
    {
        isMapOpen = true;
        mapBackground.SetActive(true);
        HelperFunctions.SetCursorActive(true);
        currentLevelIndex = GridController.Instance.GetCurrentLevelIndex(); //change this to only update on level transition
        ShowMap(currentLevelIndex);

        if (resetMapViewOnOpen)
            ResetMapView();
    }

    void GenerateMap(LevelData levelData)
    {
        List<GridNode> levelNodes = levelData.GetNodes();
        int levelIndex = levelData.LevelIndex;
        GridNode[] nodes = levelNodes.ToArray();
        Vector2Int[] coords = new Vector2Int[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            coords[i] = nodes[i].Coords.Pos;
        }

        if (levelNodes.Count == 0)
            return;

        GameObject mapParent = new GameObject($"Level{levelIndex} Map");
        mapParent.transform.SetParent(mapBackground.transform, false);
        mapParent.transform.localPosition = Vector3.zero;
        mapParent.transform.localScale = Vector3.one;
        mapParents.Add(mapParent);

        float minX = coords.Min(c => c.y);
        float maxX = coords.Max(c => c.y);
        float minY = coords.Min(c => c.x);
        float maxY = coords.Max(c => c.x);

        float tileSize = 50f;
        float totalWidth = (maxX - minX + 1) * tileSize;
        float totalHeight = (maxY - minY + 1) * tileSize;

        mapSizes[levelIndex] = new Vector2(totalWidth, totalHeight);

        Vector2 centerOffset = new Vector2(totalWidth / 2f, -totalHeight / 2f);

        List<MapTile> map = new List<MapTile>();

        for (int i = 0; i < nodes.Length; i++)
        {
            GridNode node = nodes[i];
            Vector2Int coord = coords[i];

            MapTile clone = Instantiate(mapTilePrefab, Vector2.zero, Quaternion.identity, mapBackground.transform);
            clone.InitTile(node);

            Vector2 localPos = new Vector2(coord.y * tileSize, coord.x * tileSize);
            localPos -= centerOffset;
            clone.transform.localPosition = localPos;

            clone.transform.SetParent(mapParent.transform, true);

            map.Add(clone);
        }

        generatedMaps.TryAdd(levelIndex, map);

        HideMap(levelIndex);
    }

    void ShowMap(int levelIndex)
    {
        currentLevelText.text = GridController.Instance.GetCurrentLevelName().ToUpper();

        mapParents[levelIndex].gameObject.SetActive(true);

        if (generatedMaps.TryGetValue(levelIndex, out var map))
        {
            foreach (MapTile mapTile in map)
            {
                mapTile.RefreshTile();
            }
        }
    }

    void HideMap(int levelIndex)
    {
        mapParents[levelIndex].gameObject.SetActive(false);

        currentLevelText.text = "";
    }

    public void HandleZoom()
    {
        if (!isMapOpen) return;

        float scroll = controls.UIControls.Scroll.ReadValue<Vector2>().y;

        if (Mathf.Abs(scroll) <= 0.01f)
            return;

        Transform activeMap = GetActiveMapTransform();

        if (activeMap == null)
            return;

        RectTransform mapBackgroundRect = mapBackground.GetComponent<RectTransform>();

        if (mapBackgroundRect == null)
            return;

        Vector2 mousePosition = controls.Player.MousePos.ReadValue<Vector2>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapBackgroundRect,
            mousePosition,
            null,
            out Vector2 mouseLocalPosition
        );

        float oldZoom = currentZoom;
        float newZoom = Mathf.Clamp(currentZoom + Mathf.Sign(scroll) * zoomSpeed, minZoom, maxZoom);

        if (Mathf.Approximately(oldZoom, newZoom))
            return;

        Vector3 mapPosition = activeMap.localPosition;

        Vector2 mouseToMapBeforeZoom = mouseLocalPosition - new Vector2(mapPosition.x, mapPosition.y);
        float zoomRatio = newZoom / oldZoom;
        Vector2 mouseToMapAfterZoom = mouseToMapBeforeZoom * zoomRatio;

        Vector2 positionOffset = mouseToMapBeforeZoom - mouseToMapAfterZoom;

        currentZoom = newZoom;

        activeMap.localScale = Vector3.one * currentZoom;
        activeMap.localPosition += new Vector3(positionOffset.x, positionOffset.y, 0f);

        ClampMapPosition();
    }

    public void HandlePanning()
    {
        if (!isMapOpen) return;

        Transform activeMap = GetActiveMapTransform();

        if (activeMap == null)
            return;

        Vector2 mousePosition = controls.Player.MousePos.ReadValue<Vector2>();

        if (controls.Player.LeftClick.WasPressedThisFrame())
        {
            isPanning = true;
            SetCursorVisible(!isPanning);
            lastMousePosition = mousePosition;
        }

        if (controls.Player.LeftClick.WasReleasedThisFrame())
        {
            isPanning = false;
            SetCursorVisible(!isPanning);
        }

        if (!isPanning)
            return;

        Vector2 mouseDelta = mousePosition - lastMousePosition;
        lastMousePosition = mousePosition;

        activeMap.localPosition += new Vector3(mouseDelta.x, mouseDelta.y, 0f) * panSpeed;

        ClampMapPosition();
    }

    private void SetCursorVisible(bool isVisible)
    {
        Cursor.visible = isVisible;
    }

    void ClampMapPosition()
    {
        if (!clampPanning)
            return;

        Transform activeMap = GetActiveMapTransform();

        if (activeMap == null)
            return;

        RectTransform mapBackgroundRect = mapBackground.GetComponent<RectTransform>();

        if (mapBackgroundRect == null)
            return;

        if (!mapSizes.TryGetValue(currentLevelIndex, out Vector2 mapSize))
            return;

        Vector2 viewportSize = mapBackgroundRect.rect.size;

        float scaledMapWidth = mapSize.x * currentZoom;
        float scaledMapHeight = mapSize.y * currentZoom;

        float maxX = (scaledMapWidth - viewportSize.x) / 2f;
        float maxY = (scaledMapHeight - viewportSize.y) / 2f;

        if (maxX < 0f)
        {
            if (allowHorizontalPanWhenMapSmallerThanView)
                maxX = fallbackHorizontalPanRange;
            else
                maxX = 0f;
        }

        if (maxY < 0f)
        {
            if (allowVerticalPanWhenMapSmallerThanView)
                maxY = fallbackVerticalPanRange;
            else
                maxY = 0f;
        }

        if (useViewportBasedClampPadding)
        {
            maxX += viewportSize.x * viewportClampPaddingMultiplier.x;
            maxY += viewportSize.y * viewportClampPaddingMultiplier.y;
        }
        else
        {
            maxX += clampPadding.x;
            maxY += clampPadding.y;
        }

        Vector3 pos = activeMap.localPosition;

        pos.x = Mathf.Clamp(pos.x, -maxX + clampOffset.x, maxX + clampOffset.x);
        pos.y = Mathf.Clamp(pos.y, -maxY + clampOffset.y, maxY + clampOffset.y);
        pos.z = 0f;

        activeMap.localPosition = pos;
    }

    void ResetMapView()
    {
        Transform activeMap = GetActiveMapTransform();

        if (activeMap == null)
            return;

        currentZoom = Mathf.Clamp(defaultZoom, minZoom, maxZoom);
        isPanning = false;

        activeMap.localScale = Vector3.one * currentZoom;
        activeMap.localPosition = Vector3.zero;

        ClampMapPosition();
    }

    Transform GetActiveMapTransform()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= mapParents.Count)
            return null;

        return mapParents[currentLevelIndex].transform;
    }

    void OnDrawGizmos()
    {
        if (!showPanClampDebug) return;
        if (!Application.isPlaying) return;
        if (!isMapOpen) return;

        Transform activeMap = GetActiveMapTransform();

        if (activeMap == null)
            return;

        RectTransform mapBackgroundRect = mapBackground.GetComponent<RectTransform>();

        if (mapBackgroundRect == null)
            return;

        if (!mapSizes.TryGetValue(currentLevelIndex, out Vector2 mapSize))
            return;

        Vector2 viewportSize = mapBackgroundRect.rect.size;

        float scaledMapWidth = mapSize.x * currentZoom;
        float scaledMapHeight = mapSize.y * currentZoom;

        float maxX = (scaledMapWidth - viewportSize.x) / 2f;
        float maxY = (scaledMapHeight - viewportSize.y) / 2f;

        if (maxX < 0f)
        {
            if (allowHorizontalPanWhenMapSmallerThanView)
                maxX = fallbackHorizontalPanRange;
            else
                maxX = 0f;
        }

        if (maxY < 0f)
        {
            if (allowVerticalPanWhenMapSmallerThanView)
                maxY = fallbackVerticalPanRange;
            else
                maxY = 0f;
        }

        if (useViewportBasedClampPadding)
        {
            maxX += viewportSize.x * viewportClampPaddingMultiplier.x;
            maxY += viewportSize.y * viewportClampPaddingMultiplier.y;
        }
        else
        {
            maxX += clampPadding.x;
            maxY += clampPadding.y;
        }

        Vector3 center = mapBackgroundRect.TransformPoint(new Vector3(clampOffset.x, clampOffset.y, 0f));

        Vector3 topLeft = mapBackgroundRect.TransformPoint(new Vector3(-maxX + clampOffset.x, maxY + clampOffset.y, 0f));
        Vector3 topRight = mapBackgroundRect.TransformPoint(new Vector3(maxX + clampOffset.x, maxY + clampOffset.y, 0f));
        Vector3 bottomRight = mapBackgroundRect.TransformPoint(new Vector3(maxX + clampOffset.x, -maxY + clampOffset.y, 0f));
        Vector3 bottomLeft = mapBackgroundRect.TransformPoint(new Vector3(-maxX + clampOffset.x, -maxY + clampOffset.y, 0f));

        Gizmos.color = panClampDebugColor;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        Gizmos.color = currentMapPositionDebugColor;
        Gizmos.DrawSphere(activeMap.position, debugSphereSize);

        Gizmos.DrawLine(center + Vector3.left * debugCenterCrossSize, center + Vector3.right * debugCenterCrossSize);
        Gizmos.DrawLine(center + Vector3.down * debugCenterCrossSize, center + Vector3.up * debugCenterCrossSize);
    }

    void DestroyAllMaps()
    {
        for (int i = 0; i < mapParents.Count; i++)
        {
            DestroyMap(i);
        }
    }

    void DestroyMap(int mapIndexToDestroy)
    {
        Destroy(mapParents[mapIndexToDestroy].gameObject);
        mapParents.RemoveAt(mapIndexToDestroy);
    }
}