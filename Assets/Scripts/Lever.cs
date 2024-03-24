using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public GameObject doorObject;

    public void Pulled()
    {
        Door door = doorObject.GetComponent<Door>();
        door.ToggleDoor();
        Debug.Log("Pulled the lever");
    }
}
