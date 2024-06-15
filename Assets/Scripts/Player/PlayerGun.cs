using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    MISSILE
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
    [SerializeField] Material[] selectedMat;
    [SerializeField] MeshRenderer[] selectedTriangle;
    [SerializeField] Transform projectileOriginStart;
    [SerializeField] Transform projectileOriginCurrent;
    GameObject selectedProjectilePrefab;
    GameObject selectedChargedPrefab;
    GameObject selectedProjectileSecondaryPrefab;
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
    float chargeSpeedReduction = 0.0f;

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
    [Header("Secondary Projectiles Prefabs")]
    [SerializeField] GameObject yellowProjectileSecondaryPrefab;
    [SerializeField] GameObject blueProjectileSecondaryPrefab;
    [SerializeField] GameObject redProjectileSecondaryPrefab;
    [SerializeField] GameObject purpleProjectileSecondaryPrefab;
    [SerializeField] GameObject greenProjectileSecondaryPrefab;
    [Header("Triple")]
    [SerializeField] Transform projectileOrigin2Start;
    [SerializeField] Transform projectileOrigin3Start;
    [Header("Missile")]
    [SerializeField] GameObject yellowMissilePrefab;
    [SerializeField] GameObject blueMissilePrefab;
    [SerializeField] GameObject redMissilePrefab;
    [SerializeField] GameObject purpleMissilePrefab;
    [SerializeField] GameObject greenMissilePrefab;
    [SerializeField] GameObject yellowSuperMissilePrefab;
    [SerializeField] GameObject blueSuperMissilePrefab;
    [SerializeField] GameObject redSuperMissilePrefab;
    [SerializeField] GameObject purpleSuperMissilePrefab;
    [SerializeField] GameObject greenSuperMissilePrefab;
    [SerializeField] float superMissileTime = 2.0f;
    [SerializeField] ParticleSystem superMissileChargingPs;
    [SerializeField] VisualEffect superMissileReady;
    bool superMissileCharged = false;

    [Header("Audio")]
    AudioManager audioManager;
    bool playingChargeSound = false;

    // Start is called before the first frame update
    void Start()
    {
        triggerState = TRIGGER_STATE.IDLE;
        selectedProjectilePrefab = yellowProjectilePrefab;
        selectedChargedPrefab = yellowChargedPrefab;

        screen = GetComponent<HandScreen>();

        // Skills Unlocked
        PlayerSkills skills = PlayerSkills.instance;
        blueUnlocked = skills.blueUnlocked;
        redUnlocked = skills.redUnlocked;
        purpleUnlocked = skills.purpleUnlocked;
        greenUnlocked = skills.greenUnlocked;
        SwapGunType(GUN_TYPE.NULL);
        if (greenUnlocked) selectedTriangle[0].material = selectedMat[0];
        if (blueUnlocked) selectedTriangle[1].material = selectedMat[1];
        if (redUnlocked) selectedTriangle[2].material = selectedMat[2];
        if (purpleUnlocked) selectedTriangle[3].material = selectedMat[3];

        // Power Increase
        chargeSpeedReduction = skills.chargeSpeedLevel * 0.07f;
        automaticCadence += (skills.automaticModeLevel * 0.1f) * automaticCadence;

        audioManager = AudioManager.instance;
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
                    audioManager.PlaySound("NormalBeam");
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (repeatTime >= 0.5f)
                    {
                        float holdTime = (repeatTime - 0.5f) * (increaseSpeed  + increaseSpeed * chargeSpeedReduction);
                        if (holdTime < maxIncrease) projectileOriginCurrent.position = projectileOriginStart.position + projectileOriginCurrent.forward * holdTime / 2.0f;
                        if (!chargedProjectile)
                        {
                            chargedProjectile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile.SetUp();
                            audioManager.PlaySound("LoadChargedBeam");
                            playingChargeSound = true;
                        }
                        if (holdTime > maxIncrease)
                        {
                            holdTime = maxIncrease;
                            if (playingChargeSound)
                            {
                                audioManager.StopSound("LoadChargedBeam");
                                playingChargeSound = false;
                            }
                        }
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
                        if (playingChargeSound)
                        {
                            audioManager.StopSound("LoadChargedBeam");
                            playingChargeSound = false;
                        }
                        audioManager.PlaySound("LaunchChargedBeam");
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
                        if (triggerState == TRIGGER_STATE.DOWN) audioManager.PlaySound("NormalBeam");
                        else audioManager.PlaySound("AutomaticBeam");
                    }
                }
                break;
            case PROJECTILE_TYPE.TRIPLE:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                    GameObject.Instantiate(selectedProjectileSecondaryPrefab, projectileOrigin2Start.position, projectileOrigin2Start.rotation);
                    GameObject.Instantiate(selectedProjectileSecondaryPrefab, projectileOrigin3Start.position, projectileOrigin3Start.rotation);
                    audioManager.PlaySound("NormalBeam");
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (repeatTime >= 0.5f)
                    {
                        float holdTime = (repeatTime - 0.5f) * (increaseSpeed + increaseSpeed * chargeSpeedReduction);
                        if (holdTime < maxIncrease)
                        {
                            projectileOriginCurrent.position = projectileOriginStart.position + projectileOriginCurrent.forward * holdTime / 2.0f;
                        }
                        if (!chargedProjectile)
                        {
                            chargedProjectile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation).GetComponent<PlayerCharged>();
                            chargedProjectile.SetUp();
                            audioManager.PlaySound("LoadChargedBeam");
                            playingChargeSound = true;
                        }
                        if (holdTime > maxIncrease)
                        {
                            holdTime = maxIncrease;
                            if (playingChargeSound)
                            {
                                audioManager.StopSound("LoadChargedBeam");
                                playingChargeSound = false;
                            }
                        }
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
                        if (playingChargeSound)
                        {
                            audioManager.StopSound("LoadChargedBeam");
                            playingChargeSound = false;
                        }
                        audioManager.PlaySound("LaunchChargedBeam");
                    }
                    repeatTime = 0;
                }
                break;
            case PROJECTILE_TYPE.MISSILE:
                if (triggerState == TRIGGER_STATE.DOWN)
                {
                    GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                    audioManager.PlaySound("SpawnMissile");
                }
                else if (triggerState == TRIGGER_STATE.REPEAT)
                {
                    repeatTime += Time.deltaTime;
                    if (superMissileCharged) return;
                    if (!superMissileChargingPs.isPlaying && repeatTime >= 0.5f)
                    {
                        superMissileChargingPs.Play();
                        audioManager.PlaySound("LoadSuperMissile");
                        playingChargeSound = true;
                    }
                    if (repeatTime >= superMissileTime - superMissileTime * chargeSpeedReduction)
                    {
                        superMissileChargingPs.Stop();
                        if (!superMissileReady.enabled) superMissileReady.enabled = true;
                        else superMissileReady.Play();
                        superMissileCharged = true;
                        if (playingChargeSound)
                        {
                            audioManager.FadeOutSound("LoadSuperMissile");
                            playingChargeSound = false;
                        }
                    }
                }
                else if (triggerState == TRIGGER_STATE.UP)
                {
                    if (superMissileChargingPs.isPlaying) superMissileChargingPs.Stop();
                    if (playingChargeSound)
                    {
                        audioManager.StopSound("LoadSuperMissile");
                        playingChargeSound = false;
                    }
                    if (repeatTime >= superMissileTime - superMissileTime * chargeSpeedReduction)
                    {
                        GameObject supermissile = GameObject.Instantiate(selectedChargedPrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                        if (shockwaveObtained) supermissile.GetComponent<PlayerSuperMissile>().AddShockwave();
                        superMissileCharged = false;
                        audioManager.PlaySound("SpawnSuperMissile");
                    }
                    else if (repeatTime >= 0.5f)
                    {
                        GameObject.Instantiate(selectedProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
                        audioManager.PlaySound("SpawnMissile");
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
        bool skipSound = false;
        if (newType == GUN_TYPE.NULL)
        {
            newType = selectedGun;
            skipSound = true;
        }

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
                        selectedProjectileSecondaryPrefab = yellowProjectileSecondaryPrefab;
                        break;
                    case PROJECTILE_TYPE.MISSILE:
                        selectedProjectilePrefab = yellowMissilePrefab;
                        selectedChargedPrefab = yellowSuperMissilePrefab;
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
                        selectedProjectileSecondaryPrefab = blueProjectileSecondaryPrefab;
                        break;
                    case PROJECTILE_TYPE.MISSILE:
                        selectedProjectilePrefab = blueMissilePrefab;
                        selectedChargedPrefab = blueSuperMissilePrefab;
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
                        selectedProjectileSecondaryPrefab = redProjectileSecondaryPrefab;
                        break;
                    case PROJECTILE_TYPE.MISSILE:
                        selectedProjectilePrefab = redMissilePrefab;
                        selectedChargedPrefab = redSuperMissilePrefab;
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
                        selectedProjectileSecondaryPrefab = purpleProjectileSecondaryPrefab;
                        break;
                    case PROJECTILE_TYPE.MISSILE:
                        selectedProjectilePrefab = purpleMissilePrefab;
                        selectedChargedPrefab = purpleSuperMissilePrefab;
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
                        selectedProjectileSecondaryPrefab = greenProjectileSecondaryPrefab;
                        break;
                    case PROJECTILE_TYPE.MISSILE:
                        selectedProjectilePrefab = greenMissilePrefab;
                        selectedChargedPrefab = greenSuperMissilePrefab;
                        break;
                }
                break;
        }
        if (newType != selectedGun && !skipSound) audioManager.PlaySound("SelectColor");
        selectedGun = newType;
        screen.SetScreen(newType);
    }

    public void NewBeamUnlocked(GUN_TYPE type)
    {
        switch (type)
        {
            case GUN_TYPE.BLUE:
                selectedTriangle[1].material = selectedMat[1];
                break;
            case GUN_TYPE.RED:
                selectedTriangle[2].material = selectedMat[2];
                break;
            case GUN_TYPE.PURPLE:
                selectedTriangle[3].material = selectedMat[3];
                break;
            case GUN_TYPE.GREEN:
                selectedTriangle[0].material = selectedMat[0];
                if (selectedGun == GUN_TYPE.YELLOW) SwapGunType(GUN_TYPE.GREEN);
                break;
        }
    }
}
