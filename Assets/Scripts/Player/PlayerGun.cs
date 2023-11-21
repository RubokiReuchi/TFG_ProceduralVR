using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using UnityEngine.XR.Interaction.Toolkit;

public enum GUN_TYPE
{
    NULL,
    YELLOW,
    BLUE,
    RED,
    PURPLE,
    GREEN
}

public enum PROJECTILE_TYPE
{
    NORMAL,
    AUTOMATIC,
    TRIPLE,
    MISILE
}

public enum TRIGGER_STATE
{
    IDLE,
    DOWN,
    REPEAT,
    UP
}

public class PlayerGun : MonoBehaviour
{
    [SerializeField] ActionBasedController controller;
    TRIGGER_STATE triggerState;
    float repeatTime;

    GUN_TYPE selectedGun = GUN_TYPE.YELLOW;
    [SerializeField] Transform projectileOriginStart;
    [SerializeField] Transform projectileOriginCurrent;
    GameObject selectedProjectilePrefab;
    GameObject selectedChargedPrefab;

    HandScreen screen;

    [Header("Charged Projectile")]
    [SerializeField] float increaseSpeed;
    [SerializeField] float maxIncrease;
    PlayerCharged chargedProjectile = null;

    [Header("Projectiles Prefabs")]
    [SerializeField] GameObject yellowProjectilePrefab;
    [SerializeField] GameObject blueProjectilePrefab;
    [SerializeField] GameObject redProjectilePrefab;
    [SerializeField] GameObject purpleProjectilePrefab;
    [SerializeField] GameObject greenProjectilePrefab;
    [Header("Charged Projectiles Prefabs")]
    [SerializeField] GameObject yellowChargedPrefab;
    [SerializeField] GameObject blueChargedPrefab;
    [SerializeField] GameObject redChargedPrefab;
    [SerializeField] GameObject purpleChargedPrefab;
    [SerializeField] GameObject greenChargedPrefab;

    [Header("Projectile Type")]
    [NonEditable] public PROJECTILE_TYPE projectileType = PROJECTILE_TYPE.NORMAL;
    [Header("Automatic")]
    [SerializeField] float automaticCadence;
    float automaticCd = 0;
    [Header("Triple")]
    [SerializeField] Transform projectileOrigin2Start;
    [SerializeField] Transform projectileOrigin3Start;
    [Header("Misile")]
    [SerializeField] GameObject yellowMisilePrefab;
    [SerializeField] GameObject blueMisilePrefab;
    [SerializeField] GameObject redMisilePrefab;
    [SerializeField] GameObject purpleMisilePrefab;
    [SerializeField] GameObject greenMisilePrefab;
    [SerializeField] GameObject yellowSuperMisilePrefab;
    [SerializeField] GameObject blueSuperMisilePrefab;
    [SerializeField] GameObject redSuperMisilePrefab;
    [SerializeField] GameObject purpleSuperMisilePrefab;
    [SerializeField] GameObject greenSuperMisilePrefab;
    [SerializeField] float superMisileTime;
    [SerializeField] ParticleSystem superMisileChargingPs;
    [SerializeField] VisualEffect superMisileReady;
    bool superMisileCharged = false;

    // Start is called before the first frame update
    void Start()
    {
        triggerState = TRIGGER_STATE.IDLE;
        selectedProjectilePrefab = yellowProjectilePrefab;
        selectedChargedPrefab = yellowChargedPrefab;

        screen = GetComponent<HandScreen>();
    }

