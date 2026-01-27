using TMPro;
using UnityEngine;

[SelectionBase]
public class Sign : MonoBehaviour
{
    [SerializeField] Canvas textCanvas;
    [SerializeField] TMP_Text signText;

    //public void void Interact()
    //{
    //    DisplaySignText();
    //}

    //void DisplaySignText()
    //{
    //    Debug.Log("Displaying Sign Text...");
    //    textCanvas.enabled = true;  
    //}

    public void SetSignText(string newSignText)
    {
        signText.text = newSignText;
    }
}
