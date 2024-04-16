using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;



public class Enemy : MonoBehaviour
{
    public COIN type;
    [SerializeField] protected int coinAmount;
    [HideInInspector] public bool alive = true;
    [HideInInspector] public bool firstEnable = true;
    protected NavMeshAgent agent;
    [SerializeField] protected float maxHealth;
    [NonEditable][SerializeField] protected float currentHealth;
    [SerializeField] protected LayerMask foundationsLayers;
    [SerializeField] protected GameObject[] materialGameObjects;
    [SerializeField] protected Material originalMaterial;
    protected Material material;
    protected Material material2;

    [Header("Shield")]
    [NonEditable] public bool hasShield = false;
    [HideInInspector] public EnemyShield shield = null;


    [Header("Freeze")]
    [SerializeField] protected float freezeResistance;
    protected float freezePercentage = 0;
    protected float freezeDuration = 10; // secs
    protected float freezedTime; // secs
    protected float recoverDelay = 2; // secs
    protected float recoverTime; // secs
    protected float invulneravilityTime = 0; // secs, Used to prevent the instant unfreeze when hitting the enemy
    protected float freezeSlow = 0;
    protected float defaultSpeed = 0;
    protected bool freezeApplied = false;
    [SerializeField] protected GameObject iceBlocksParticlesPrefab;
    [SerializeField] protected Transform iceBlocksParticlesSpawn;

    [Header("AreaDamage")]
    List<string> activeUUIDs = new();

    private void LateUpdate()
    {
        if (freezePercentage == 100)
        {
            invulneravilityTime -= Time.deltaTime;
            freezedTime -= Time.deltaTime + Time.deltaTime * freezeResistance / 25.0f;
            if (freezedTime < 0)
            {
                freezedTime = 0;
                freezePercentage = 0;
                material.SetFloat("_FreezeInterpolation", 0);
                if (material2 != null) material2.SetFloat("_FreezeInterpolation", 0);
            }
        }
        else if (freezePercentage > 0)
        {
            if (recoverTime > 0)
            {
                recoverTime -= Time.deltaTime;
                if (recoverTime < 0) recoverTime = 0;
            }
            else if (recoverTime == 0)
            {
                freezePercentage -= (Time.deltaTime + Time.deltaTime * freezeResistance / 25.0f) * 20.0f;
                if (freezePercentage < 0) freezePercentage = 0;
                material.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
                if (material2 != null) material2.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
            }
        }
    }

    public virtual void StartCheckOptions()
    {

    }

    public virtual void TakeDamage(float amount, GameObject damageText = null)
    {

    }

    public void TakeAreaDamage(float amount, string UUID, GameObject damageText = null)
    {
        if (activeUUIDs.Contains(UUID)) return;
        else
        {
            activeUUIDs.Add(UUID);
            TakeDamage(amount, damageText);
        }
    }

    public virtual void TakeFreeze(float amount)
    {

    }

    public void TakeAreaFreeze(float amount, string UUID)
    {
        if (activeUUIDs.Contains(UUID)) return;
        else
        {
            activeUUIDs.Add(UUID);
            TakeFreeze(amount);
        }
    }

    public virtual void TakeHeal(float amount)
    {

    }

    public virtual void Die()
    {

    }

    protected void GiveCoin()
    {
        if (type == COIN.BIOMATTER) PlayerSkills.instance.biomatter += coinAmount;
        else PlayerSkills.instance.gear += coinAmount;
    }
}
