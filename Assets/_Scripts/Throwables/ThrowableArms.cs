using UnityEngine;

public class ThrowableArms : MonoBehaviour
{
    [SerializeField] Transform throwLocation;
    Animator armsAnimator;

    private void Awake()
    {
        armsAnimator = GetComponentInChildren<Animator>();
    }

    public Animator GetArmsAnimator()
    {
        return armsAnimator;
    }

    public Transform GetArmsThrowLocation()
    {
        return throwLocation;
    }
}
