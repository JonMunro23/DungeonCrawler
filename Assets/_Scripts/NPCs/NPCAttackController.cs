using System.Collections;
using UnityEngine;

public class NPCAttackController : MonoBehaviour
{
    NPCController groupController;

    [SerializeField] float attackCooldown, delayBetweenAttacks, delayBeforeDamageDealt;
    [SerializeField] AudioClip[] attackSFx;
    public bool isAttacking {  get; private set; }
    bool canAttack;

    GridNode frontNode;
    public void Init(NPCController newGroupController)
    {
        groupController = newGroupController;
        canAttack = true;
    }

    public void TryAttack()
    {
        if (isAttacking || !canAttack)
            return;

        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        canAttack = false;
        isAttacking = true;
        groupController.animController.PlayAnimation("Attack");
        groupController.audioSource.PlayOneShot(GetRandomAttackClip());
        yield return new WaitForSeconds(delayBeforeDamageDealt);
        var occupyingGameObject = frontNode.GetOccupyingGameobject();
        if(occupyingGameObject)
        {
            if(occupyingGameObject.TryGetComponent(out PlayerController player))
            {
                if(player.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(groupController.NPCData.damage);
                }
            }
        }
        StartCoroutine(AttackCooldown());
    }

    AudioClip GetRandomAttackClip()
    {
        int rand = Random.Range(0, attackSFx.Length);
        return attackSFx[rand];
    }
    public bool CheckForPlayer()
    {
        frontNode = groupController.currentlyOccupiedGridnode.GetNodeInDirection(groupController.movementController.currentOrientation.forward);
        if (!frontNode)
            return false;

        if (frontNode.currentOccupant.occupantType == GridNodeOccupantType.Player)
            return true;

        return false;
    }
    
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        yield return new WaitForSeconds(delayBetweenAttacks);
        canAttack = true;
        groupController.TryAttack();
    }
}
