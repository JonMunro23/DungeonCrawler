using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile Object", menuName = "Enemies/New Enemy")]

public class EnemyData : ScriptableObject
{
    public int health;
    public int damage;
    public int experienceValue;

    public GameObject prefab;
}
