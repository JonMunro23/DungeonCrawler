using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Enemies/New Enemy")]
public class NPCData : ScriptableObject
{
    public int health;
    public int damage;
    public int experienceValue;

    public GameObject prefab;
}
