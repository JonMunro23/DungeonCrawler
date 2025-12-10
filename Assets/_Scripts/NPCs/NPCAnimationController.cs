using System.Collections;
using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    NPCController controller;
    Animator animator;

    [SerializeField] GameObject npcMesh;

    public void Init(NPCController controller)
    {
        this.controller = controller;
        animator = GetComponentInChildren<Animator>();
    }

    public void PlayAnimation(string animationName, float animationDuration = 0f)
    {
        if (!animator)
            return;

        if(animationName == "TurnLeft")
        {
            StartCoroutine(LerpNPCRot(npcMesh, npcMesh.transform.localRotation, npcMesh.transform.localRotation * Quaternion.Euler(-Vector3.up * 90), animationDuration));
        }
        else if (animationName == "TurnRight")
        {
            StartCoroutine(LerpNPCRot(npcMesh, npcMesh.transform.localRotation, npcMesh.transform.localRotation * Quaternion.Euler(Vector3.up * 90), animationDuration));
        }
        else
            animator.Play(animationName);

    }

    IEnumerator LerpNPCRot(GameObject NPC, Quaternion startRot, Quaternion endRot, float lerpDuration)
    {
        float timeElapsed = 0;

        while (NPC && timeElapsed < lerpDuration)
        {
            float t = timeElapsed / lerpDuration;
            NPC.transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if(NPC)
            NPC.transform.localRotation = endRot;
    }
}
