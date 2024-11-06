using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileData projectile;
    Rigidbody rb;
    float speed;
    [HideInInspector]
    public int damage;

    Transform spawnLocation;

    bool canDealDamage = true;

    // Start is called before the first frame update
    void Start()
    {
        spawnLocation = GameObject.FindGameObjectWithTag(projectile.spawnLocationObjectTag).transform;
        speed = projectile.speed;
        if(projectile.isRigidbodyInParent == true)
        {
            rb = GetComponentInParent<Rigidbody>();
        }
        else if(projectile.isRigidbodyInParent == false)
        {
            rb = GetComponent<Rigidbody>();
        }

        if(projectile.isAffectedByGravity == true)
        {
            rb.useGravity = true;
        }
        else if(projectile.isAffectedByGravity == false)
        {
            rb.useGravity = false;
        }
        rb.AddForce(spawnLocation.forward * speed * Time.deltaTime, ForceMode.Impulse);
    }

    int CalculateDamage(ProjectileData projectile)
    {
        int damage = Random.Range(projectile.minDamage, projectile.maxDamage + 1);
        return damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(projectile.destroyOnContact == true)
        {
            if(other.CompareTag("Enemy"))
            {
                if (projectile.hasOwnDamage == true)
                {
                    if (canDealDamage == true)
                    {
                        other.GetComponent<NPCGroupController>().TakeDamage(CalculateDamage(projectile));
                        canDealDamage = false;
                    }
                }
                else if (projectile.hasOwnDamage == false)
                {
                    if (canDealDamage == true)
                    {
                        other.GetComponent<NPCGroupController>().TakeDamage(damage);
                        canDealDamage = false;
                    }
                }
                if (projectile.isRigidbodyInParent == true)
                {
                    Destroy(transform.parent.gameObject);
                }
                else if (projectile.isRigidbodyInParent == false)
                {
                    Destroy(gameObject);
                }
            }
            if(projectile.isRigidbodyInParent == true)
            {
                Destroy(transform.parent.gameObject);
            }
            else if(projectile.isRigidbodyInParent == false)
            {
                Destroy(gameObject);
            }
        }
        if(other.CompareTag("Enemy"))
        {
            if (projectile.hasOwnDamage == true)
            {
                if (canDealDamage == true)
                {
                    other.GetComponent<NPCGroupController>().TakeDamage(CalculateDamage(projectile));
                    canDealDamage = false;
                }
            }
            else if (projectile.hasOwnDamage == false)
            {
                if (canDealDamage == true)
                {
                    other.GetComponent<NPCGroupController>().TakeDamage(damage);
                    canDealDamage = false;
                }

            }
            if (projectile.isRigidbodyInParent == true)
            {
                Destroy(transform.parent.gameObject);
            }
            else if (projectile.isRigidbodyInParent == false)
            {
                Destroy(gameObject);
            }
        }
    }
}
