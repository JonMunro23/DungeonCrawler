using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMana : MonoBehaviour
{
    [SerializeField]
    Slider playerManaSlider;
    [SerializeField]
    TMP_Text playerManaText;
    public int currentPlayerMana, maxPlayerMana;
    public float currentManaRegenCooldown, baseManaRegenCooldown;

    float manaRegenBuffDuration;

    bool canRegenMana = true;

    private void Start()
    {
        playerManaSlider.maxValue = maxPlayerMana;
        currentPlayerMana = maxPlayerMana;
        playerManaSlider.value = currentPlayerMana;
        playerManaText.text = currentPlayerMana.ToString() + " / " + maxPlayerMana.ToString();
        currentManaRegenCooldown = baseManaRegenCooldown;
    }

    private void Update()
    {
        RegenMana();
    }

    public void ConsumeMana(int consumeAmount)
    {
        currentPlayerMana -= consumeAmount;
        playerManaSlider.value = currentPlayerMana;
        playerManaText.text = currentPlayerMana.ToString() + " / " + maxPlayerMana.ToString();
        if (currentPlayerMana <= 0)
        {
            Debug.Log("Game Over");
        }
    }

    public void ReplenishMana(int replenishAmount)
    {
        currentPlayerMana += replenishAmount;
        if (currentPlayerMana > maxPlayerMana)
        {
            currentPlayerMana = maxPlayerMana;
        }
        playerManaSlider.value = currentPlayerMana;
        playerManaText.text = currentPlayerMana.ToString() + " / " + maxPlayerMana.ToString();
    }

    public void IncreaseManaRegen(float divisionFactor, float duration)
    {
        currentManaRegenCooldown = currentManaRegenCooldown / divisionFactor;
        manaRegenBuffDuration = duration;
        StartCoroutine(ManaRegenBuffDuration());
    }

    IEnumerator ManaRegenBuffDuration()
    {
        yield return new WaitForSeconds(manaRegenBuffDuration);
        currentManaRegenCooldown = baseManaRegenCooldown;
    }

    void RegenMana()
    {
        if(currentPlayerMana < maxPlayerMana)
        {
            if (canRegenMana == true)
            {
                //Debug.Log(currentPlayerMana + " beans?");
                canRegenMana = false;
                currentPlayerMana++;
                playerManaSlider.value = currentPlayerMana;
                playerManaText.text = currentPlayerMana.ToString() + " / " + maxPlayerMana.ToString();
                StartCoroutine(ManaRegenCooldown());
            }
        }
    }

    IEnumerator ManaRegenCooldown()
    {
        yield return new WaitForSeconds(currentManaRegenCooldown);
        if(currentPlayerMana < maxPlayerMana)
        {
            canRegenMana = true;
        }
    }
}
