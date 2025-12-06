using UnityEngine;
using HighlightPlus;
using System;
using TMPro;

public class SelectableCharacter : MonoBehaviour
{
    HighlightEffect highlightEffect;

    [Header("Canvas Properties")]
    [SerializeField] GameObject charInfoCanvas;
    [SerializeField] TMP_Text charNameText;
    [SerializeField] TMP_Text charClassText;
    [SerializeField] TMP_Text charDescriptionText;

    [Space]
    [SerializeField] CharacterData characterData;
    [SerializeField] GameObject mainCamera;

    public bool isSelected;

    public static event Action<CharacterData> OnCharacterSelected;

    private void Awake()
    {
        highlightEffect = GetComponentInChildren<HighlightEffect>();
    }

    private void Start()
    {
        InitCanvas();
    }

    void InitCanvas()
    {
        charNameText.text = characterData.charName;
        charClassText.text = characterData.className;
        charDescriptionText.text = characterData.charDescription;

        charInfoCanvas.SetActive(false);
    }

    private void OnMouseOver()
    {
        if (MainMenu.isInMainMenu)
            return;

        if (highlightEffect.highlighted)
            return;

        highlightEffect.highlighted = true;
        charInfoCanvas.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (MainMenu.isInMainMenu)
            return;

        if (highlightEffect.highlighted)
        {
            isSelected = true;
            highlightEffect.highlighted = false;
            OnCharacterSelected?.Invoke(characterData);
            mainCamera.SetActive(false);
            charInfoCanvas.SetActive(false);
        }
    }

    private void OnMouseExit()
    {
        if (MainMenu.isInMainMenu)
            return;

        if (!isSelected)
        {
            highlightEffect.highlighted = false;
            charInfoCanvas.SetActive(false);
        }
    }
}
