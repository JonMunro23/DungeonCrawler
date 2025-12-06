using System.Collections;
using UnityEngine;

public class HelperFunctions
{
    public static void SetCursorActive(bool isActive)
    {
        if (isActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (!CharacterMenuUIController.isCharacterMenuOpen && !PlayerInventoryManager.isInContainer && !WorldInteractionManager.hasGrabbedItem && !MainMenu.isInMainMenu && !MapController.isMapOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public static IEnumerator DamageOverTime(IDamageable affectedEntity, StatusEffect statusEffect)
    {
        if (affectedEntity == null || statusEffect == null)
            yield break;

        float total = Mathf.Max(0f, statusEffect.effectLength);
        float tick = Mathf.Max(0.0001f, statusEffect.damageInterval); // guard against 0/negatives
        int dmg = Mathf.RoundToInt(statusEffect.damage);
        var type = statusEffect.damageType;

        if (total <= 0f)
            yield break;

        // Optional: immediate first tick (common for DoTs). Move/remove as needed.
        affectedEntity.TryDamage(dmg, type);

        float elapsed = 0f;

        while (elapsed < total)
        {
            // Don’t overshoot total duration on the last wait.
            float wait = Mathf.Min(tick, total - elapsed);
            if (wait <= 0f) break;

            yield return new WaitForSeconds(wait);
            elapsed += wait;

            // Final tick at/before the end.
            if (elapsed <= total + 0.0001f)
                affectedEntity.TryDamage(dmg, type);
        }
    }

}
