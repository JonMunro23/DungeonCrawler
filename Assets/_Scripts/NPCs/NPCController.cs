using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Random = UnityEngine.Random;
using System.Collections;

[SelectionBase]
public class NPCController : MonoBehaviour, IDamageable
{
    public int levelIndex;

    [HideInInspector] public NPCAnimationController animController;
    [HideInInspector] public NPCMovementController movementController;
    [HideInInspector] public NPCAttackController attackController;
    [HideInInspector] public NPCFloatingTextController floatingTextController;

    [Header("References")]
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
    [SerializeField] int currentArmourRating;
    [SerializeField] int currentEvasionRating;
    bool isDead => currentGroupHealth <= 0;

    [Header("Item Dropping")]
    public List<ItemData> guaranteedDrops = new List<ItemData>();
    public List<ItemData> randomDrops = new List<ItemData>();

    public static Action<NPCController> onNPCDeath;
    Coroutine fireDamageCoroutine, acidDamageCoroutine, armourReductionCoroutine;

    private void Awake()
    {
        movementController = GetComponent<NPCMovementController>();
        animController = GetComponent<NPCAnimationController>();
        attackController = GetComponent<NPCAttackController>();
        floatingTextController = GetComponent<NPCFloatingTextController>();
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
        InitStats();
    }

    private void InitStats()
    {
        currentGroupHealth = maxGroupHealth;
        currentArmourRating = NPCData.baseArmourRating;
        currentEvasionRating = NPCData.baseEvasionRating;
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

    void SpawnNPCModels()
    {
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

    public void TryDamage(bool wasHit, int damage, DamageType damageType = DamageType.Standard, bool wasCrit = false)
    {
        if(!isDead)
        {
            if(!wasHit)
            {
                floatingTextController.SpawnDamageText(wasHit, damage, damageType, wasCrit);
                return;
            }

            if (!movementController.isTurning && !movementController.isMoving)
            {
                int rand = Random.Range(0, 100);
                if(rand <= hitReactionChance)
                    animController.PlayAnimation("HitReaction", 0, Random.Range(0, spawnedNPCs.Count));
            }

            currentGroupHealth -= damage;

            floatingTextController.SpawnDamageText(wasHit, damage, damageType, wasCrit);
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
                currentlyOccupiedGridnode.ResetOccupant();
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

    

    public DamageData GetDamageData()
    {
        return new DamageData(Mathf.RoundToInt(currentGroupHealth), currentArmourRating, currentEvasionRating);
    }

    public void AddStatusEffect(StatusEffectType statusEffectTypeToAdd, float duration = 5)
    {
        switch (statusEffectTypeToAdd)
        {
            case StatusEffectType.Fire:
                if(fireDamageCoroutine != null)
                    StopCoroutine(fireDamageCoroutine);

                fireDamageCoroutine = StartCoroutine(TakeDamageOverTime(duration, 20, 60, .45f, DamageType.Fire));
                break;
            case StatusEffectType.Acid:
                int acidArmourReduction = 20;
                if (armourReductionCoroutine != null)
                {
                    StopCoroutine(armourReductionCoroutine);
                    currentArmourRating = NPCData.baseArmourRating;
                }

                armourReductionCoroutine = StartCoroutine(ReduceArmourRating(duration, acidArmourReduction));

                if(acidDamageCoroutine != null)
                    StopCoroutine(acidDamageCoroutine);

                acidDamageCoroutine = StartCoroutine(TakeDamageOverTime(duration, 15, 40, .6f, DamageType.Acid));
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
    IEnumerator TakeDamageOverTime(float duration, int minDamage, int maxDamage, float damageIntervals, DamageType damageType)
    {
        float timeElapsed = 0;
        float interval = 0;

        while (timeElapsed < duration)
        {
            if (timeElapsed >= interval)
            {
                TryDamage(true, Random.Range(minDamage, maxDamage), damageType);
                interval += damageIntervals;
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ReduceArmourRating(float duration, int reductionAmount)
    {
        currentArmourRating -= reductionAmount;
        if (currentArmourRating < 0)
            currentArmourRating = 0;

        yield return new WaitForSeconds(duration);

        currentArmourRating = NPCData.baseArmourRating;

    }
}
