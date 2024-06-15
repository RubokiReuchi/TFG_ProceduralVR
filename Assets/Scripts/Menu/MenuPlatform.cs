using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlatform : MonoBehaviour
{
    [SerializeField] AudioSource platformSource;
    [SerializeField] AudioSource tubeSource;

    public void PlayPlatformSound()
    {
        platformSource.Play();
    }

    public void PlayTubeSound()
    {
        tubeSource.Play();
    }
}
