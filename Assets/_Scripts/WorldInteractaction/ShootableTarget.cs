using UnityEngine;

[SelectionBase]
public class ShootableTarget : InteractableBase
{
    [SerializeField] MeshRenderer targetMeshRenderer;
    [SerializeField] Material activatedMaterial;

    public override void Interact()
    {
        SetStartingActivationState(true);
    }

    public override void SetStartingActivationState(bool activatedState)
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
}
