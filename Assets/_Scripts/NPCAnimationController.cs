using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    NPCGroupController groupController;

    [SerializeField] List<Animator> animators = new List<Animator>();
    [SerializeField] GameObject[] spawnedNPCs;

    public void Init(NPCGroupController _groupController)
    {
        groupController = _groupController;
        spawnedNPCs = groupController.spawnedNPCs.ToArray();

        foreach (GameObject NPC in spawnedNPCs)
        {
            animators.Add(NPC.GetComponent<Animator>());
        }
    }

    public void PlayAnimation(string animationName, float animationDuration = 0f)
    {
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
