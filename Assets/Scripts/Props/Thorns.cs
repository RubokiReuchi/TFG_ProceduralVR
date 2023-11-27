using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Thorns : MonoBehaviour
{
    [SerializeField] bool isPartOfPuzzle;
    Animator animator;

    [SerializeField] GameObject thorns1;
    [SerializeField] GameObject thorns2;
    [SerializeField] Material originalGreenMaterial;
    [SerializeField] Material originalRedMaterial;
    Material greenMaterial;
    Material redMaterial;

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

    public void StartPuzzle()
    {
        animator.enabled = true;
    }

    public void Disintegrate()
    {
        StartCoroutine(DisintegrateCo());
    }

    IEnumerator DisintegrateCo()
    {
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
