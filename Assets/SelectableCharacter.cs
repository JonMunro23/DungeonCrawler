using UnityEngine;
using HighlightPlus;
using System;

public class SelectableCharacter : MonoBehaviour
{
    HighlightEffect highlightEffect;
    [SerializeField] CharacterData characterData;

    public bool isSelected;

    public static event Action<SelectableCharacter> OnCharacterSelected;

    private void Awake()
    {
        highlightEffect = GetComponentInChildren<HighlightEffect>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        if (highlightEffect.highlighted)
            return;

        highlightEffect.highlighted = true;
    }

    private void OnMouseDown()
    {
        if(highlightEffect.highlighted)
        {
            isSelected = true;
            OnCharacterSelected?.Invoke(this);
        }
    }

    private void OnMouseExit()
    {
        if(!isSelected)
            highlightEffect.highlighted = false;
    }
}
