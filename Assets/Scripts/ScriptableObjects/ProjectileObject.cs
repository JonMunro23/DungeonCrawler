using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "Projectile Object")]
public class ProjectileObject : ScriptableObject
{

    public GameObject projModel;
    public int minDamage;
    public int maxDamage;
    public int speed;
    public string spawnLocationObjectTag;

    public bool isAffectedByGravity;
    public bool hasOwnDamage;

    public bool destroyOnContact;
    public bool isRigidbodyInParent;

}

