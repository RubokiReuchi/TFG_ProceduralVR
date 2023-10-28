using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ActionBasedController))]
public class GunController : MonoBehaviour
{
    ActionBasedController controller;
    [SerializeField] InputActionProperty stickAction;
    [SerializeField] RightHand hand;
    [SerializeField] PlayerGun playerGun;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<ActionBasedController>();
    }

    // Update is called once per frame
    void Update()
    {
        hand.SetIndex(controller.activateActionValue.action.ReadValue<float>());
        if (controller.activateAction.action.ReadValue<float>() == 1.0f)
        {
            // cooldown...
            hand.Dash();
        }

        float stickX = stickAction.action.ReadValue<Vector2>().x;
        float stickY = stickAction.action.ReadValue<Vector2>().y;
        if (stickX > 0.1f)
        {
            float stickAbsX = Mathf.Abs(stickX);
            float stickAbsY = Mathf.Abs(stickY);
            if (stickY > 0.1f)
            {
                if (stickAbsX > stickAbsY) playerGun.SwapGunType(GUN_TYPE.BLUE); // right input
                else playerGun.SwapGunType(GUN_TYPE.RED); // up input
            }
            else if (stickY < -0.1f)
            {
                if (stickAbsX > stickAbsY) playerGun.SwapGunType(GUN_TYPE.BLUE); // right input
                else playerGun.SwapGunType(GUN_TYPE.YELLOW); // down input
            }
            else playerGun.SwapGunType(GUN_TYPE.BLUE); // right input
        }
        else if (stickX < -0.1f)
        {
            float stickAbsX = Mathf.Abs(stickX);
            float stickAbsY = Mathf.Abs(stickY);
            if (stickY > 0.1f)
            {
                if (stickAbsX > stickAbsY) playerGun.SwapGunType(GUN_TYPE.PURPLE); // left input
                else playerGun.SwapGunType(GUN_TYPE.RED); // up input
            }
            else if (stickY < -0.1f)
            {
                if (stickAbsX > stickAbsY) playerGun.SwapGunType(GUN_TYPE.PURPLE); // left input
                else playerGun.SwapGunType(GUN_TYPE.YELLOW); // down input
            }
            else playerGun.SwapGunType(GUN_TYPE.PURPLE); // left input
        }
        else if (stickY > 0.1f)
        {
            playerGun.SwapGunType(GUN_TYPE.RED); // up input
        }
        else if (stickY < -0.1f)
        {
            playerGun.SwapGunType(GUN_TYPE.YELLOW); // down input
        }
    }
}
