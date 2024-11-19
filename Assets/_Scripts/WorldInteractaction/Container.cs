using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [SerializeField]
    GameObject containerInventory;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Escape))
        //{
        //    CloseContainer();
        //}
    }

    public void OpenContainer()
    {
        containerInventory.SetActive(true);
    }

    public void CloseContainer()
    {
        containerInventory.SetActive(false);
    }
}