    // Update is called once per frame
    void Update()
    {
        CalculateTriggerState();

        switch (projectileType)
        {
            case PROJECTILE_TYPE.NORMAL:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (repeatTime >= 0.5f)
                    {
                        float holdTime = (repeatTime - 0.5f) * increaseSpeed;
                        if (holdTime < maxIncrease) projectileOriginCurrent.position = projectileOriginStart.position + projectileOriginCurrent.forward * holdTime / 2.0f;
                        if (!chargedProjectile)
                        {
                            chargedProjectile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile.SetUp();
                        }
                        if (holdTime > maxIncrease) holdTime = maxIncrease;
                        chargedProjectile.Increase(new Vector3(holdTime, holdTime, holdTime), projectileOriginCurrent.position);
                    }
                }
                else if (triggerState == TRIGGER_STATE.UP)
                {
                    if (repeatTime >= 0.5f && chargedProjectile)
                    {
                        chargedProjectile.SetDamage((repeatTime - 0.5f) * increaseSpeed, 0, maxIncrease);
                        chargedProjectile.Launch(projectileOriginCurrent.rotation);
                        chargedProjectile = null;
                        projectileOriginCurrent.position = projectileOriginStart.position;
                    }
                    repeatTime = 0;
                }
                break;
            case PROJECTILE_TYPE.AUTOMATIC:
                automaticCd -= Time.deltaTime;
                if (triggerState == TRIGGER_STATE.DOWN || triggerState == TRIGGER_STATE.REPEAT)
                {
                    if (automaticCd <= 0)
                    {
                        GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                        automaticCd = 1 / automaticCadence;
                    }
                }
                break;
            case PROJECTILE_TYPE.TRIPLE:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOrigin2Start.position, projectileOrigin2Start.rotation);
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOrigin3Start.position, projectileOrigin3Start.rotation);
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (repeatTime >= 0.5f)
                    {
                        float holdTime = (repeatTime - 0.5f) * increaseSpeed;
                        if (holdTime < maxIncrease)
                        {
                            projectileOriginCurrent.position = projectileOriginStart.position + projectileOriginCurrent.forward * holdTime / 2.0f;
                        }
                        if (!chargedProjectile)
                        {
                            chargedProjectile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile.SetUp();
                        }
                        if (holdTime > maxIncrease) holdTime = maxIncrease;
                        chargedProjectile.Increase(new Vector3(holdTime, holdTime, holdTime), projectileOriginCurrent.position);
                    }
                }
                else if (triggerState == TRIGGER_STATE.UP)
                {
                    if (repeatTime >= 0.5f && chargedProjectile)
                    {
                        chargedProjectile.SetDamage((repeatTime - 0.5f) * increaseSpeed, 0, maxIncrease);
                        chargedProjectile.Launch(projectileOriginCurrent.rotation);
                        chargedProjectile = null;
                        projectileOriginCurrent.position = projectileOriginStart.position;
                    }
                    repeatTime = 0;
                }
                break;
            case PROJECTILE_TYPE.MISILE:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (superMisileCharged) return;
                    if (!superMisileChargingPs.isPlaying && repeatTime >= 0.5f) superMisileChargingPs.Play();
                    if (repeatTime >= superMisileTime)
                    {
                        superMisileChargingPs.Stop();
                        if (!superMisileReady.enabled) superMisileReady.enabled = true;
                        else superMisileReady.Play();
                        superMisileCharged = true;
                    }
                }
                else if (triggerState == TRIGGER_STATE.UP)
                {
                    if (superMisileChargingPs.isPlaying) superMisileChargingPs.Stop();
                    if (repeatTime >= superMisileTime)
                    {
                        GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                        superMisileCharged = false;
                    }
                    else if (repeatTime >= 0.5f)
                    {
                        GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                    }
                    repeatTime = 0;
                }
                break;
        }
    }

    void CalculateTriggerState()
    {
        switch (triggerState)
        {
            case TRIGGER_STATE.IDLE:
                if (controller.activateActionValue.action.ReadValue<float>() >= 0.5f) triggerState = TRIGGER_STATE.DOWN;
                break;
            case TRIGGER_STATE.DOWN:
            case TRIGGER_STATE.REPEAT:
                if (controller.activateActionValue.action.ReadValue<float>() >= 0.5f) triggerState = TRIGGER_STATE.REPEAT;
                else triggerState = TRIGGER_STATE.UP;
                break;
            case TRIGGER_STATE.UP:
                if (controller.activateActionValue.action.ReadValue<float>() >= 0.5f) triggerState = TRIGGER_STATE.DOWN;
                else triggerState = TRIGGER_STATE.IDLE;
                break;
        }
    }

    public void SwapGunType(GUN_TYPE newType)
    {
        if (newType == GUN_TYPE.NULL) newType = selectedGun;
        switch (newType)
        {
            case GUN_TYPE.YELLOW:
                switch (projectileType)
                {
                    case PROJECTILE_TYPE.NORMAL:
                    case PROJECTILE_TYPE.AUTOMATIC:
                    case PROJECTILE_TYPE.TRIPLE:
                        selectedProjectilePrefab = yellowProjectilePrefab;
                        selectedChargedPrefab = yellowChargedPrefab;
                        break;
                    case PROJECTILE_TYPE.MISILE:
                        selectedProjectilePrefab = yellowMisilePrefab;
                        selectedChargedPrefab = yellowSuperMisilePrefab;
                        break;
                }
                break;
            case GUN_TYPE.BLUE:
                switch (projectileType)
                {
                    case PROJECTILE_TYPE.NORMAL:
                    case PROJECTILE_TYPE.AUTOMATIC:
                    case PROJECTILE_TYPE.TRIPLE:
                        selectedProjectilePrefab = blueProjectilePrefab;
                        selectedChargedPrefab = blueChargedPrefab;
                        break;
                    case PROJECTILE_TYPE.MISILE:
                        selectedProjectilePrefab = blueMisilePrefab;
                        selectedChargedPrefab = blueSuperMisilePrefab;
                        break;
                }
                break;
            case GUN_TYPE.RED:
                switch (projectileType)
                {
                    case PROJECTILE_TYPE.NORMAL:
                    case PROJECTILE_TYPE.AUTOMATIC:
                    case PROJECTILE_TYPE.TRIPLE:
                        selectedProjectilePrefab = redProjectilePrefab;
                        selectedChargedPrefab = redChargedPrefab;
                        break;
                    case PROJECTILE_TYPE.MISILE:
                        selectedProjectilePrefab = redMisilePrefab;
                        selectedChargedPrefab = redSuperMisilePrefab;
                        break;
                }
                break;
            case GUN_TYPE.PURPLE:
                switch (projectileType)
                {
                    case PROJECTILE_TYPE.NORMAL:
                    case PROJECTILE_TYPE.AUTOMATIC:
                    case PROJECTILE_TYPE.TRIPLE:
                        selectedProjectilePrefab = purpleProjectilePrefab;
                        selectedChargedPrefab = purpleChargedPrefab;
                        break;
                    case PROJECTILE_TYPE.MISILE:
                        selectedProjectilePrefab = purpleMisilePrefab;
                        selectedChargedPrefab = purpleSuperMisilePrefab;
                        break;
                }
                break;
            case GUN_TYPE.GREEN:
                switch (projectileType)
                {
                    case PROJECTILE_TYPE.NORMAL:
                    case PROJECTILE_TYPE.AUTOMATIC:
                    case PROJECTILE_TYPE.TRIPLE:
                        selectedProjectilePrefab = greenProjectilePrefab;
                        selectedChargedPrefab = greenChargedPrefab;
                        break;
                    case PROJECTILE_TYPE.MISILE:
                        selectedProjectilePrefab = greenMisilePrefab;
                        selectedChargedPrefab = greenSuperMisilePrefab;
                        break;
                }
                break;
        }
        selectedGun = newType;
        screen.SetScreen(newType);
    }
}
