using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool isPlatePressed;
    public GameObject linkedObject;

    public List<GameObject> presentObjects = new List<GameObject>();

    public void ToggleLinked()
    {
        if (!linkedObject)
            return;

        if(isPlatePressed == true)
        {
            linkedObject.GetComponent<Door>().ToggleDoor();
        }
        else if(isPlatePressed == false)
        {
            linkedObject.GetComponent<Door>().ToggleDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        presentObjects.Add(other.gameObject);
        if (other.CompareTag("WorldItem"))
        {
            other.GetComponent<WorldItem>().isOnPressurePlate = true;
        }
        if (isPlatePressed == false)
        {
            isPlatePressed = true;
            ToggleLinked();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        presentObjects.Remove(other.gameObject);
        if(isPlatePressed == true)
        {
            if(presentObjects.Count == 0)
            {
                isPlatePressed = false;
                ToggleLinked();
            }
        }
    }
}
