using UnityEngine;

public class BillboardFX : MonoBehaviour
{
    [SerializeField] Transform camTransform;
    Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation * Quaternion.Euler(new Vector3(0,180,0));
    }

    void Update()
    {
        transform.rotation = camTransform.rotation * originalRotation;
    }
}
