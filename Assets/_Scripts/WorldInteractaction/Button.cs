using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{

    [SerializeField]
    GameObject[] objectsToInteractWith;
    bool isInMotion;
    float timeElapsed;
    [SerializeField]
    float pushAnimDuration;
    [SerializeField]
    Vector3 animStartingPos, animPushedPos;
    [SerializeField]
    AnimationCurve pushAnimCurve;

    void Update()
    {
        if (isInMotion)
        {
            if (timeElapsed < pushAnimDuration)
            {
                float t = timeElapsed / pushAnimDuration;
                t = pushAnimCurve.Evaluate(t);
                transform.position = Vector3.Lerp(animStartingPos, animPushedPos, t);
                
                timeElapsed += Time.deltaTime;
            }
            else
            {
                transform.position = animStartingPos;
                isInMotion = false;
                timeElapsed = 0;
            }

        }
    }

    void PushButton()
    {
        foreach (GameObject item in objectsToInteractWith)
        {
            IInteractable interactive = item.GetComponent<IInteractable>();
            if (interactive != null)
            {
                interactive.Interact();
            }
        }

        isInMotion = true;
    }

    public void Interact()
    {
        Debug.Log("Pushing Button");
        PushButton();
    }
}
