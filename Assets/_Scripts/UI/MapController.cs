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

    List<GameObject> mapParents = new List<GameObject>();
    Dictionary<int, List<MapTile>> generatedMaps = new Dictionary<int, List<MapTile>>();
    int currentLevelIndex;

    public static bool isMapOpen;

    private void OnEnable()
    {
        GridController.OnLevelGenerated += OnLevelGenerated;
    }

    private void OnDisable()
    {
        GridController.OnLevelGenerated -= OnLevelGenerated;
    }

    void OnLevelGenerated(int levelIndex)
    {
        GenerateMap(levelIndex);
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
    }

    void GenerateMap(int levelIndex)
    {
        List<GridNode> levelNodes = GridController.Instance.GetCurrentNodesForLevel(levelIndex);
        GridNode[] nodes = levelNodes.ToArray();
        Vector2Int[] coords = new Vector2Int[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            coords[i] = nodes[i].Coords.Pos;
        }

        if (levelNodes.Count == 0)
            return;

        GameObject mapParent = new GameObject($"Level{levelIndex} Map");
        mapParent.transform.SetParent(mapBackground.transform, true);
        mapParents.Add(mapParent);

        float minX = coords.Min(c => c.y);
        float maxX = coords.Max(c => c.y);
        float minY = coords.Min(c => c.x);
        float maxY = coords.Max(c => c.x);

        float tileSize = 50f;
        float totalWidth = (maxX - minX + 1) * tileSize;
        float totalHeight = (maxY - minY + 1) * tileSize;

        Vector2 centerOffset = new Vector2(totalWidth / 2f, -totalHeight / 2f);

        List<MapTile> map = new List<MapTile>();

        for (int i = 0; i < nodes.Length; i++)
        {
            GridNode node = nodes[i];
            Vector2Int coord = coords[i];

            MapTile clone = Instantiate(mapTile, Vector2.zero, Quaternion.identity, mapContainerTransform);
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

        if(generatedMaps.TryGetValue(levelIndex, out var map))
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
