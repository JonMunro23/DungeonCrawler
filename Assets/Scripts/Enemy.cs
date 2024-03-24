using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour, IDamageable
{
    public float maxEnemyHealth;
    public float currentEnemyHealth;

    [SerializeField]
    GameObject damageTakenFloatingText;
    [SerializeField]
    Transform floatingTextSpawnLocation;

    [SerializeField]
    Transform centerSpawnPoint;
    [SerializeField]
    Transform[] spawnPoints;

    List<GameObject> spawnedEnemies = new List<GameObject>();

    public List<ItemObject> randomDrops = new List<ItemObject>();
    public List<ItemObject> guaranteedDrops = new List<ItemObject>();

    public EnemyData enemyTypeToSpawn;
    public int amountToSpawnInStack;

    bool isDead;

    private void Start()
    {
        currentEnemyHealth = maxEnemyHealth;
        if (amountToSpawnInStack > 1)
        {
            for (int i = 0; i < amountToSpawnInStack; i++)
            {
                SpawnEnemies(enemyTypeToSpawn, spawnPoints[i]);
            }
        }
        else
        {
            SpawnEnemies(enemyTypeToSpawn, centerSpawnPoint);
        }
        currentEnemyHealth = maxEnemyHealth;
    }

    public void SpawnEnemies(EnemyData enemyTypeToSpawn, Transform spawnLocation)
    {
        spawnedEnemies.Add(Instantiate(enemyTypeToSpawn.prefab, spawnLocation));
        maxEnemyHealth += enemyTypeToSpawn.health;
        
    }

    public void TakeDamage(int damage, bool wasCrit = false)
    {
        if(!isDead)
        {
            currentEnemyHealth -= damage;
            SpawnFloatingText(damage, wasCrit);
            float remainingEnemies = currentEnemyHealth / maxEnemyHealth * amountToSpawnInStack;
            int roundedEnemyCount = Mathf.CeilToInt(remainingEnemies);

            if(roundedEnemyCount < spawnedEnemies.Count)
            {
                int difference = spawnedEnemies.Count - roundedEnemyCount;
                int randIndex = Random.Range(0, difference);
                Destroy(spawnedEnemies[randIndex]);
                spawnedEnemies.RemoveAt(randIndex);
            }

            if (currentEnemyHealth <= 0)
            {
                isDead = true;
                if(guaranteedDrops.Count > 0) 
                {
                    foreach (ItemObject drop in guaranteedDrops)
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
