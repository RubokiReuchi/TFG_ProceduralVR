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
    missile
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
    [HideInInspector] public bool blueUnlocked = false;
    [HideInInspector] public bool redUnlocked = false;
    [HideInInspector] public bool purpleUnlocked = false;
    [HideInInspector] public bool greenUnlocked = false;

    HandScreen screen;

    [Header("Charged Projectile")]
    [SerializeField] float increaseSpeed;
    [SerializeField] float maxIncrease;
    PlayerCharged chargedProjectile = null;
    [NonEditable] public bool shockwaveObtained = false;

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
    [Header("missile")]
    [SerializeField] GameObject yellowmissilePrefab;
    [SerializeField] GameObject bluemissilePrefab;
    [SerializeField] GameObject redmissilePrefab;
    [SerializeField] GameObject purplemissilePrefab;
    [SerializeField] GameObject greenmissilePrefab;
    [SerializeField] GameObject yellowSupermissilePrefab;
    [SerializeField] GameObject blueSupermissilePrefab;
    [SerializeField] GameObject redSupermissilePrefab;
    [SerializeField] GameObject purpleSupermissilePrefab;
    [SerializeField] GameObject greenSupermissilePrefab;
    [SerializeField] float supermissileTime;
    [SerializeField] ParticleSystem supermissileChargingPs;
    [SerializeField] VisualEffect supermissileReady;
    bool supermissileCharged = false;

    // Start is called before the first frame update
    void Start()
    {
        triggerState = TRIGGER_STATE.IDLE;
        selectedProjectilePrefab = yellowProjectilePrefab;
        selectedChargedPrefab = yellowChargedPrefab;

        screen = GetComponent<HandScreen>();

        // Skills Unlocked
        blueUnlocked = PlayerSkills.instance.blueUnlocked;
        redUnlocked = PlayerSkills.instance.redUnlocked;
        purpleUnlocked = PlayerSkills.instance.purpleUnlocked;
        greenUnlocked = PlayerSkills.instance.greenUnlocked;
        SwapGunType(GUN_TYPE.NULL);
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
                        chargedProjectile.Launch(projectileOriginCurrent.rotation, shockwaveObtained);
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
                        chargedProjectile.Launch(projectileOriginCurrent.rotation, shockwaveObtained);
                        chargedProjectile = null;
                        projectileOriginCurrent.position = projectileOriginStart.position;
                    }
                    repeatTime = 0;
                }
                break;
            case PROJECTILE_TYPE.missile:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (supermissileCharged) return;
                    if (!supermissileChargingPs.isPlaying && repeatTime >= 0.5f) supermissileChargingPs.Play();
                    if (repeatTime >= supermissileTime)
                    {
                        supermissileChargingPs.Stop();
                        if (!supermissileReady.enabled) supermissileReady.enabled = true;
                        else supermissileReady.Play();
                        supermissileCharged = true;
                    }
                }
                else if (triggerState == TRIGGER_STATE.UP)
                {
                    if (supermissileChargingPs.isPlaying) supermissileChargingPs.Stop();
                    if (repeatTime >= supermissileTime)
                    {
                        GameObject supermissile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                        if (shockwaveObtained) supermissile.GetComponent<PlayerSupermissile>().AddShockwave();
                        supermissileCharged = false;
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

        if (newType == GUN_TYPE.BLUE && !blueUnlocked) return;
        else if (newType == GUN_TYPE.RED && !redUnlocked) return;
        else if (newType == GUN_TYPE.PURPLE && !purpleUnlocked) return;
        else if (newType == GUN_TYPE.YELLOW && greenUnlocked) newType = GUN_TYPE.GREEN;

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
                    case PROJECTILE_TYPE.missile:
                        selectedProjectilePrefab = yellowmissilePrefab;
                        selectedChargedPrefab = yellowSupermissilePrefab;
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
                    case PROJECTILE_TYPE.missile:
                        selectedProjectilePrefab = bluemissilePrefab;
                        selectedChargedPrefab = blueSupermissilePrefab;
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
                    case PROJECTILE_TYPE.missile:
                        selectedProjectilePrefab = redmissilePrefab;
                        selectedChargedPrefab = redSupermissilePrefab;
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
                    case PROJECTILE_TYPE.missile:
                        selectedProjectilePrefab = purplemissilePrefab;
                        selectedChargedPrefab = purpleSupermissilePrefab;
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
                    case PROJECTILE_TYPE.missile:
                        selectedProjectilePrefab = greenmissilePrefab;
                        selectedChargedPrefab = greenSupermissilePrefab;
                        break;
                }
                break;
        }
        selectedGun = newType;
        screen.SetScreen(newType);
    }

    public void UpgradeToGreen()
    {
        if (selectedGun != GUN_TYPE.YELLOW) return;

        SwapGunType(GUN_TYPE.GREEN);
    }
}
