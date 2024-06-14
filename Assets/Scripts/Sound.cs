using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    public enum MIXER
    {
        MUSIC,
        SFX,
        ENVIROMENT
    }

    public string name;

    public AudioClip clip;
    public MIXER mixer;

    public bool loop = false;
    [Range(0.0f, 1.0f)] public float volume = 1.0f;
    [Range(-3.0f, 3.0f)] public float pitch = 1.0f;
    public float fadeOutSpeed = 0.0f;

    [HideInInspector] public AudioSource source;

    public void InitSound()
    {
        source.clip = clip;
        source.loop = loop;
        source.volume = volume;
        source.pitch = pitch;
        source.playOnAwake = false;
    }
}
