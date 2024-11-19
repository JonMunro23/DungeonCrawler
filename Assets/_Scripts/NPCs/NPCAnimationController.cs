using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    NPCController groupController;

    [SerializeField] List<Animator> animators = new List<Animator>();

    public void Init(NPCController _groupController)
    {
        groupController = _groupController;

        foreach (GameObject NPC in groupController.spawnedNPCs)
        {
            animators.Add(NPC.GetComponent<Animator>());
        }
    }

    public void PlayAnimation(string animationName, float animationDuration = 0f, int npcIndex = -1)
    {
        if(npcIndex > -1)
        {
            animators[npcIndex].Play(animationName);
            return;
        }

        foreach (Animator animator in animators)
        {
            if (!animator)
                return;

            GameObject npc = animator.gameObject;

            if(animationName == "TurnLeft")
            {
                StartCoroutine(LerpNPCRot(npc, npc.transform.localRotation, npc.transform.localRotation * Quaternion.Euler(-Vector3.up * 90), animationDuration));
            }
            else if (animationName == "TurnRight")
            {
                StartCoroutine(LerpNPCRot(npc, npc.transform.localRotation, npc.transform.localRotation * Quaternion.Euler(Vector3.up * 90), animationDuration));
            }
            else
                animator.Play(animationName);
        }
    }

    public void RemoveNPCsAnimator(GameObject NPC)
    {
        if(animators.Contains(NPC.GetComponent<Animator>()))
            animators.Remove(NPC.GetComponent<Animator>());
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
