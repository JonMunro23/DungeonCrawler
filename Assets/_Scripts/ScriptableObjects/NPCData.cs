using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "NPCs/New NPCData")]
public class NPCData : ScriptableObject
{
    public string identifier;
    public GameObject prefab;
    public Vector2 minMaxGroupSize = new Vector2(1,1);

    [Header("Stats")]
    public int experienceValue;
    public int health = 100;
    public int baseArmourRating;
    public int baseEvasionRating;

    [Header("Attacking")]
    public Vector2 minMaxDamage = new Vector2(25, 50);
    public float attackCooldown;
    public float delayBetweenAttacks;
    public float delayBeforeDamageDealt;
    public bool isRanged;
    public int attackRange;

    [Header("Movement")]
    public float moveDuration = 1;
    public float minDelayBetweenMovement = 0;
    public float turnDuration = .6f;
    public float minDelayBetweenTurning = .1f;

    [Header("SFx")]
    public AudioClip[] attackSFx;
    public AudioClip[] walkSFX;
    public AudioClip turnSFX;
    public AudioClip deathSFX;
}
