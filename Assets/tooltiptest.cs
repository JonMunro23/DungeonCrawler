using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModelShark;

public class tooltiptest : MonoBehaviour
{
    TooltipTrigger trigger;

    // Start is called before the first frame update
    void Start()
    {
        trigger = GetComponent<TooltipTrigger>();
        trigger.parameterizedTextFields[0].value = "cum cum cum";
        trigger.tooltipStyle.transform.localScale = Vector3.one * 3;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
