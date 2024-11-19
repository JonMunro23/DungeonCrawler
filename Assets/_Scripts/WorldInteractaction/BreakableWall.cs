using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    public void Break()
    {
        //play breaking animation
        //play breaking sound
        Destroy(gameObject);
        Debug.Log(gameObject.name + " is broken");
    }
}
