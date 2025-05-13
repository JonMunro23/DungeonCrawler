using UnityEngine;

[SelectionBase]
public class ShootableTarget : InteractableBase
{
    [SerializeField] MeshRenderer targetMeshRenderer;
    [SerializeField] Material activatedMaterial;
    public override bool GetTriggerOnExit()
    {
        throw new System.NotImplementedException();
    }

    public override void Interact()
    {
        SetIsActivated(true);
    }

    public override void InteractWithItem(ItemData item)
    {
        throw new System.NotImplementedException();
    }

    public override void SetIsActivated(bool activatedState)
    {
        if(activatedState)
        {
            TriggerObjects();
            SetTargetMaterial(activatedMaterial);
        }
    }

    void SetTargetMaterial(Material materialToSet)
    {
        targetMeshRenderer.material = materialToSet;
    }

    public override void SetTriggerOnExit(bool triggerOnExit)
    {
        
    }
}
