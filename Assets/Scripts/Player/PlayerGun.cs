using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public enum GUN_TYPE
{
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
    LASER,
    TRIPLE
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

    [NonEditable][SerializeField] GUN_TYPE selectedGun;
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
    [Header("Laser")]
    [SerializeField] GameObject yellowLaserPrefab;
    [SerializeField] GameObject blueLaserPrefab;
    [SerializeField] GameObject redLaserPrefab;
    [SerializeField] GameObject purpleLaserPrefab;
    [SerializeField] GameObject greenLaserPrefab;
    [SerializeField] float laserCadence;
    [SerializeField] float laserStartDelay;
    float laserCd = 0;
    [Header("Triple")]
    [SerializeField] Transform projectileOrigin2Start;
    [SerializeField] Transform projectileOrigin2Current;
    [SerializeField] Transform projectileOrigin3Start;
    [SerializeField] Transform projectileOrigin3Current;
    PlayerCharged chargedProjectile2 = null;
    PlayerCharged chargedProjectile3 = null;

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
            case PROJECTILE_TYPE.LASER:
                laserCd -= Time.deltaTime;
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    laserCd = laserStartDelay;
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    if (laserCd <= 0)
                    {
                        GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                        laserCd = 1 / laserCadence;
                    }
                }
                break;
            case PROJECTILE_TYPE.TRIPLE:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOrigin2Current.position, projectileOrigin2Current.rotation);
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOrigin3Current.position, projectileOrigin3Current.rotation);
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
                            projectileOrigin2Current.position = projectileOrigin2Start.position + projectileOrigin2Current.forward * holdTime / 2.0f;
                            projectileOrigin3Current.position = projectileOrigin3Start.position + projectileOrigin3Current.forward * holdTime / 2.0f;
                        }
                        if (!chargedProjectile)
                        {
                            chargedProjectile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile.SetUp();
                            chargedProjectile2 = GameObject.Instantiate(selectedChargedPrefab, projectileOrigin2Current.position, projectileOrigin2Current.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile2.SetUp();
                            chargedProjectile3 = GameObject.Instantiate(selectedChargedPrefab, projectileOrigin3Current.position, projectileOrigin3Current.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile3.SetUp();
                        }
                        if (holdTime > maxIncrease) holdTime = maxIncrease;
                        chargedProjectile.Increase(new Vector3(holdTime, holdTime, holdTime), projectileOriginCurrent.position);
                        chargedProjectile2.Increase(new Vector3(holdTime, holdTime, holdTime), projectileOrigin2Current.position);
                        chargedProjectile3.Increase(new Vector3(holdTime, holdTime, holdTime), projectileOrigin3Current.position);
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
                        chargedProjectile2.SetDamage((repeatTime - 0.5f) * increaseSpeed, 0, maxIncrease);
                        chargedProjectile2.Launch(projectileOrigin2Current.rotation);
                        chargedProjectile2 = null;
                        projectileOrigin2Current.position = projectileOrigin2Start.position;
                        chargedProjectile3.SetDamage((repeatTime - 0.5f) * increaseSpeed, 0, maxIncrease);
                        chargedProjectile3.Launch(projectileOrigin3Current.rotation);
                        chargedProjectile3 = null;
                        projectileOrigin3Current.position = projectileOrigin3Start.position;
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
        switch (newType)
        {
            case GUN_TYPE.YELLOW:
                if (projectileType != PROJECTILE_TYPE.LASER)
                {
                    selectedProjectilePrefab = yellowProjectilePrefab;
                    selectedChargedPrefab = yellowChargedPrefab;
                }
                else
                {
                    selectedProjectilePrefab = yellowLaserPrefab;
                }
                break;
            case GUN_TYPE.BLUE:
                if (projectileType != PROJECTILE_TYPE.LASER)
                {
                    selectedProjectilePrefab = blueProjectilePrefab;
                }
                else
                {
                    selectedProjectilePrefab = blueLaserPrefab;
                } 
                break;
            case GUN_TYPE.RED:
                if (projectileType != PROJECTILE_TYPE.LASER)
                {
                    selectedProjectilePrefab = redProjectilePrefab;
                    selectedChargedPrefab = redChargedPrefab;
                }
                else
                {
                    selectedProjectilePrefab = redLaserPrefab;
                }
                break;
            case GUN_TYPE.PURPLE:
                if (projectileType != PROJECTILE_TYPE.LASER)
                {
                    selectedProjectilePrefab = purpleProjectilePrefab;
                    selectedChargedPrefab = purpleChargedPrefab;
                }
                else
                {
                    selectedProjectilePrefab = purpleLaserPrefab;
                }
                break;
            case GUN_TYPE.GREEN:
                if (projectileType != PROJECTILE_TYPE.LASER)
                {
                    selectedProjectilePrefab = greenProjectilePrefab;
                    selectedChargedPrefab = greenChargedPrefab;
                }
                else
                {
                    selectedProjectilePrefab = greenLaserPrefab;
                }
                break;
        }
        screen.SetScreen(newType);
    }
}
