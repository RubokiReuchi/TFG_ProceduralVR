using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MutantOrb : Enemy
{
    Transform playerHead;
    [Header("Orb")]
    [SerializeField] float fireRate;
    [SerializeField] GameObject rayPrefab;
    [SerializeField] Material growingRingMat;

    void Start()
    {
        playerHead = GameObject.FindGameObjectWithTag("PlayerHead").transform;
        //material = new Material(originalMaterial);
        //foreach (var materialGO in materialGameObjects) materialGO.GetComponent<MeshRenderer>().material = material;
        Color ringColor = growingRingMat.GetColor("_TintColor");
        growingRingMat.SetColor("_TintColor", new Color(ringColor.r, ringColor.g, ringColor.b, 0.45f));

        currentHealth = maxHealth;
    }
}
