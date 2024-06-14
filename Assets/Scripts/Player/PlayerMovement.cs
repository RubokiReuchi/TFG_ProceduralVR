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
    float skipGroudDetection = 0.0f;

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

    [Header("Slow")]
    float baseSpeed;
    float slowPercentage;
    float slowDuration;
    [SerializeField] Material slowMetterMat;
    float displayedSlow;

    [Header("Audio")]
    AudioManager audioManager;
    float stepSoundCd = 1.0f;
    bool lastFrameGrounded = true;

    // Start is called before the first frame update
    void Start()
    {
        // Skills Unlocked
        PlayerSkills skills = PlayerSkills.instance;
        dashUnlocked = skills.dashUnlocked;
        superJumpUnlocked = skills.superJumpUnlocked;

        // Power Increase
        dashCooldown -= (skills.dashCdLevel * 0.03f) * dashCooldown;

        controller = GetComponent<CharacterController>();
        groundJump = true;
        airJump = false;
        expectedGravity = 0;
        dashCd = 0;

        baseSpeed = moveProvider.moveSpeed;
        slowPercentage = 0;
        slowDuration = 0;
        slowMetterMat.SetFloat("_FillPercentage", 0);
        displayedSlow = 0;

        tunnelingMaterial.SetFloat("_ApertureSize", 1);

        audioManager = AudioManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        bool isGrounded = IsGrounded();

        // jump
        if (isGrounded)
        {
            stepSoundCd -= Time.deltaTime * controller.velocity.magnitude;
            if (stepSoundCd <= 0)
            {
                audioManager.PlaySoundArray("Step");
                stepSoundCd = 1.0f;
            }
            if (!lastFrameGrounded) audioManager.PlaySound("Land");

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
            if (superJumpUnlocked)
            {
                movement.y = superJumpHeight;
                audioManager.PlaySound("SuperJump");
            }
            else
            {
                movement.y = jumpHeight;
                audioManager.PlaySound("Jump");
            }
            expectedGravity = jumpHeight;
            //moveProvider.m_VerticalVelocity = Vector3.zero; // m_VerticalVelocity wasn't public, I change it
            skipGroudDetection = 0.1f;
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

            if ((isGrounded || airDashObtained) && dashCd <= 0 && dashAction.action.WasPressedThisFrame())
            {
                float stickX = stickAction.action.ReadValue<Vector2>().x;
                float stickY = stickAction.action.ReadValue<Vector2>().y;
                Quaternion headRotationY = new Quaternion(0, head.rotation.y, 0, head.rotation.w);
                Vector3 direction = (stickX == 0 && stickY == 0) ? headRotationY * (-transform.forward) : headRotationY * new Vector3(stickX, 0, stickY).normalized;
                StartCoroutine(DashCo(direction));
                audioManager.PlaySound("Dash");
            }
        }

        // slow
        if (slowDuration > 0)
        {
            slowDuration -= Time.deltaTime;
        }
        else if (slowPercentage > 0)
        {
            slowPercentage -= Time.deltaTime * 100.0f;
            if (slowPercentage < 0) slowPercentage = 0;
            displayedSlow = slowPercentage;
            slowMetterMat.SetFloat("_FillPercentage", displayedSlow / 2.0f);
        }
        if (moveProvider.moveSpeed != baseSpeed - baseSpeed * (slowPercentage / 100.0f))
        {
            moveProvider.moveSpeed = baseSpeed - baseSpeed * (slowPercentage / 100.0f);
        }

        lastFrameGrounded = isGrounded;
    }

    bool IsGrounded()
    {
        if (skipGroudDetection > 0.0f)
        {
            skipGroudDetection -= Time.deltaTime;
            return false;
        }
        return Physics.Raycast(transform.position - Vector3.down * 0.1f, Vector3.down, 0.15f);
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

    public void TakeSlow(float slowPercentage, float slowDuration)
    {
        if (slowPercentage >= this.slowPercentage) this.slowPercentage = slowPercentage;
        this.slowDuration = slowDuration;
        slowMetterMat.SetFloat("_FillPercentage", this.slowPercentage / 2.0f);
    }
}
