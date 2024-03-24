using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "Consumable Item Object")]
public class ConsumableItemObject : ItemObject
{
    public enum ConsumableType
    {
        healing,
        buff,

    };
    [Header("Consumable Properties")]
    public ConsumableType consumableType;
    public int replenishAmount;
    public string resourceToReplenish;

    public enum BuffType
    {
        Protection,
        StatIncrease,
        AttackUp
    };
    public BuffType buffType;

}

