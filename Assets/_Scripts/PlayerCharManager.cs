using UnityEngine;

public class PlayerCharManager : MonoBehaviour
{
    [SerializeField] SelectableCharacter currentlySelectedCharacter;

    private void OnEnable()
    {
        SelectableCharacter.OnCharacterSelected += OnCharacterSelected; 
    }

    private void OnDisable()
    {
        SelectableCharacter.OnCharacterSelected -= OnCharacterSelected;
    }

    void OnCharacterSelected(SelectableCharacter selectedChar)
    {
        if(currentlySelectedCharacter != null)
            currentlySelectedCharacter.isSelected = false;

        currentlySelectedCharacter = selectedChar;
    }
}
