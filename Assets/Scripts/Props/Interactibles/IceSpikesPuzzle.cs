using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpikesPuzzle : Puzzle
{
    Animator animator;
    public IceSpike[] allSpikes;
    public Transform invisibleWall;
    public ParticleSystem meltPs;
    Vector3 targetScale;
    [SerializeField] AudioSource landSource;
    [SerializeField] AudioSource meltSource;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        targetScale = transform.localScale;
    }

    void Update()
    {
        if (targetScale.y < invisibleWall.localScale.y)
        {
            invisibleWall.localScale -= Vector3.up * Time.deltaTime * 0.2f;
        }
        if (invisibleWall.localScale.y <= 0)
        {
            invisibleWall.gameObject.SetActive(false);
        }
    }

    public override void StartPuzzle()
    {
        animator.enabled = true;
    }

    public override void HitPuzzle(float damage, string projectileTag)
    {
        if (projectileTag == "RedProjectile")
        {
            foreach (IceSpike spike in allSpikes) spike.Melt(damage);
            meltPs.Play();
            targetScale -= Vector3.up * damage / 100.0f;
            if (targetScale.y < 0.4f) targetScale = Vector3.zero;
            meltSource.Play();
        }
    }

    public void PlayLandSound()
    {
        landSource.Play();
    }
}
