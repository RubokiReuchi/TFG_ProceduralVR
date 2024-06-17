using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MUSIC_STATE
{
    MENU,
    LOBBY,
    OFF_COMBAT,
    ON_COMBAT,
    BOSS
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance { get; private set; }

    [SerializeField] MUSIC_STATE state;

    [SerializeField] float fadeTime;
    bool fading = false;
    [SerializeField] AudioClip mainMenuMusic;
    [SerializeField] AudioClip lobbyMusic;
    [SerializeField] AudioClip[] offBattleMusic;
    int offbattleIndex;
    [SerializeField] AudioClip[] onBattleMusic;
    int onbattleIndex;
    [SerializeField] AudioClip bossMusic;
    [SerializeField] AudioSource source1;
    [SerializeField] AudioSource source2;
    bool playingSource1 = true;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (state == MUSIC_STATE.MENU || fading) return;
        if (playingSource1 && source1.time > source1.clip.length - fadeTime)
        {
            StartCoroutine(FadeCurrentMusic());
        }
        else if (!playingSource1 && source2.time > source2.clip.length - fadeTime)
        {
            StartCoroutine(FadeCurrentMusic());
        }
    }

    IEnumerator FadeCurrentMusic()
    {
        fading = true;
        AudioClip nextClip;
        if (state == MUSIC_STATE.OFF_COMBAT)
        {
            int nextIndex = offbattleIndex;
            while (nextIndex == offbattleIndex) nextIndex = Random.Range(0, offBattleMusic.Length);
            nextClip = offBattleMusic[nextIndex];
            offbattleIndex = nextIndex;
        }
        else if (state == MUSIC_STATE.ON_COMBAT)
        {
            int nextIndex = onbattleIndex;
            while (nextIndex == onbattleIndex) nextIndex = Random.Range(0, onBattleMusic.Length);
            nextClip = onBattleMusic[nextIndex];
            onbattleIndex = nextIndex;
        }
        else
        {
            Debug.LogError("Logic Error");
            yield break;
        }

        if (playingSource1)
        {
            source2.clip = nextClip;
            source2.volume = 0;
            source2.Play();
            float progresion = 0.0f;
            while (progresion < 1.0f)
            {
                progresion += Time.deltaTime / fadeTime;
                if (progresion > 1.0f) progresion = 1.0f;
                source1.volume = 1.0f - progresion;
                source2.volume = progresion;
                yield return null;
            }
            source1.Stop();
            playingSource1 = false;
        }
        else
        {
            source1.clip = nextClip;
            source1.volume = 0;
            source1.Play();
            float progresion = 0.0f;
            while (progresion < 1.0f)
            {
                progresion += Time.deltaTime / fadeTime;
                if (progresion > 1.0f) progresion = 1.0f;
                source2.volume = 1.0f - progresion;
                source1.volume = progresion;
                yield return null;
            }
            source2.Stop();
            playingSource1 = true;
        }
        fading = false;
    }

    public void SwapTo(MUSIC_STATE newState)
    {
        AudioClip nextClip;
        switch (newState)
        {
            case MUSIC_STATE.LOBBY:
                nextClip = lobbyMusic;
                break;
            case MUSIC_STATE.OFF_COMBAT:
                int nextOffIndex = offbattleIndex;
                while (nextOffIndex == offbattleIndex) nextOffIndex = Random.Range(0, offBattleMusic.Length);
                nextClip = offBattleMusic[nextOffIndex];
                offbattleIndex = nextOffIndex;
                break;
            case MUSIC_STATE.ON_COMBAT:
                int nextOnIndex = onbattleIndex;
                while (nextOnIndex == onbattleIndex) nextOnIndex = Random.Range(0, onBattleMusic.Length);
                nextClip = onBattleMusic[nextOnIndex];
                onbattleIndex = nextOnIndex;
                break;
            case MUSIC_STATE.BOSS:
                nextClip = bossMusic;
                break;
            case MUSIC_STATE.MENU:
            default:
                Debug.LogError("Logic Error");
                return;
        }

        StartCoroutine(FadeOtherMusic(nextClip, fading));
        state = newState;
    }

    IEnumerator FadeOtherMusic(AudioClip nextClip, bool wasFading)
    {
        if (wasFading) yield return new WaitForSeconds(fadeTime);

        fading = true;
        if (playingSource1)
        {
            source2.clip = nextClip;
            source2.volume = 0;
            source2.Play();
            float progresion = 0.0f;
            while (progresion < 1.0f)
            {
                progresion += Time.deltaTime / fadeTime;
                if (progresion > 1.0f) progresion = 1.0f;
                source1.volume = 1.0f - progresion;
                source2.volume = progresion;
                yield return null;
            }
            source1.Stop();
            playingSource1 = false;
            if (state == MUSIC_STATE.MENU) source1.loop = false;
        }
        else
        {
            source1.clip = nextClip;
            source1.volume = 0;
            source1.Play();
            float progresion = 0.0f;
            while (progresion < 1.0f)
            {
                progresion += Time.deltaTime / fadeTime;
                if (progresion > 1.0f) progresion = 1.0f;
                source2.volume = 1.0f - progresion;
                source1.volume = progresion;
                yield return null;
            }
            source2.Stop();
            playingSource1 = true;
        }
        fading = false;
    }
}