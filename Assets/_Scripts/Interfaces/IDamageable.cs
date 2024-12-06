using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public void TryDamage(int damageTaken, bool wasCrit = false);  
}
