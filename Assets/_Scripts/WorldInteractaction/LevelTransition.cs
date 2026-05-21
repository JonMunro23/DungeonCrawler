using System;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] int levelIndexToGoTo;
    [SerializeField] Vector2Int playerMoveToCoords;

    public static event Action<int, Vector2Int> onLevelTransitionEntered;

    public void InitLevelTransition(int _levelIndexToGoTo, Vector2Int _playerMoveToCoords)
    {
        levelIndexToGoTo = _levelIndexToGoTo;
        playerMoveToCoords = _playerMoveToCoords;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController playerController))
        {
            onLevelTransitionEntered?.Invoke(levelIndexToGoTo, playerMoveToCoords);
            
        }
    }
}
