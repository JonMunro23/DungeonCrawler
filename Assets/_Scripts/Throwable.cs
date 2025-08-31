using System.Threading.Tasks;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    [SerializeField] ThrowableItemData itemData;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 launchVelocity)
    {
        rb.linearVelocity = launchVelocity;
        if(itemData.isExplosive && !itemData.explodesOnContact)
            Prime();
    }

    public async void Prime()
    {
        await Task.Delay((int)(itemData.fuseLength * 1000));

        Explode();
    }

    void Explode()
    {
        ParticleSystem explosionVFX = Instantiate(itemData.explosionVFX, transform.position, transform.rotation);
        AudioManager.Instance.PlayClipAtPoint(itemData.explosionSFX, transform.position, 2.5f, 25f, .3f);         
        Collider[] colliders = Physics.OverlapSphere(transform.position, itemData.blastRadius);
        foreach (Collider collider in colliders)
        {
            if(collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TryDamage(itemData.damage, DamageType.Explosive);
            }
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;

        if(itemData.explodesOnContact)
        {
            Explode();
            return;
        }

        if (other.CompareTag("Enemy")) return;

        AudioManager.Instance.PlayClipAtPoint(itemData.bounceSFX, transform.position, 2.5f, 15f, .3f);
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = new Color(1, .92f, .0016f, .25f);
    //    Gizmos.DrawSphere(transform.position, itemData.blastRadius);
    //}

}
