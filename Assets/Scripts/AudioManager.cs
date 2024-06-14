using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [SerializeField] AudioMixerGroup musicMixer;
    [SerializeField] AudioMixerGroup sfxMixer;
    [SerializeField] AudioMixerGroup enviromentMixer;
    [SerializeField] Sound[] sounds;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;

        foreach (var sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.InitSound();
            switch (sound.mixer)
            {
                case Sound.MIXER.MUSIC:
                    sound.source.outputAudioMixerGroup = musicMixer;
                    break;
                case Sound.MIXER.SFX:
                    sound.source.outputAudioMixerGroup = sfxMixer;
                    break;
                case Sound.MIXER.ENVIROMENT:
                    sound.source.outputAudioMixerGroup = enviromentMixer;
                    break;
                default:
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySound(string soundName)
    {
        AudioSource source = Array.Find(sounds, sound => sound.name == soundName).source;
        source.Play();
    }

    public void StopSound(string soundName)
    {
        AudioSource source = Array.Find(sounds, sound => sound.name == soundName).source;
        if (source.isPlaying) source.Stop();
    }

    public void FadeOutSound(string soundName)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == soundName);
        if (sound.source.isPlaying) StartCoroutine(FadeOutSound_Co(sound.source, sound.fadeOutSpeed));
    }

    IEnumerator FadeOutSound_Co(AudioSource source, float fadeSpeed)
    {
        float initialVolume = source.volume;
        float volume = initialVolume;
        while (volume > 0.0f)
        {
            volume -= Time.deltaTime * fadeSpeed;
            if (volume < 0.0f) volume = 0.0f;
            source.volume = volume;
            yield return null;
        }
        source.Stop();
        source.volume = initialVolume;
    }
}
