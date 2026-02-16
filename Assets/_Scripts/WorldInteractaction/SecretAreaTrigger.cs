using System;
using UnityEngine;

public class SecretAreaTrigger : MonoBehaviour
{
    BoxCollider triggerCollider;

    int experienceValue;
    bool isTriggered;
    /// <summary>
    ///  int = experience value of secret discovery
    /// </summary>
    public static event Action<int> onSecretDiscovered;

    private void Awake()
    {
        triggerCollider = GetComponent<BoxCollider>();
    }

    // ==========================
    #region External API

    public void SetExperienceValue(int experienceValue)
    {
        this.experienceValue = experienceValue;
    }

    public void SetColliderSize(float width, float height)
    {
        triggerCollider.size = new Vector3(width, 3, height);
    }

    #endregion
    // ==========================

    private void OnTriggerEnter(Collider other)
    {
        if(isTriggered) return;

        if(other.CompareTag("Player"))
        {
            isTriggered = true;
            triggerCollider.enabled = false;

            onSecretDiscovered?.Invoke(experienceValue);
        }
    }
}
