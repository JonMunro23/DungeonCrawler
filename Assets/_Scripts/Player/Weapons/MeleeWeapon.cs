using System.Threading.Tasks;
using UnityEngine;
public class MeleeWeapon : Weapon
{
    public override MeleeWeapon GetMeleeWeapon()
    {
        return this;
    }

    public override RangedWeapon GetRangedWeapon()
    {
        return null;
    }

    public override void UseWeapon()
    {
        base.UseWeapon();
        int rand = Random.Range(0, 2);
        if (rand == 0)
            weaponAnimator.Play("Swing Right 1");
        else
            weaponAnimator.Play("Swing Left 1");

        weaponAudioEmitter.ForcePlay(GetRandomClipFromArray(weaponItemData.attackSFX), weaponItemData.attackSFXVolume);

        GridNode forwardNode = PlayerController.currentOccupiedNode.GetNodeInDirection(transform.root.forward);
        if (!forwardNode)
            return;

        if (forwardNode.currentOccupant.occupantType == GridNodeOccupantType.None)
            return;

        if (forwardNode.GetOccupyingGameobject().TryGetComponent(out IDamageable damageable))
        {
            //if (RollForHit())
            //{
                int damage = CalculateDamage(damageable.GetDamageData().currentArmourRating);
                bool isCrit = RollForCrit();
                if (isCrit)
                    damage *= Mathf.CeilToInt(weaponItemData.critDamageMultiplier + PlayerWeaponManager.bonusCritMultiplier);

                damageable.TryDamage(damage, isCrit);
           // }
        }
    }
}
