using System.Reflection;
using UnityEngine;

public class WeaponMotion : MonoBehaviour
{
    [SerializeField] private bool useBreathAnimation = true;
    [SerializeField] private float breathingSpeed = 2;
    [SerializeField] private float breathingAmplitude = .01f;
    private float breathingProgress;
    private Vector3 breathingRot;

    private void Update()
    {
        if (PauseMenu.isPaused || !PlayerController.isPlayerAlive)
            return;

        BreathingAnimation(1);

        transform.localRotation = transform.localRotation * Quaternion.Euler(breathingRot);
    }

    /// <summary>
    /// Breath animation simulates the natural movement of the arms while the character holds a weapon.
    /// </summary>
    /// <param name="speed">The animation speed.</param>
    public void BreathingAnimation(float speed = 1)
    {
        if (useBreathAnimation)
        {
            // The animation progress
            CalculateAngle(ref breathingProgress, breathingSpeed, speed);

            if (speed > 0)
            {
                float sin = Mathf.Sin(breathingProgress);
                float cos = Mathf.Cos(breathingProgress);

                // Calculates the target rotation using the values of sine and cosine multiplied by the animation magnitude.
                Vector3 breathingRot = new Vector3(sin * cos * breathingAmplitude, sin * breathingAmplitude);

                this.breathingRot = Vector3.Lerp(this.breathingRot, breathingRot, Time.deltaTime * 5 * breathingSpeed * speed);
            }
            else
            {
                breathingRot = Vector3.Lerp(breathingRot, Vector3.zero, Time.deltaTime * 5);
            }
        }
        else
        {
            breathingRot = Vector3.Lerp(breathingRot, Vector3.zero, Time.deltaTime * 5);
        }
    }


    private void CalculateAngle(ref float angle, float animationSpeed, float overallSpeed)
    {
        if (angle >= Mathf.PI * 2)
        {
            angle -= Mathf.PI * 2;
        }

        // Sum the time elapsed since the last frame multiplied by the animation speed.
        angle += Time.deltaTime * animationSpeed * overallSpeed;
    }
}
