
using UnityEngine;

[System.Serializable]
public class Dialogue
{
    public string NPCName;

    [TextArea(3, 10)]
    public string[] FirstTimeMeetingSentences, genericMeetingSentences, TradeInitationSentences, CancelTradeSentences, AcceptTradeSentences, LeaveConversationSentences, relationshipFirstMilestoneSentences;
}
