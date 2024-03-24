using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractive
{
    public bool requiresKey;
    public ItemObject.KeyType keyType;
    [SerializeField]
    bool isOpen;

    [SerializeField]
    Vector3 openedPos, closedPos;
    [SerializeField]
    float openingDuration;
    float timeElapsed;

    bool isInMotion;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = isOpen ? openedPos : closedPos;
    }

    void Update()
    {
        if (isInMotion)
        {
            if (timeElapsed < openingDuration)
            {
                float t = timeElapsed / openingDuration;
                if (isOpen)
                    transform.position = Vector3.Lerp(closedPos, openedPos, t);
                else
                    transform.position = Vector3.Lerp(openedPos, closedPos, t);
                timeElapsed += Time.deltaTime;
            }
            else
            {
                transform.position = isOpen ? openedPos : closedPos;
                isInMotion = false;
                timeElapsed = 0;
            }

        }
    }

    public void ToggleDoor()
    {
        if (!isInMotion)
        {
            isOpen = !isOpen;
            isInMotion = true;
        }
    }

    public void Interact()
    {
        ToggleDoor();
    }
}
