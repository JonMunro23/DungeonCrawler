[System.Serializable]
public class DamageData
{
    public int currentHealth;
    public int currentArmourRating;
    public int currentEvasionRating;

    public DamageData(int currentHealth, int currentArmourRating, int currentEvasionRating)
    {
        this.currentHealth = currentHealth;
        this.currentArmourRating = currentArmourRating;
        this.currentEvasionRating = currentEvasionRating;
    }
}

public interface IDamageable
{
    public void TryDamage(int damageTaken, bool wasCrit = false);  
    public DamageData GetDamageData();
    public void AddStatusEffect(StatusEffectType statusEffectTypeToAdd, float duration = 5f);
}
