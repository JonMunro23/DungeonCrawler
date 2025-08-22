using System.Threading.Tasks;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    [SerializeField] ThrowableItemData itemData;
    Rigidbody rb;

    AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Launch(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
    }

    public async void Prime()
    {
        await Task.Delay((int)(itemData.fuseLength * 1000));

        Explode();
    }

    void Explode()
    {
        ParticleSystem explosionVFX = Instantiate(itemData.explosionVFX, transform.position, transform.rotation);
        audioSource.PlayOneShot(itemData.explosionSFX);
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
}
