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
                chargedProjectile.Launch(projectileOriginCurrent.rotation);
                chargedProjectile = null;
                projectileOriginCurrent.position = projectileOriginStart.position;
            }
            repeatTime = 0;
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
                selectedProjectilePrefab = yellowProjectilePrefab;
                selectedChargedPrefab = yellowChargedPrefab;
                break;
            case GUN_TYPE.BLUE:
                selectedProjectilePrefab = blueProjectilePrefab;
                selectedChargedPrefab = blueChargedPrefab;
                break;
            case GUN_TYPE.RED:
                selectedProjectilePrefab = redProjectilePrefab;
                selectedChargedPrefab = redChargedPrefab;
                break;
            case GUN_TYPE.PURPLE:
                selectedProjectilePrefab = purpleProjectilePrefab;
                selectedChargedPrefab = purpleChargedPrefab;
                break;
            case GUN_TYPE.GREEN:
                selectedProjectilePrefab = greenProjectilePrefab;
                selectedChargedPrefab = greenChargedPrefab;
                break;
        }
        screen.SetScreen(newType);
    }
}
