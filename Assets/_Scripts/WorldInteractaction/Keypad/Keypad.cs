using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Keypad : InteractableBase
{
    int codeLength = 4;

    [SerializeField] char[] correctCode;
    [SerializeField] char[] inputCode;
    [SerializeField] List<KeypadButton> buttons = new List<KeypadButton>();
    [SerializeField] int currentCodeIndex;

    [SerializeField] TMP_Text codeDisplay;
    [SerializeField] MeshRenderer displayMesh;
    [SerializeField] Material correctMat, incorrectMat, defaultMat;
    [SerializeField] float resetTimer = 1f;

    public void Init(string code = "")
    {
        if(code.Length == 4)
        {
            correctCode = code.ToCharArray();
        }
        else
            GenerateCode();

        Debug.Log(new string(correctCode));

        inputCode = new char[codeLength];
        for (int i = 0; i < codeLength; i++)
        {
            inputCode[i] = '_';
        }

        currentCodeIndex = 0;

        UpdateDisplay();
    }

    void GenerateCode()
    {
        correctCode = new char[codeLength];

        for (int i = 0; i < codeLength; i++)
        {
            correctCode[i] = (char)('0' + Random.Range(0, 10));
        }
    }

    public void InputNumber(int buttonNumber)
    {
        if (currentCodeIndex >= codeLength) return;

        inputCode[currentCodeIndex] = (char)('0' + buttonNumber);

        currentCodeIndex++;

        UpdateDisplay();
    }

    public void SubmitCode()
    {
        SetKeypadInteractable(false);
        if (IsCodeCorrect())
        {
            UpdateDisplayMeshMaterial(correctMat);
            TriggerObjects();
        }
        else
        {
            UpdateDisplayMeshMaterial(incorrectMat);
            StartCoroutine(ResetKeycardTimer());
        }
    }

    public void CancelCode()
    {
        ResetKeypad();
    }

    IEnumerator ResetKeycardTimer()
    {
        yield return new WaitForSeconds(resetTimer);
        UpdateDisplayMeshMaterial(defaultMat);
        SetKeypadInteractable(true);
        ResetKeypad();
    }

    void ResetKeypad()
    {
        for (int i = 0; i < inputCode.Length; i++)
        {
            inputCode[i] = '_';
        }

        currentCodeIndex = 0;

        UpdateDisplay();
    }

    bool IsCodeCorrect()
    {
        for (int i = 0; i < codeLength; i++)
        {
            if (inputCode[i] != correctCode[i])
                return false;
        }

        return true;
    }

    void UpdateDisplay()
    {
        codeDisplay.text = new string(inputCode);
    }

    void UpdateDisplayMeshMaterial(Material newMaterial)
    {
        displayMesh.material = newMaterial;
    }

    void SetKeypadInteractable(bool isInteractable)
    {
        canUse = isInteractable;
        foreach (KeypadButton button in buttons)
        {
            button.canUse = isInteractable;
        }
    }
}