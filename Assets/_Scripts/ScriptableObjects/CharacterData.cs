using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharData/New Character Data")]
public class CharacterData : ScriptableObject
{
    public string charName;
    public int maxHealth;
    public Sprite charPortrait;
}
