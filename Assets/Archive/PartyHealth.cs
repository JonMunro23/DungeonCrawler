using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyHealth : MonoBehaviour
{
    public CharacterObject[] characters;
    [SerializeField]
    CharacterDisplays characterDisplays;
    //direction[1 = forward, 2 = left, 3 = back, 4 = right]

    private void Start()
    {
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(1, 1);
        }

    }

    public void TakeDamage(int damage, int direction)
    {
        if(direction == 1)
        {
            //Front 2 characters take damage
            int[] charsToHit = { 0, 1 };
            int rand = Random.Range(0, 2);
            for (int i = 0; i < characters.Length; i++)
            {
                if(characters[i].charID == charsToHit[rand])
                {
                    Debug.Log(characters[i].charName +"  "+ characters[i].charID);
                    characters[i].currentHealth -= damage;
                    characterDisplays.UpdateHealthBars(characters[i].charID);
                    Debug.Log("meme");

                }
            }
        }
        else if(direction == 2)
        {
            //left 2 characters take damage
            int[] charsToHit = { 0, 2 };
            int rand = Random.Range(0, 2);
        }
        else if(direction == 3)
        {
            //back 2 characters take damage
            int[] charsToHit = { 2, 3 };
            int rand = Random.Range(0, 2);
        }
        else if(direction == 4)
        {
            //right 2 characters take damage
            int[] charsToHit = { 1, 3 };
            int rand = Random.Range(0, 2);

        }
    }
}
