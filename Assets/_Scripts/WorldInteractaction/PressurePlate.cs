using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[SelectionBase]
public class PressurePlate : InteractableBase
{
    public List<GameObject> presentObjects = new List<GameObject>();
    [SerializeField] Transform plateTransform;
    [SerializeField] float pressDownPos;
    float defaultPos;

    bool triggerOnExit;

    private void Start()
    {
        defaultPos = plateTransform.localPosition.y;
    }

    public override void Interact()
    {
        throw new System.NotImplementedException();
    }

    public override void InteractWithItem(ItemData item)
    {
        throw new System.NotImplementedException();
    }

    void PressPlateAnim()
    {
        plateTransform.localPosition = new Vector3(0, pressDownPos, 0);
    }

    void ReleasePlateAnim()
    {
        plateTransform.localPosition = new Vector3(0, defaultPos, 0);

    }

    public override void SetIsActivated(bool activatedState)
    {
        if (activatedState)
        {
            PressPlateAnim();
            TriggerObjects();
        }
        else
        {
            ReleasePlateAnim();
            if (GetTriggerOnExit())
                TriggerObjects();
        }

    }

    public void RemoveGameobjectFromPlate(GameObject objectToRemove)
    {
        if (presentObjects.Contains(objectToRemove))
            presentObjects.Remove(objectToRemove);

        if (presentObjects.Count == 0)
        {
            SetIsActivated(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(presentObjects.Count == 0)
            SetIsActivated(true);

        presentObjects.Add(other.gameObject);

        if (!other.TryGetComponent(out WorldItem item))
            return;

        item.occupiedPressurePlate = this;
    }

    private void OnTriggerExit(Collider other)
    {
        RemoveGameobjectFromPlate(other.gameObject);
    }

    public override void SetTriggerOnExit(bool triggerOnExit)
    {
        this.triggerOnExit = triggerOnExit;
    }

    public override bool GetTriggerOnExit()
    {
        return triggerOnExit;
    }
}
