using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilites : MonoBehaviour
{
    PlayerHealth playerHealth;
    PlayerMana playerMana;

    [SerializeField]
    ProjectileObject projectileFireball;

    bool canUsePlayerClassAbility = true, canUseCompanion1Ability = true, canUseCompanion2Ability = true;
    
    public int playerClassAbilityManaCost;

    [SerializeField]
    GameObject playerClassAbilityCooldownImage, companion1AbilityCooldownImage, companion2AbilityCooldownImage;

    Transform projectileSpawnLocation;

    public int playerClassAbilityCooldown, companion1AbilityCooldown, companion2AbilityCooldown;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMana = GetComponent<PlayerMana>();
    }

    // Start is called before the first frame update
    void Start()
    {
        projectileSpawnLocation = GameObject.FindGameObjectWithTag(projectileFireball.spawnLocationObjectTag).transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Alpha1) && canUsePlayerClassAbility == true)
        {
            if(playerMana.currentPlayerMana > playerClassAbilityManaCost)
            {
                UsePlayerClassAbility();
            }
        }
        if (Input.GetKey(KeyCode.Alpha2) && canUseCompanion1Ability == true)
        {
            UseCompanion1Ability();
        }
        if (Input.GetKey(KeyCode.Alpha3) && canUseCompanion2Ability == true)
        {
            UseCompanion2Ability();
        }
    }

    void UsePlayerClassAbility()
    {
        canUsePlayerClassAbility = false;

        playerClassAbilityCooldownImage.SetActive(true);

        playerMana.ConsumeMana(playerClassAbilityManaCost);

        //cast fireball
        GameObject fireballClone = Instantiate(projectileFireball.projModel, projectileSpawnLocation.position, projectileSpawnLocation.rotation);
        fireballClone.GetComponent<Projectile>().projectile = projectileFireball;

        StartCoroutine(PlayerClassAbilityCooldown());
    }

    void UseCompanion1Ability()
    {
        canUseCompanion1Ability = false;

        companion1AbilityCooldownImage.SetActive(true);

        //heal player
        playerHealth.Heal(50);

        StartCoroutine(Companion1AbilityCooldown());
    }

    void UseCompanion2Ability()
    {
        canUseCompanion2Ability = false;

        companion2AbilityCooldownImage.SetActive(true);

        //increase mana regen from 2 to .5 for 10 seconds
        playerMana.IncreaseManaRegen(4, 10);

        StartCoroutine(Companion2AbilityCooldown());
    }

    IEnumerator PlayerClassAbilityCooldown()
    {
        yield return new WaitForSeconds(playerClassAbilityCooldown);
        playerClassAbilityCooldownImage.SetActive(false);
        canUsePlayerClassAbility = true;
    }

    IEnumerator Companion1AbilityCooldown()
    {
        yield return new WaitForSeconds(companion1AbilityCooldown);
        companion1AbilityCooldownImage.SetActive(false);
        canUseCompanion1Ability = true;
    }

    IEnumerator Companion2AbilityCooldown()
    {
        yield return new WaitForSeconds(companion2AbilityCooldown);
        companion2AbilityCooldownImage.SetActive(false);
        canUseCompanion2Ability = true;
    }
}
