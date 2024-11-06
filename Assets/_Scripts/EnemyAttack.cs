using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage;
    public float attackCooldown;

    bool canAttack = true;

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player") && canAttack == true)
        {
            canAttack = false;
            StartCoroutine(AttackCooldown());
            other.GetComponent<PlayerHealthController>().TakeDamage(damage);
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
