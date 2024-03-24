using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDisplays : MonoBehaviour
{
    [SerializeField]
    CharacterObject[] characters;
    [SerializeField]
    GameObject characterDisplay;
    [SerializeField]
    GameObject[] displays;
    [SerializeField]
    Transform charDisplayGrid;

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            GameObject charDisplay = Instantiate(characterDisplay, transform.position, Quaternion.identity);
            charDisplay.transform.SetParent(charDisplayGrid);
            charDisplay.GetComponent<CharacterDisplay>().LinkToChar(characters[i]);
            charDisplay.transform.localScale = new Vector3(1, 1, 1);
        }
        displays = GameObject.FindGameObjectsWithTag("CharacterDisplay");
    }

    public void UpdateHealthBars(int charID)
    {
        for (int i = 0; i < displays.Length; i++)
        {
            if(charID == displays[i].GetComponent<CharacterDisplay>().displayID)
            {
                displays[i].GetComponent<CharacterDisplay>().healthBar.value = displays[i].GetComponent<CharacterDisplay>().character.currentHealth;
            }
        }
    }

}
