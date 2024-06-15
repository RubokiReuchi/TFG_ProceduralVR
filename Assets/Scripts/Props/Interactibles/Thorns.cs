using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thorns : Puzzle
{
    [SerializeField] bool isPartOfPuzzle;
    Animator animator;

    [SerializeField] GameObject thorns1;
    [SerializeField] GameObject thorns2;
    [SerializeField] Material originalGreenMaterial;
    [SerializeField] Material originalRedMaterial;
    Material greenMaterial;
    Material redMaterial;
    bool destroying = false;
    [SerializeField] AudioSource growSource;
    [SerializeField] AudioSource destroySource;

    void Start()
    {
        greenMaterial = new Material(originalGreenMaterial);
        redMaterial = new Material(originalRedMaterial);
        Material[] auxArray = { greenMaterial, redMaterial }; 
        thorns1.GetComponent<MeshRenderer>().materials = auxArray;
        thorns2.GetComponent<MeshRenderer>().materials = auxArray;
        greenMaterial.SetFloat("_DissolvePercentage", 0);
        redMaterial.SetFloat("_DissolvePercentage", 0);

        if (isPartOfPuzzle) animator = GetComponent<Animator>();
    }

    public override void StartPuzzle()
    {
        animator.enabled = true;
        GetComponent<BoxCollider>().enabled = true;
        growSource.Play();
    }

    public override void HitPuzzle(float damage, string projectileTag)
    {
        if (!destroying && projectileTag == "GreenProjectile")
        {
            StartCoroutine(Disintegrate());
            destroySource.Play();
        }
    }

    IEnumerator Disintegrate()
    {
        destroying = true;
        float dissolvePercentage = 0;
        while (dissolvePercentage < 1)
        {
            dissolvePercentage += Time.deltaTime;
            if (dissolvePercentage > 1) dissolvePercentage = 1;
            greenMaterial.SetFloat("_DissolvePercentage", dissolvePercentage);
            redMaterial.SetFloat("_DissolvePercentage", dissolvePercentage);
            yield return null;
        }
        Destroy(gameObject);
    }
}
