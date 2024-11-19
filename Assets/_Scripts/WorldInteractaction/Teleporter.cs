using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField]
    Transform teleportLocation;

    private void OnTriggerStay(Collider other)
    {

        other.transform.position = teleportLocation.position;
        other.transform.rotation = teleportLocation.rotation;
        //play teleporter sound
        //play teleportation effect
    }
}
