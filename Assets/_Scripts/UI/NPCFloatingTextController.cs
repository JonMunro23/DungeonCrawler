using TMPro;
using UnityEngine;
using DG.Tweening;

public class NPCFloatingTextController : MonoBehaviour
{
    [SerializeField] GameObject damageTakenFloatingText;
    [SerializeField] Transform floatingTextSpawnLocation;

    [SerializeField] float textSpeed = .25f;
    [SerializeField] float textDelayBeforeFade = 1f;
    [SerializeField] float textFadeDuration = 1f;

    [Header("Spawn Variation Limits")]
    [SerializeField] float xMinMaxLimits = .5f;
    [SerializeField] float yMinMaxLimits = .35f;
    [SerializeField] float zMinMaxLimits = .5f;

    [Header("Text Colours")]
    [SerializeField] Color defaultDamageTextColour;
    [SerializeField] Color critDamageTextColour;
    [SerializeField] Color fireDamageTextColour;
    [SerializeField] Color acidDamageTextColour;

    public void SpawnDamageText(int damage, DamageType damageType = DamageType.Standard)
    {
        GameObject textClone = Instantiate(damageTakenFloatingText, RandomiseFloatingTextSpawnLocation(), transform.rotation);
        textClone.GetComponent<FloatingDamageText>().SetUpwardsSpeed(textSpeed);

        TMP_Text cloneTextComponent = textClone.GetComponentInChildren<TMP_Text>();

        switch (damageType)
        {
            case DamageType.Fire:
                cloneTextComponent.color = fireDamageTextColour;
                break;
            case DamageType.Acid:
                cloneTextComponent.color = acidDamageTextColour;
                break;
        }

        //if (wasCrit)
        //{
        //    cloneTextComponent.color = critDamageTextColour;
        //    cloneTextComponent.fontSize += .10f;
        //}
        cloneTextComponent.text = damage.ToString();

        StartTextRemoval(textClone, cloneTextComponent);
    }

    void StartTextRemoval(GameObject textClone, TMP_Text cloneTextComponent)
    {
        cloneTextComponent.DOFade(0, textFadeDuration).SetDelay(textDelayBeforeFade).OnComplete(() =>
        {
            Destroy(textClone);
        });
    }

    Vector3 RandomiseFloatingTextSpawnLocation()
    {
        float xVariation = Random.Range(-xMinMaxLimits, xMinMaxLimits);
        float yVariation = Random.Range(-yMinMaxLimits, yMinMaxLimits);
        float zVariation = Random.Range(-zMinMaxLimits, zMinMaxLimits);

        Vector3 newPos = floatingTextSpawnLocation.position + new Vector3(xVariation, yVariation, zVariation);
        return newPos;

    }
}
