using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abilites : MonoBehaviour
{
    PlayerHealthController playerHealth;
    PlayerMana playerMana;

    [SerializeField]
    ProjectileData projectileFireball;

    bool canUsePlayerClassAbility = true, canUseCompanion1Ability = true, canUseCompanion2Ability = true;
    
    public int playerClassAbilityManaCost;

    [SerializeField]
    GameObject playerClassAbilityCooldownImage, companion1AbilityCooldownImage, companion2AbilityCooldownImage;

    Transform projectileSpawnLocation;

    public int playerClassAbilityCooldown, companion1AbilityCooldown, companion2AbilityCooldown;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealthController>();
        playerMana = GetComponent<PlayerMana>();
    }

    // Start is called before the first frame update
    void Start()
    {
        projectileSpawnLocation = GameObject.FindGameObjectWithTag(projectileFireball.spawnLocationObjectTag).transform;
    }

    
}
