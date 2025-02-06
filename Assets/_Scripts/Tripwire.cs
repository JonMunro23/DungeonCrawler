using UnityEngine;

public class Tripwire : InteractableBase
{
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform lineRendererOrigin;
    Collider initialCollider;
    RaycastHit hit;
    Ray ray;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ray = new Ray(lineRendererOrigin.position, lineRendererOrigin.forward);
        lineRenderer.SetPosition(0, lineRendererOrigin.position);

        InitTripwire();
    }

    public void InitTripwire()
    {
        if (Physics.Raycast(ray, out hit))
        {
            lineRenderer.SetPosition(1, hit.point);
            initialCollider = hit.collider;
        }
    }

    private void Update()
    {
        if (isActivated)
            return;

        if(Physics.Raycast(ray, out hit))
        {
            if (hit.collider != initialCollider)
            {
                if(!isActivated)
                    SetIsActivated(true);

                lineRenderer.enabled = false;
            }
        }
    }

    public override void Interact()
    {
        throw new System.NotImplementedException();
    }

    public override void InteractWithItem(ItemData item)
    {
        throw new System.NotImplementedException();
    }

    public override void SetIsActivated(bool activatedState)
    {
        if(activatedState)
            TriggerObjects();
    }

    public override void SetTriggerOnExit(bool triggerOnExit)
    {
    }

    public override bool GetTriggerOnExit()
    {
        throw new System.NotImplementedException();
    }
}
