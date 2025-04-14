using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    float speed;
    Transform cam;

    private void Awake()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void SetUpwardsSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
