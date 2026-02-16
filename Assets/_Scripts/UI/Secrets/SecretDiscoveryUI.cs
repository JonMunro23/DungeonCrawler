using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretDiscoveryUI : MonoBehaviour
{
    [SerializeField] TMP_Text experienceText, secretFoundText;
    [SerializeField] Image dividingLine;
    [SerializeField] float fadeInDuration = .2f, fadeOutDuration = .5f, displayDuration = 2f;

    public void ShowUI(int experienceValue)
    {
        SetExperienceAmount(experienceValue);
        secretFoundText.DOFade(1, fadeInDuration);
        dividingLine.DOFade(1, fadeInDuration);
        experienceText.DOFade(1, fadeInDuration).OnComplete(() =>
        {
            StartCoroutine(BeginDisplayCountdown());
        });
    }

    void HideUI()
    {
        secretFoundText.DOFade(0, fadeOutDuration);
        dividingLine.DOFade(0, fadeOutDuration);
        experienceText.DOFade(0, fadeOutDuration);
    }

    void SetExperienceAmount(int experienceAmount) => experienceText.text = $"+{experienceAmount.ToString()}XP";

    IEnumerator BeginDisplayCountdown()
    {
        yield return new WaitForSeconds(displayDuration);
        HideUI();
    }
}
