using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public class NPCController : MonoBehaviour, IDamageable
{
    public int levelIndex;

    [HideInInspector] public NPCAnimationController animController;
    [HideInInspector] public NPCMovementController movementController;
    [HideInInspector] public NPCAttackController attackController;

    [Header("References")]
    [SerializeField] GameObject damageTakenFloatingText;
    [SerializeField] Transform floatingTextSpawnLocation;
    [SerializeField] Transform centerSpawnPoint;
    [SerializeField] Transform[] spawnPoints;
    public AudioSource audioSource;

    [Header("Grid Data")]
    public GridNode currentlyOccupiedGridnode;

    [Header("Group Data")]
    public int amountToSpawnInStack;
    [SerializeField] int hitReactionChance;
    [HideInInspector] public List<GameObject> spawnedNPCs = new List<GameObject>();
    public NPCData NPCData { get; private set; }

    [Header("Group Stats")]
    public float currentGroupHealth;
    public float maxGroupHealth;
    bool isDead => currentGroupHealth <= 0;

    [Header("Item Dropping")]
    public List<ItemData> guaranteedDrops = new List<ItemData>();
    public List<ItemData> randomDrops = new List<ItemData>();

    public static Action<NPCController> onNPCDeath;

    private void Awake()
    {
        movementController = GetComponent<NPCMovementController>();
        animController = GetComponent<NPCAnimationController>();
        attackController = GetComponent<NPCAttackController>();
        audioSource = GetComponent<AudioSource>();
    }

    public void InitNPC(int _levelIndex, NPCData npcData, GridNode spawnGridNode = null)
    {
        levelIndex = _levelIndex;
        NPCData = npcData;

        if(spawnGridNode != null)
        {
            //spawnGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.NPC));
            currentlyOccupiedGridnode = spawnGridNode;
        }

        SpawnNPCModels();
        InitControllers();
    }

    public void SetNPCHealth(int newHealthValue)
    {
        currentGroupHealth = newHealthValue;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SnapToNode(GridNode node)
    {
        movementController.SnapToNode(node);
    }

    void SnapToRotation(float newRot)
    {
        movementController.SnapToRotation(newRot);

    }

    private void SpawnNPCModels()
    {
        currentGroupHealth = maxGroupHealth;
        if (amountToSpawnInStack > 1)
        {
            for (int i = 0; i < amountToSpawnInStack; i++)
            {
                SpawnNPCs(NPCData, spawnPoints[i]);
            }
        }
        else
        {
            SpawnNPCs(NPCData, centerSpawnPoint);
        }
        currentGroupHealth = maxGroupHealth;
    }

    void InitControllers()
    {
        if(movementController)
            movementController.Init(this);

        if(animController)
            animController.Init(this);

        if(attackController)
            attackController.Init(this);
    }

    public void SpawnNPCs(NPCData enemyTypeToSpawn, Transform spawnLocation)
    {
        spawnedNPCs.Add(Instantiate(enemyTypeToSpawn.prefab, spawnLocation.position, spawnLocation.rotation, spawnLocation));
        maxGroupHealth += enemyTypeToSpawn.health;
        
    }

    public void TryDamage(int damage, bool wasCrit = false)
    {
        if(!isDead)
        {
            if(!movementController.isTurning && !movementController.isMoving)
            {
                int rand = Random.Range(0, 100);
                if(rand <= hitReactionChance)
                    animController.PlayAnimation("HitReaction", 0, Random.Range(0, spawnedNPCs.Count));
            }
            currentGroupHealth -= damage;
            SpawnFloatingText(damage, wasCrit);
            float remainingEnemies = currentGroupHealth / maxGroupHealth * amountToSpawnInStack;
            int roundedEnemyCount = Mathf.CeilToInt(remainingEnemies);

            if(roundedEnemyCount < spawnedNPCs.Count)
            {
                int difference = spawnedNPCs.Count - roundedEnemyCount;
                int randIndex = Random.Range(0, difference);
                animController.RemoveNPCsAnimator(spawnedNPCs[0]);
                foreach (GameObject npc in spawnedNPCs)
                {
                    Destroy(npc);
                }
                spawnedNPCs.RemoveAt(0);
            }

            if (currentGroupHealth <= 0)
            {
                if(guaranteedDrops.Count > 0) 
                {
                    foreach (ItemData drop in guaranteedDrops)
                    {
                        Instantiate(drop.itemWorldModel, transform.position, Quaternion.identity);
                    }

                }
                currentlyOccupiedGridnode.ClearOccupant();
                onNPCDeath?.Invoke(this);
                Destroy(gameObject);
            }
        }
    }

    public void TryAttack()
    {
        if (attackController.CheckForPlayer())
        {
            attackController.TryAttack();
        }
        else
            movementController.FindNewPathToPlayer();
    }

    void SpawnFloatingText(int damage, bool wasCrit = false)
    {
        GameObject textClone = Instantiate(damageTakenFloatingText, RandomiseFloatingTextSpawnLocation(), transform.rotation);
        if (wasCrit)
        {
            textClone.GetComponentInChildren<TMP_Text>().color = Color.red;
            textClone.GetComponentInChildren<TMP_Text>().fontSize += .10f;
        }
        textClone.GetComponentInChildren<TMP_Text>().text = damage.ToString();
        Destroy(textClone, Random.Range(.9f, 1f));
    }

    Vector3 RandomiseFloatingTextSpawnLocation()
    {
        float xVariation = Random.Range(-.7f, .7f);
        float yVariation = Random.Range(-.7f, .7f);

        Vector3 newPos = floatingTextSpawnLocation.position + new Vector3(xVariation, yVariation, floatingTextSpawnLocation.localPosition.z);
        return newPos;

    }

    public DamageData GetDamageData()
    {
        return new DamageData(Mathf.RoundToInt(currentGroupHealth), 10, 5);
    }

    public void AddStatusEffect(StatusEffectType statusEffectTypeToAdd, float duration = 5)
    {
        switch (statusEffectTypeToAdd)
        {
            case StatusEffectType.Fire:
                TakeDamageOverTime(duration, 20, 60, .45f);
                break;
            case StatusEffectType.Acid:
                //Reduce armour rating
                TakeDamageOverTime(duration, 15, 40, .6f);
                break;
        }
    }

    /// <summary>
    /// Deals damage to the entity at a set interval over a period of time
    /// </summary>
    /// <param name="duration">Duration of the damage over time effect.</param>
    /// <param name="minDamage">The minimum possible damage to take.</param>
    /// <param name="maxDamage">The maximum possible damage to take.</param>
    /// <param name="damageIntervals">The time between damage ticks.</param>
    IEnumerator TakeDamageOverTime(float duration, int minDamage, int maxDamage, float damageIntervals)
    {
        float timeElapsed = 0;
        float interval = 0;

        while (timeElapsed < duration)
        {
            if (timeElapsed >= interval)
            {
                TryDamage(Random.Range(minDamage, maxDamage));
                interval += damageIntervals;
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
