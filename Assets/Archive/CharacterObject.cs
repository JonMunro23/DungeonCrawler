using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Object")]
public class CharacterObject : ScriptableObject
{
    public int charID;
    public string charName;
    public string charRace;
    public string charClass;
    public Texture charPortrait;

    public int charLevel;

    public int maxHealth;
    public int currentHealth;

    public int maxMana;
    public int currentMana;

    public int maxHunger;
    public int currentHunger;

    public int maxExperience;
    public int currentExperience;
}
