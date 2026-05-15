using System;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerControls Controls { get; private set; }

    public static Action<PlayerControls> OnPlayerControlsInitialised;

    private void Awake()
    {
        Controls = new PlayerControls();
        OnPlayerControlsInitialised?.Invoke(Controls);
    }

    private void OnEnable()
    {
        Controls.Enable();
    }

    private void OnDisable()
    {
        Controls.Disable();
    }
}
