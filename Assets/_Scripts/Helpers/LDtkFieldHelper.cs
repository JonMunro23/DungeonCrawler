using System;
using UnityEngine;

public static class LDtkFieldHelper
{
    /// <summary>
    /// Safely converts an LDtk field value (object) to a given type (int, float, bool, string).
    /// </summary>
    public static T GetValue<T>(object value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "LDtk field value is null");

        Type targetType = typeof(T);

        try
        {
            // Handle common LDtk cases directly
            if (targetType == typeof(int))
                return (T)(object)Convert.ToInt32(value);
            if (targetType == typeof(float))
                return (T)(object)Convert.ToSingle(value);
            if (targetType == typeof(bool))
                return (T)(object)Convert.ToBoolean(value);
            if (targetType == typeof(string))
                return (T)(object)value.ToString();

            // Fallback for unexpected conversions
            return (T)Convert.ChangeType(value, targetType);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LDtkFieldHelper] Failed to convert '{value}' ({value.GetType()}) to {targetType}: {e.Message}");
            return default;
        }
    }
}
