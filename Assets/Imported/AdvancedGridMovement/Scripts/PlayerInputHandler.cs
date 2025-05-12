using System;
using UnityEngine;
using UnityEngine.Events;


public class PlayerInputHandler : MonoBehaviour
{
    [System.Serializable]
    public class EventMapping
    {
        public KeyCode key;
        public UnityEvent callback;
    }

    [System.Serializable]
    public class HoldEventMapping : EventMapping
    {
        public float minHoldDuration;
        public UnityEvent keyReleaseCallback;
    }

    [SerializeField] private EventMapping[] eventMappings;
    [SerializeField] private EventMapping[] eventMappingsKeyDown;
    [SerializeField] private EventMapping[] eventMappingsKeyUp;
    [SerializeField] private HoldEventMapping[] eventMappingsHold;

    static float holdTime;
    static bool canInvoke = true;
    static KeyCode currentHeldKey;

    void Update()
    {
        if(PauseMenu.isPaused || !PlayerController.isPlayerAlive || MapController.isMapOpen) return;

        Action<EventMapping> actionKeyDown = new Action<EventMapping>(InputMappingKeyDown);
        Array.ForEach(eventMappingsKeyDown, actionKeyDown);

        Action<EventMapping> actionKeyUp = new Action<EventMapping>(InputMappingKeyUp);
        Array.ForEach(eventMappingsKeyUp, actionKeyUp);

        if(eventMappings.Length > 0)
        {
            Action<EventMapping> action = new Action<EventMapping>(InputMapping);
            Array.ForEach(eventMappings, action);
        }

        if(eventMappingsHold.Length > 0)
        {
            Action<HoldEventMapping> action = new Action<HoldEventMapping>(InputMappingHold);
            Array.ForEach(eventMappingsHold, action);
        }
    }

    private static void InputMapping(EventMapping eventMapping)
    {
        if (Input.GetKey(eventMapping.key))
        {
            eventMapping.callback.Invoke();
        }
    }

    private static void InputMappingKeyDown(EventMapping eventMapping)
    {
        if (Input.GetKeyDown(eventMapping.key))
        {
            eventMapping.callback.Invoke();
        }
    }

    private static void InputMappingKeyUp(EventMapping eventMapping)
    {
        if (Input.GetKeyUp(eventMapping.key) && currentHeldKey != eventMapping.key)
        {
            eventMapping.callback.Invoke();
        }
    }

    static void InputMappingHold(HoldEventMapping holdEventMapping)
    {
        if(Input.GetKey(holdEventMapping.key))
        {
            holdTime += Time.deltaTime;
            if(holdTime > holdEventMapping.minHoldDuration && canInvoke == true)
            {
                currentHeldKey = holdEventMapping.key;
                canInvoke = false;
                holdEventMapping.callback.Invoke();
            }

        }
        if(Input.GetKeyUp(holdEventMapping.key))
        {
            holdTime = 0;
            canInvoke = true;
            holdEventMapping.keyReleaseCallback.Invoke();
            currentHeldKey = KeyCode.None;
        }
    }
}
