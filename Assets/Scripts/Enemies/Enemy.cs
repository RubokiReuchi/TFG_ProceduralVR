using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public bool firstEnable = true;
    protected NavMeshAgent agent;
    Rigidbody rb;
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] protected LayerMask foundationsLayers;

    // crawl control
    [HideInInspector] public bool horizontalPushed = false;
    [HideInInspector] public bool verticalPushed = false;
    [HideInInspector] public bool bouncyPushed = false;
    bool giantPushed = false;
    [HideInInspector] public bool onCC = false;

    [NonEditable] public bool hasShield = false;
    [HideInInspector] public EnemyShield shield = null;

    public virtual void StartCheckOptions()
    {

    }

    public virtual void TakeDamage(float amount)
    {

    }

    public virtual void Die()
    {

    }
}
