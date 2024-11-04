using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    GameObject dialogueMenu, playerDialogueOptions, tradeMenu, dialogueBox;

    [SerializeField]
    RawImage NPCPortrait;
    [SerializeField]
    TMP_Text NPCName, DialogueText, NPCTradeInventoryText;
    [SerializeField]
    Animator animator;

    [SerializeField]
    float textScrollSpeed;

    Queue<string> sentences;
    string currentSentence;

    NPC currentNPC;
    string currentConversation;

    public static bool isInDialogue;
    bool hasTextFinished = true;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue (NPC npc)
    {
        isInDialogue = true;
        currentNPC = npc;
        currentConversation = "meet";
        dialogueMenu.SetActive(true);
        NPCPortrait.texture = currentNPC.NPCPortraitTexture;
        NPCName.text = currentNPC.dialogue.NPCName;
        DialogueText.text = "";

        sentences.Clear();
        if(npc.isFirstTimeMeeting == true)
        {
            foreach (string sentence in currentNPC.dialogue.FirstTimeMeetingSentences)
            {
                sentences.Enqueue(sentence);
            }
            npc.isFirstTimeMeeting = false;
        }
        else if(npc.isFirstTimeMeeting == false)
        {
            foreach (string sentence in currentNPC.dialogue.genericMeetingSentences)
            {
                sentences.Enqueue(sentence);
            }
        }
        Invoke("DisplayNextSentence", 1);
    }

    public void InitiateTradeDialogue()
    {
        currentConversation = "open trade";
        playerDialogueOptions.SetActive(false);
        foreach (string sentence in currentNPC.dialogue.TradeInitationSentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void CancelTradeDialogue()
    {
        currentConversation = "cancel trade";
        CloseTradeMenu();
        foreach (string sentence in currentNPC.dialogue.CancelTradeSentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void AcceptTradeDialogue()
    {
        currentConversation = "accept trade";
        CloseTradeMenu();
        foreach (string sentence in currentNPC.dialogue.AcceptTradeSentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void EndDialogue()
    {
        currentConversation = "leave";
        playerDialogueOptions.SetActive(false);
        foreach (string sentence in currentNPC.dialogue.LeaveConversationSentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {

        if (hasTextFinished == true)
        {
            if (sentences.Count == 0)
            {
                if (currentConversation == "meet")
                {
                    DialogueText.text = "";
                    playerDialogueOptions.SetActive(true);
                }
                else if (currentConversation == "open trade")
                {
                    OpenTradeMenu();
                }
                else if (currentConversation == "cancel trade" || currentConversation == "accept trade")
                {
                    DialogueText.text = "";
                    playerDialogueOptions.SetActive(true);
                }
                else if (currentConversation == "leave")
                {
                    animator.SetBool("isOpen", false);
                    Invoke("CloseDialogueMenu", .55f);
                }
                return;
            }
            currentSentence = sentences.Dequeue();
            DialogueText.text = "";
            StopAllCoroutines();
            hasTextFinished = false;
            StartCoroutine(TypeSentence(currentSentence));
        }
        else if (hasTextFinished == false)
        {
            StopAllCoroutines();
            DialogueText.text = currentSentence;
            hasTextFinished = true;
        }
    }

    void OpenTradeMenu()
    {
        dialogueBox.SetActive(false);
        tradeMenu.SetActive(true);
        NPCTradeInventoryText.text = NPCName.text + "'s Inventory";
    }
    
    void CloseTradeMenu()
    {
        tradeMenu.SetActive(false);
        dialogueBox.SetActive(true);
    }

    IEnumerator TypeSentence(string sentence)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            DialogueText.text += letter;
            yield return new WaitForSeconds(textScrollSpeed);
        }
        hasTextFinished = true;
    }

    void CloseDialogueMenu()
    {
        dialogueMenu.SetActive(false);
        isInDialogue = false;
    }
}
