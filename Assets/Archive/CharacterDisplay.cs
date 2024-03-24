using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplay : MonoBehaviour
{
    public int displayID;
    public CharacterObject character;

    public RawImage portrait;
    public Slider healthBar, manaBar;

    public ItemObject leftHand, rightHand;

    public void LinkToChar(CharacterObject charToLink)
    {
        character = charToLink;
        displayID = character.charID;
        SyncStats();
    }

    void SyncStats()
    {
        portrait.texture = character.charPortrait;
        healthBar.maxValue = character.maxHealth;
        healthBar.value = character.currentHealth;
        manaBar.maxValue = character.maxMana;
        manaBar.value = character.currentMana;
    }

    
}
