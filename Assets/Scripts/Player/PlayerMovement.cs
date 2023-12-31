using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;

    [Header("Jump")]
    [SerializeField] InputActionProperty jumpAction;
    [SerializeField] ActionBasedContinuousMoveProvider moveProvider;
    [HideInInspector] public bool superJumpUnlocked = false;
    [SerializeField] float jumpHeight;
    [SerializeField] float superJumpHeight;
    float expectedGravity;
    bool groundJump;
    bool airJump;
    Vector3 movement;
    [NonEditable] public bool dobleJumpObtained = false;

    [Header("Dash")]
    [SerializeField] Transform head;
    [SerializeField] InputActionProperty stickAction;
    [SerializeField] InputActionProperty dashAction;
    [HideInInspector] public bool dashUnlocked = false;
    [SerializeField] float dashCooldown;
    float dashCd;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] ParticleSystem dashPs;
    [SerializeField] Material tunnelingMaterial;
    [NonEditable] public bool airDashObtained = false;


    // Start is called before the first frame update
    void Start()
    {
        // Skills Unlocked
        dashUnlocked = PlayerSkills.instance.dashUnlocked;
        superJumpUnlocked = PlayerSkills.instance.superJumpUnlocked;

        controller = GetComponent<CharacterController>();
        groundJump = true;
        airJump = false;
        expectedGravity = 0;
        dashCd = 0;

        tunnelingMaterial.SetFloat("_ApertureSize", 1);
    }

    // Update is called once per frame
    void Update()
    {
        // jump
        if (IsGrounded())
        {
            groundJump = true;
            if (dobleJumpObtained) airJump = true;
            movement.y = 0;
        }
        else groundJump = false;

        if (expectedGravity > 0)
        {
            expectedGravity -= Physics.gravity.y * Time.deltaTime;
            if (expectedGravity < 0) movement.y = 0;
        }

        if ((groundJump || airJump) && jumpAction.action.WasPressedThisFrame())
        {
            if (groundJump) groundJump = false;
            else airJump = false;
            if (superJumpUnlocked) movement.y = superJumpHeight;
            else movement.y = jumpHeight;
            expectedGravity = jumpHeight;
            moveProvider.m_VerticalVelocity = Vector3.zero; // m_VerticalVelocity wasn't public, I change it
        }
        if (movement.y > 0) controller.Move(movement * Time.deltaTime);

        // dash
        if (dashUnlocked)
        {
            if (dashCd > 0)
            {
                dashCd -= Time.deltaTime;
                if (dashCd <= 0) dashPs.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if ((IsGrounded() || airDashObtained) && dashCd <= 0 && dashAction.action.WasPressedThisFrame())
            {
                float stickX = stickAction.action.ReadValue<Vector2>().x;
                float stickY = stickAction.action.ReadValue<Vector2>().y;
                Quaternion headRotationY = new Quaternion(0, head.rotation.y, 0, head.rotation.w);
                Vector3 direction = (stickX == 0 && stickY == 0) ? headRotationY * (-transform.forward) : headRotationY * new Vector3(stickX, 0, stickY).normalized;
                StartCoroutine(DashCo(direction));
            }
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position - Vector3.down * 0.1f, Vector3.down, 0.1f);
    }

    IEnumerator DashCo(Vector3 direction)
    {
        dashCd = 999;
        dashPs.Play();
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            tunnelingMaterial.SetFloat("_ApertureSize", 0.5f);

            controller.Move(dashSpeed * Time.deltaTime * direction);
            yield return null;
        }
        dashCd = dashCooldown;
        tunnelingMaterial.SetFloat("_ApertureSize", 1);
    }
}
