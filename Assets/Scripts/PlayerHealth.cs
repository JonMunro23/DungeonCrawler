using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    Slider playerHealthSlider;
    [SerializeField]
    TMP_Text playerHealhText, playerArmourText, playerEvasionText;
    public int currentPlayerHealth, maxPlayerHealth;

    public int currentArmourRating, currentEvasionRating;

    private void Start()
    {
        playerHealthSlider.maxValue = maxPlayerHealth;
        currentPlayerHealth = maxPlayerHealth;
        playerHealthSlider.value = currentPlayerHealth;
        playerHealhText.text = currentPlayerHealth.ToString() + " / " + maxPlayerHealth.ToString();

    }

    public void IncreaseArmour(int increaseAmount)
    {
        currentArmourRating += increaseAmount;
        playerArmourText.text = currentArmourRating.ToString();

    }

    public void IncreaseEvasion(int increaseAmount)
    {
        currentEvasionRating += increaseAmount;
        playerEvasionText.text = currentEvasionRating.ToString();
    }

    public void DecreaseArmour(int decreaseAmount)
    {
        currentArmourRating -= decreaseAmount;
        playerArmourText.text = currentArmourRating.ToString();

    }

    public void DecreaseEvasion(int decreaseAmount)
    {
        currentEvasionRating -= decreaseAmount;
        playerEvasionText.text = currentEvasionRating.ToString();
    }

    public void TakeDamage(int damage)
    {
        int rand = Random.Range(0, 101);
        if (rand > currentEvasionRating)
        {
            currentPlayerHealth -= (damage - currentArmourRating);
            playerHealthSlider.value = currentPlayerHealth;
            playerHealhText.text = currentPlayerHealth.ToString() + " / " + maxPlayerHealth.ToString();
            if (currentPlayerHealth <= 0)
            {
                Debug.Log("Game Over");
            }
        }else
        {
            //attack dodged
            Debug.Log("Attack dodged");
        }
    }

    public void Heal(int healAmount)
    {
        currentPlayerHealth += healAmount;
        if(currentPlayerHealth > maxPlayerHealth)
        {
            currentPlayerHealth = maxPlayerHealth;
        }
        playerHealthSlider.value = currentPlayerHealth;
        playerHealhText.text = currentPlayerHealth.ToString() + " / " + maxPlayerHealth.ToString();
    }
}
