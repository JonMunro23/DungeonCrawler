using System.Collections;
using UnityEngine;

public class NPCAttackController : MonoBehaviour
{
    NPCController npcController;

    public bool isAttacking {  get; private set; }
    bool canAttack;

    GridNode playerNode;
    public void Init(NPCController newNPCController)
    {
        npcController = newNPCController;
        canAttack = true;
    }

    public void TryAttack()
    {
        if (isAttacking || !canAttack)
            return;

        StartCoroutine(Attack());
    }

    int GetRandomDamageValue()
    {
        return (int)Random.Range(npcController.npcData.minMaxDamage.x, npcController.npcData.minMaxDamage.y);
    }

    IEnumerator Attack()
    {
        canAttack = false;
        isAttacking = true;
        if(playerNode == npcController.currentlyOccupiedGridnode.GetNodeInDirection(npcController.movementController.currentOrientation.forward))
            npcController.animController.PlayAnimation("MeleeAttack");
        else
            npcController.animController.PlayAnimation("Attack");

        if (npcController.npcData.attackSFx.Length > 0)
            npcController.audioSource.PlayOneShot(GetRandomAttackClip());

        yield return new WaitForSeconds(npcController.npcData.delayBeforeDamageDealt);
        var occupyingGameObject = playerNode.GetOccupyingGameobject();
        if(occupyingGameObject)
        {
            if(occupyingGameObject.TryGetComponent(out PlayerController player))
            {
                if(player.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TryDamage(GetRandomDamageValue());
                }
            }
        }
        StartCoroutine(AttackCooldown());
    }

    AudioClip GetRandomAttackClip()
    {
        int rand = Random.Range(0, npcController.npcData.attackSFx.Length);
        return npcController.npcData.attackSFx[rand];
    }
    public bool CheckForPlayer()
    {
        if(npcController.npcData.isRanged)
        {
            Ray ray = new Ray(npcController.movementController.currentOrientation.position, npcController.movementController.currentOrientation.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, npcController.npcData.attackRange * 3))
            {
                //Debug.Log(hit.transform.name);
                if (hit.transform.CompareTag("Player"))
                {
                    playerNode = GridController.Instance.GetNodeFromWorldPos(hit.transform.position);
                    return true;
                }
            }
        }

        playerNode = npcController.currentlyOccupiedGridnode.GetNodeInDirection(npcController.movementController.currentOrientation.forward);
        if (!playerNode)
            return false;

        if (playerNode.currentOccupant.occupantType == GridNodeOccupantType.Player)
            return true;

        return false;
    }
    
    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(npcController.npcData.attackCooldown);
        isAttacking = false;
        yield return new WaitForSeconds(npcController.npcData.delayBetweenAttacks);
        canAttack = true;
        npcController.TryAttack();
    }
}
