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

    Dictionary<int, List<MapTile>> generatedMaps = new Dictionary<int, List<MapTile>>();
    List<MapTile> currentActiveMap = new List<MapTile>();
    int currentLevelIndex;

    public static bool isMapOpen;
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

        HideMap();
    }

    void OpenMap()
    {
        isMapOpen = true;
        mapBackground.SetActive(true);
        HelperFunctions.SetCursorActive(true);
        currentLevelIndex = GridController.Instance.GetCurrentLevelIndex();
        if(generatedMaps.TryGetValue(currentLevelIndex, out List<MapTile> map))
        {
            ShowMap(map);
        }
        else
        {
            GenerateMap();
        }

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

        List<MapTile> map = new List<MapTile>();
        for (int i = 0; i < activeNodes.Count; i++)
        {
            GridNode node = nodes[i];
            Vector2 coord = coords[i];
            MapTile clone = Instantiate(mapTile, Vector2.zero, Quaternion.identity, mapContainerTransform);
            clone.InitTile(node);
            map.Add(clone);

            Vector2 localPos = new Vector2(coord.y * tileSize, coord.x * tileSize);
            localPos -= centerOffset;
            clone.transform.localPosition = localPos;
        }

        generatedMaps.Add(currentLevelIndex, map);
        currentActiveMap = map;
    }

    void ShowMap(List<MapTile> mapToShow)
    {
        foreach (MapTile tile in mapToShow)
        {
            tile.gameObject.SetActive(true);
            tile.RefreshTile();
        }
        currentActiveMap = mapToShow;
    }

    void HideMap()
    {
        foreach (MapTile tile in currentActiveMap)
        {
            tile.gameObject.SetActive(false);
        }   
    }

    void DestroyAllMaps()
    {
        for (int i = 0; i < generatedMaps.Count; i++)
        {
            DestroyMap(i);
        }
    }

    void DestroyMap(int mapIndexToDestroy)
    {
        if(generatedMaps.TryGetValue(mapIndexToDestroy, out List<MapTile> mapToDestroy))
        {
            foreach(MapTile tile in mapToDestroy)
            {
                Destroy(tile.gameObject);
            }
            generatedMaps.Remove(mapIndexToDestroy);
        }

    }
}
