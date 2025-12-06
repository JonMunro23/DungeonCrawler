using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour, IDamageable, IGridNode
{
    [Header("Stats")]
    [SerializeField] List<DamageType> susceptibleDamageTypes = new List<DamageType>();
    [SerializeField] int health;
    [SerializeField] int armourRating;


    GridNode occupyingNode;
    int levelIndex;

    public void AddStatusEffect(StatusEffect statusEffectToAdd)
    {
        //Cannot have status effects applied
    }

    public Vector2 GetCoords()
    {
        return occupyingNode.Coords.Pos;
    }

    public DamageData GetDamageData()
    {
        return new DamageData(health, armourRating);
    }

    public int GetLevelIndex()
    {
        return levelIndex;
    }

    public void SetLevelIndex(int _levelIndex)
    {
        levelIndex = _levelIndex;
    }

    public void SetOccupyingNode(GridNode occupyingNode)
    {
        this.occupyingNode = occupyingNode;
    }

    public void TryDamage(int damageTaken, DamageType damageType = DamageType.Standard)
    {
        if (!susceptibleDamageTypes.Contains(damageType))
            return;

        occupyingNode.ResetOccupant();
        Destroy(gameObject);
    }
}
