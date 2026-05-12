using UnityEngine;

public class SecretsManager : MonoBehaviour
{
    // ==========================
    // References
    // ==========================

    [SerializeField] AudioClip secretJingle;

    int discoveredSecrets;

    // ==========================
    #region Unity Lifecycle
   
    private void OnEnable()
    {
        SecretAreaTrigger.onSecretDiscovered += OnSecretAreaDiscovered;
    }

    private void OnDisable()
    {
        SecretAreaTrigger.onSecretDiscovered -= OnSecretAreaDiscovered;
    }

    #endregion
    // ==========================

    // ==========================
    #region Event handlers

    void OnSecretAreaDiscovered(int secretExperienceValue)
    {
        discoveredSecrets++;
        PlaySecretJingle();
    }

    #endregion
    // ==========================

    // ==========================
    #region External API

    public void SetDiscoveredSecretsAmount(int discoveredSecrets)
    {
        this.discoveredSecrets = discoveredSecrets;
    }

    public int GetDiscoveredSecretsAmount()
    {
        return discoveredSecrets;
    }
    #endregion
    // ==========================

    // ==========================
    #region Secret Jingle

    void PlaySecretJingle()
    {
        if(secretJingle != null)
            AudioManager.Instance.Play2DClip(secretJingle, .25f);
    }

    #endregion
    // ==========================


}
