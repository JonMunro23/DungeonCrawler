using UnityEngine;

public class CharacterSelection : MonoBehaviour
{
    [SerializeField] GameObject helicopter;
    [SerializeField] GameObject introCanvas;

    private void OnEnable()
    {
        MainMenu.onNewGameStarted += OnNewGameStarted;
    }

    private void OnDisable()
    {
        MainMenu.onNewGameStarted -= OnNewGameStarted;
    }

    void OnNewGameStarted()
    {
        helicopter.SetActive(true);
        introCanvas.SetActive(true);
        HelperFunctions.SetCursorActive(true);
    }
}
