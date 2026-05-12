using UnityEngine;

public class PlayerStatusEffectsAudioController : MonoBehaviour
{
    AudioEmitter emitter;

    private void OnEnable()
    {
        PlayerStatusEffectManager.onStatusEffectAdded += OnStatusEffectAdded;
        PlayerStatusEffectManager.onStatusEffectEnded += OnStatusEffectEnded;
    }

    private void OnDisable()
    {
        PlayerStatusEffectManager.onStatusEffectAdded -= OnStatusEffectAdded;
        PlayerStatusEffectManager.onStatusEffectEnded -= OnStatusEffectEnded;
    }

    private void Start()
    {
        emitter = AudioManager.Instance.RegisterSource("[AudioEmitter] StatusEffects", transform.root, spatialBlend: 0);

    }

    void OnStatusEffectAdded(StatusEffectData addedStatusEffect)
    {
        PlayStatusEffectAudio(addedStatusEffect);
    }
    void OnStatusEffectEnded(StatusEffectData endedStatusEffect)
    {
        StopStatusEffectAudio();
    }

    void PlayStatusEffectAudio(StatusEffectData effectData)
    {
        if(effectData.effectSFX != null)
            emitter.PlayLooped(effectData.effectSFX, .5f);
    }

    void StopStatusEffectAudio()
    {
        emitter.Stop();
    }

}
