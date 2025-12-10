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
            currentlyOccupiedGridnode = spawnGridNode;
            spawnGridNode.SetOccupant(new GridNodeOccupant(gameObject, GridNodeOccupantType.NPC));
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

    public void TryDamage(int damage, DamageType damageType = DamageType.Standard)
    {
        if(!isDead)
        {
            if (!movementController.isTurning && !movementController.isMoving)
            {
                int rand = Random.Range(0, 100);
                if(rand <= hitReactionChance)
                    animController.PlayAnimation("HitReaction", 0, Random.Range(0, spawnedNPCs.Count));
            }

            currentGroupHealth -= damage;

            floatingTextController.SpawnDamageText(damage, damageType);
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

    public void AddStatusEffect(StatusEffect statusEffectToAdd)
    {
        //switch (statusEffectToAdd.effectType)
        //{
        //    case StatusEffectType.Fire:
        //        if(fireDamageCoroutine != null)
        //            StopCoroutine(fireDamageCoroutine);

        //        fireDamageCoroutine = StartCoroutine(HelperFunctions.TakeDamageOverTime(this, statusEffectToAdd));
        //        break;
        //    case StatusEffectType.Acid:
        //        int acidArmourReduction = 20;
        //        if (armourReductionCoroutine != null)
        //        {
        //            StopCoroutine(armourReductionCoroutine);
        //            currentArmourRating = NPCData.baseArmourRating;
        //        }

        //        armourReductionCoroutine = StartCoroutine(ReduceArmourRating(statusEffectToAdd.effectLength, acidArmourReduction));

        //        if(acidDamageCoroutine != null)
        //            StopCoroutine(acidDamageCoroutine);

        //        acidDamageCoroutine = StartCoroutine(HelperFunctions.TakeDamageOverTime(this, statusEffectToAdd));
        //        break;
        //}
        //statusEffectToAdd.ApplyStatusEffect(this);
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
