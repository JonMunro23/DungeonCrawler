using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCGroupController : MonoBehaviour, IDamageable
{

    [HideInInspector] public NPCAnimationController animController;
    [HideInInspector] public NPCMovementController movementController;

    [Header("References")]
    [SerializeField] GameObject damageTakenFloatingText;
    [SerializeField] Transform floatingTextSpawnLocation;
    [SerializeField] Transform centerSpawnPoint;
    [SerializeField] Transform[] spawnPoints;

    [Header("Grid Data")]
    public GridNode currentlyOccupiedGridnode;

    [Header("Group Data")]
    public NPCData NPCToSpawn;
    public int amountToSpawnInStack;
    public List<GameObject> spawnedNPCs = new List<GameObject>();
    bool isDead;

    [Header("Group Stats")]
    public float currentGroupHealth;
    public float maxGroupHealth;

    [Header("Item Dropping")]
    public List<ItemData> guaranteedDrops = new List<ItemData>();
    public List<ItemData> randomDrops = new List<ItemData>();


    private void Awake()
    {
        movementController = GetComponent<NPCMovementController>();
        animController = GetComponent<NPCAnimationController>();
    }

    public void InitGroup(GridNode spawnGridNode)
    {
        currentlyOccupiedGridnode = spawnGridNode;

        SpawnNPCs();
        InitControllers();
    }

    private void SpawnNPCs()
    {
        currentGroupHealth = maxGroupHealth;
        if (amountToSpawnInStack > 1)
        {
            for (int i = 0; i < amountToSpawnInStack; i++)
            {
                SpawnEnemies(NPCToSpawn, spawnPoints[i]);
            }
        }
        else
        {
            SpawnEnemies(NPCToSpawn, centerSpawnPoint);
        }
        currentGroupHealth = maxGroupHealth;
    }

    void InitControllers()
    {
        movementController.Init(this);
        animController.Init(this);
    }

    public void SpawnEnemies(NPCData enemyTypeToSpawn, Transform spawnLocation)
    {
        spawnedNPCs.Add(Instantiate(enemyTypeToSpawn.prefab, spawnLocation.position, spawnLocation.rotation, spawnLocation));
        maxGroupHealth += enemyTypeToSpawn.health;
        
    }

    public void TakeDamage(int damage, bool wasCrit = false)
    {
        if(!isDead)
        {
            currentGroupHealth -= damage;
            SpawnFloatingText(damage, wasCrit);
            float remainingEnemies = currentGroupHealth / maxGroupHealth * amountToSpawnInStack;
            int roundedEnemyCount = Mathf.CeilToInt(remainingEnemies);

            if(roundedEnemyCount < spawnedNPCs.Count)
            {
                int difference = spawnedNPCs.Count - roundedEnemyCount;
                int randIndex = Random.Range(0, difference);
                animController.RemoveNPCsAnimator(spawnedNPCs[randIndex]);
                Destroy(spawnedNPCs[randIndex]);
                spawnedNPCs.RemoveAt(randIndex);
            }

            if (currentGroupHealth <= 0)
            {
                isDead = true;
                if(guaranteedDrops.Count > 0) 
                {
                    foreach (ItemData drop in guaranteedDrops)
                    {
                        Instantiate(drop.itemWorldModel, transform.position, Quaternion.identity);
                    }

                }
                Destroy(gameObject);
            }
        }
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
}
