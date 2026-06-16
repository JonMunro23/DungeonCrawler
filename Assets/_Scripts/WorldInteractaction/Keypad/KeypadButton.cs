using TMPro;
using UnityEngine;

public class KeypadButton : InteractableBase
{
    [SerializeField] TMP_Text buttonNumberText;

    [SerializeField] int buttonNumber;
    [SerializeField] bool isSubmitButton;
    [SerializeField] bool isCancelButton;
    Keypad parentKeypad;

    private void Awake()
    {
        parentKeypad = GetComponentInParent<Keypad>();
    }

    [ContextMenu("SetButtonNumber")]
    public void SetButtonNumberText()
    {
        buttonNumberText.text = buttonNumber.ToString();
    }

    public override void Interact()
    {
        PressButton();
    }

    void PressButton()
    {
        if(isSubmitButton)
        {
            parentKeypad.SubmitCode();
            return;
        }

        if (isCancelButton)
        {
            parentKeypad.CancelCode();
            return;
        }

        parentKeypad.InputNumber(buttonNumber);
    }
}
