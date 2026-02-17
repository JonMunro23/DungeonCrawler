using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIndicator : MonoBehaviour
{
    [SerializeField] Image effectImage;
    public void Init(StatusEffectData effectToIndicate)
    {
        effectImage.sprite = effectToIndicate.effectSprite;
    }
}
