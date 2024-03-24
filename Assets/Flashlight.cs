using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [SerializeField]
    KeyCode flashlightActivationKey;
    Light flashlight;
    bool isLightOn;

    private void Awake()
    {
        flashlight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(flashlightActivationKey))
        {
            ToggleFlashlight();
        }
    }

    void ToggleFlashlight()
    {
        if(isLightOn)
            TurnLightOff();
        else 
            TurnLightOn();

    }

    void TurnLightOff()
    {
        isLightOn = false;
        flashlight.enabled = false;
    }

    void TurnLightOn()
    {
        isLightOn = true;
        flashlight.enabled = true;
    }
}
