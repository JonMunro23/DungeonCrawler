using UnityEngine;
public class MeleeWeapon : Weapon
{
    public override void UseWeapon()
    {
        base.UseWeapon();
         
        int rand = Random.Range(0, 2);
        if (rand == 0)
            weaponAnimator.Play("Swing Right 1");
        else
            weaponAnimator.Play("Swing Left 1");

        GridNode forwardNode = GridController.Instance.GetNodeInDirection(PlayerController.currentOccupiedNode, transform.root.forward);
        if (!forwardNode)
            return;

        if (forwardNode.currentOccupant.occupantType == GridNodeOccupantType.None)
            return;

        if (forwardNode.GetOccupyingGameobject().TryGetComponent(out IDamageable damageable))
        {
            int damage = CalculateDamage();
            bool isCrit = RollForCrit();
            if (isCrit)
                damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier);

            damageable.TakeDamage(damage, isCrit);
        }
    }
}
