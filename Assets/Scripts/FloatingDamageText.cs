using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField]
    float speed;
    Transform cam;

    //Vector3 upwardsVariance;

    private void Awake()
    {
        cam = Camera.main.transform;
    }


    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }

    //void RandomSpeedAndUpwardsDirection()
    //{
    //    speed = Random.Range(-.4f, .4f);

    //    float xVariation = Random.Range(-.2f, .2f);
    //    float yVariation = Random.Range(-.2f, .2f);

    //    upwardsVariance = new Vector3(xVariation, yVariation, 0);
    //}
}
