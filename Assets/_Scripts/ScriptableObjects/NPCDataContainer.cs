using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NPCDataContainer", menuName = "NPCs/New NPCData Container")]

public class NPCDataContainer : ScriptableObject
{
    public List<NPCData> NPCData = new List<NPCData>();

    public NPCData GetDataFromIdentifier(string identifier)
    {
        foreach (NPCData npc in NPCData)
        {
            if(npc.identifier == identifier)
            {
                return npc;
            }
        }

        return null;
    }
}
