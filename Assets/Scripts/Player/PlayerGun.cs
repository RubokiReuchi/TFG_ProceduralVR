using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

    [SerializeField] Transform projectileOriginStart;
    [SerializeField] Transform projectileOriginCurrent;
    [SerializeField] GameObject yellowProjectilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        triggerState = TRIGGER_STATE.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateTriggerState();

        if (triggerState == TRIGGER_STATE.DOWN)
        {
            GameObject.Instantiate(yellowProjectilePrefab, projectileOriginCurrent.position, projectileOriginCurrent.rotation);
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
}
