using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(Prime());
    }

    public IEnumerator Prime()
    {
        yield return new WaitForSeconds(itemData.fuseLength);

        Explode();
    }

    public IEnumerator Arm()
    {
        isArming = true;
        yield return new WaitForSeconds(itemData.fuseLength);
        isArmed = true;
    }

    public void Explode()
    {
        if (itemData.detonationType == DetonationType.Proximity || itemData.detonationType == DetonationType.Remote)
            if (!isArmed) return;

        ParticleSystem explosionVFX = Instantiate(itemData.explosionVFX, transform.position, transform.rotation);
        AudioManager.Instance.PlayClipAtPoint(itemData.explosionSFX, transform.position, 2.5f, 25f, .3f);

        List<GridNode> nodesInBlastRadius = new List<GridNode>();
        GridNode centerNode = GridController.Instance.GetNodeFromWorldPos(transform.position);
        nodesInBlastRadius.Add(centerNode);
        nodesInBlastRadius.AddRange(centerNode.allNeighbouringNodes);

        if(itemData.statusEffect != null)
        {
            switch (itemData.statusEffect.effectType)
            {
                case StatusEffectType.DamageOverTime:
                    foreach (GridNode node in nodesInBlastRadius)
                    {
                        node.AddTimedNodeEffect(itemData.statusEffect);
                    }
                    break;
                default:
                    break;
            }
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, itemData.blastRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TryDamage(itemData.damage, itemData.damageType);
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
            StartCoroutine(Arm());

         if (other.CompareTag("Enemy")) return;

        AudioManager.Instance.PlayClipAtPoint(itemData.bounceSFX, transform.position, 2.5f, 15f, .3f);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1, .92f, .0016f, .25f);
    //    Gizmos.DrawSphere(transform.position, itemData.blastRadius);
    //}

}
