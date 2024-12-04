using System;
using UnityEngine;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] int levelIndexToGoTo;
    [SerializeField] Vector2 playerMoveToCoords;

    public static Action<int, Vector2> onLevelTransitionEntered;

    public void InitLevelTransition(int _levelIndexToGoTo, Vector2 _playerMoveToCoords)
    {
        levelIndexToGoTo = _levelIndexToGoTo;
        playerMoveToCoords = _playerMoveToCoords;
    }

    private async void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController playerController))
        {
            //await playerController.FadeOutScreen();
            onLevelTransitionEntered?.Invoke(levelIndexToGoTo, playerMoveToCoords);
            //await playerController.FadeInScreen();
            
        }
    }
}
