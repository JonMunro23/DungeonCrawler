using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    [SerializeField] ThrowableItemData itemData;
    Rigidbody rb;

    bool isArmed, isArming;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public bool IsArmed() => isArmed;

    public void Throw(Vector3 launchVelocity)
    {
        rb.linearVelocity = launchVelocity;
        if (itemData.isExplosive && itemData.detonationType == DetonationType.Timed)
            Prime();
    }

    public async void Prime()
    {
        await Task.Delay((int)(itemData.fuseLength * 1000));

        Explode();
    }

    public async void Arm()
    {
        isArming = true;
        await Task.Delay((int)(itemData.fuseLength * 1000));
        isArmed = true;
        Debug.Log("Armed");
    }

    public void Explode()
    {
        if (itemData.detonationType == DetonationType.Proximity || itemData.detonationType == DetonationType.Remote)
            if (!isArmed) return;

        ParticleSystem explosionVFX = Instantiate(itemData.explosionVFX, transform.position, transform.rotation);
        AudioManager.Instance.PlayClipAtPoint(itemData.explosionSFX, transform.position, 2.5f, 25f, .3f);

        List<GridNode> damageTiles = new List<GridNode>();
        GridNode centerNode = GridController.Instance.GetNodeFromWorldPos(transform.position);
        damageTiles.Add(centerNode);
        damageTiles.AddRange(centerNode.GetNeighbouringNodes(true));

        foreach (GridNode node in damageTiles)
        {
            node.HighlightCellPath();
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, itemData.blastRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TryDamage(itemData.damage, DamageType.Explosive);
            }
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;

        if (itemData.detonationType == DetonationType.Contact)
        {
            Explode();
            return;
        }
        else if (!isArming && (itemData.detonationType == DetonationType.Proximity || itemData.detonationType == DetonationType.Remote))
            Arm();

         if (other.CompareTag("Enemy")) return;

        AudioManager.Instance.PlayClipAtPoint(itemData.bounceSFX, transform.position, 2.5f, 15f, .3f);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1, .92f, .0016f, .25f);
    //    Gizmos.DrawSphere(transform.position, itemData.blastRadius);
    //}

}
