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

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (targetScale.x < invisibleWall.localScale.x)
        {
            invisibleWall.localScale -= Vector3.up * Time.deltaTime * 0.2f;
        }
        if (invisibleWall.localScale.x <= 0)
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
        }
    }
}
