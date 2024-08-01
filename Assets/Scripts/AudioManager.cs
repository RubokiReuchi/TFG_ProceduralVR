using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [SerializeField] AudioMixerGroup musicMixer;
    [SerializeField] AudioMixerGroup sfxMixer;
    [SerializeField] AudioMixerGroup projectileMixer;
    [SerializeField] AudioMixerGroup enviromentMixer;
    [SerializeField] Sound[] sounds;
    [SerializeField] SoundArray[] soundArrays;

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
                case Sound.MIXER.PROJECTILE:
                    sound.source.outputAudioMixerGroup = projectileMixer;
                    break;
                case Sound.MIXER.ENVIROMENT:
                    sound.source.outputAudioMixerGroup = enviromentMixer;
                    break;
                default:
                    break;
            }
        }

        foreach (var soundArray in soundArrays)
        {
            soundArray.source = gameObject.AddComponent<AudioSource>();
            soundArray.InitSound();
            switch (soundArray.mixer)
            {
                case Sound.MIXER.MUSIC:
                    soundArray.source.outputAudioMixerGroup = musicMixer;
                    break;
                case Sound.MIXER.SFX:
                    soundArray.source.outputAudioMixerGroup = sfxMixer;
                    break;
                case Sound.MIXER.PROJECTILE:
                    soundArray.source.outputAudioMixerGroup = projectileMixer;
                    break;
                case Sound.MIXER.ENVIROMENT:
                    soundArray.source.outputAudioMixerGroup = enviromentMixer;
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

    public void PlaySoundArray(string soundArrayName)
    {
        SoundArray soundArray = Array.Find(soundArrays, soundArrays => soundArrays.name == soundArrayName);
        soundArray.source.clip = soundArray.clips[UnityEngine.Random.Range(0, soundArray.clips.Length)];
        soundArray.source.Play();
    }
}