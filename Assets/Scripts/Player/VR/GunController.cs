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
    [SerializeField] Animator beamSelectAnimator;
    float blendTarget = 0;
    float blendAmount = 0;
    [SerializeField] Animator[] blendTriangle;
    float[] blendTriangleTarget = { 0, 0, 0, 0 };
    float[] blendTriangleAmount = { 0, 0, 0, 0 };


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<ActionBasedController>();
    }

    // Update is called once per frame
    void Update()
    {
        hand.SetIndex(controller.activateActionValue.action.ReadValue<float>());

        float stickX = stickAction.action.ReadValue<Vector2>().x;
        float stickY = stickAction.action.ReadValue<Vector2>().y;
        if (stickX > 0.1f)
        {
            float stickAbsX = Mathf.Abs(stickX);
            float stickAbsY = Mathf.Abs(stickY);
            if (stickY > 0.1f)
            {
                if (stickAbsX > stickAbsY) BeamSelector(GUN_TYPE.BLUE); // right input
                else BeamSelector(GUN_TYPE.RED); // up input
            }
            else if (stickY < -0.1f)
            {
                if (stickAbsX > stickAbsY) BeamSelector(GUN_TYPE.BLUE); // right input
                else BeamSelector(GUN_TYPE.YELLOW); // down input
            }
            else BeamSelector(GUN_TYPE.BLUE); // right input
        }
        else if (stickX < -0.1f)
        {
            float stickAbsX = Mathf.Abs(stickX);
            float stickAbsY = Mathf.Abs(stickY);
            if (stickY > 0.1f)
            {
                if (stickAbsX > stickAbsY) BeamSelector(GUN_TYPE.PURPLE); // left input
                else BeamSelector(GUN_TYPE.RED); // up input
            }
            else if (stickY < -0.1f)
            {
                if (stickAbsX > stickAbsY) BeamSelector(GUN_TYPE.PURPLE); // left input
                else BeamSelector(GUN_TYPE.YELLOW); // down input
            }
            else BeamSelector(GUN_TYPE.PURPLE); // left input
        }
        else if (stickY > 0.1f)
        {
            BeamSelector(GUN_TYPE.RED); // up input
        }
        else if (stickY < -0.1f)
        {
            BeamSelector(GUN_TYPE.YELLOW); // down input
        }
        else blendTarget = 0.0f;

        if (blendAmount < blendTarget)
        {
            blendAmount += Time.deltaTime * 5.0f;
            if (blendAmount > blendTarget) blendAmount = blendTarget;
            beamSelectAnimator.SetFloat("Blend", blendAmount);
        }
        else if (blendAmount > blendTarget)
        {
            blendAmount -= Time.deltaTime * 5.0f;
            if (blendAmount < blendTarget) blendAmount = blendTarget;
            beamSelectAnimator.SetFloat("Blend", blendAmount);
        }

        for (int i = 0; i < 4; i++)
        {
            if (blendTriangleAmount[i] < blendTriangleTarget[i])
            {
                blendTriangleAmount[i] += Time.deltaTime * 5.0f;
                if (blendTriangleAmount[i] > blendTriangleTarget[i]) blendTriangleAmount[i] = blendTriangleTarget[i];
                blendTriangle[i].SetFloat("Blend", blendTriangleAmount[i]);
            }
            else if (blendTriangleAmount[i] > blendTriangleTarget[i])
            {
                blendTriangleAmount[i] -= Time.deltaTime * 5.0f;
                if (blendTriangleAmount[i] < blendTriangleTarget[i]) blendTriangleAmount[i] = blendTriangleTarget[i];
                blendTriangle[i].SetFloat("Blend", blendTriangleAmount[i]);
            }
        }
    }

    void BeamSelector(GUN_TYPE type)
    {
        playerGun.SwapGunType(type);
        blendTarget = 1.0f;

        switch (type)
        {
            case GUN_TYPE.YELLOW:
                blendTriangleTarget[0] = 1.0f;
                blendTriangleTarget[1] = 0.0f;
                blendTriangleTarget[2] = 0.0f;
                blendTriangleTarget[3] = 0.0f;
                break;
            case GUN_TYPE.BLUE:
                if (!playerGun.blueUnlocked) return;
                blendTriangleTarget[0] = 0.0f;
                blendTriangleTarget[1] = 1.0f;
                blendTriangleTarget[2] = 0.0f;
                blendTriangleTarget[3] = 0.0f;
                break;
            case GUN_TYPE.RED:
                if (!playerGun.redUnlocked) return;
                blendTriangleTarget[0] = 0.0f;
                blendTriangleTarget[1] = 0.0f;
                blendTriangleTarget[2] = 1.0f;
                blendTriangleTarget[3] = 0.0f;
                break;
            case GUN_TYPE.PURPLE:
                if (!playerGun.purpleUnlocked) return;
                blendTriangleTarget[0] = 0.0f;
                blendTriangleTarget[1] = 0.0f;
                blendTriangleTarget[2] = 0.0f;
                blendTriangleTarget[3] = 1.0f;
                break;
        }
    }
}
