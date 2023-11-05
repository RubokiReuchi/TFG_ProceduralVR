using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;

    [Header("Jump")]
    [SerializeField] InputActionProperty jumpAction;
    [SerializeField] float jumpHeight;
    int remainingJumps;
    float gravity = Physics.gravity.y;
    Vector3 movement;

    [Header("Dash")]
    [SerializeField] Transform head;
    [SerializeField] InputActionProperty stickAction;
    [SerializeField] InputActionProperty dashAction;
    [SerializeField] float dashCooldown;
    float dashCd;
    [SerializeField] float dashSpeed;
    [SerializeField] float dashDuration;
    [SerializeField] ParticleSystem dashPs;
    [SerializeField] Material tunnelingMaterial;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        remainingJumps = 0;
        dashCd = 0;

        tunnelingMaterial.SetFloat("_ApertureSize", 1);
    }

    // Update is called once per frame
    void Update()
    {
        // jump
        if (IsGrounded())
        {
            remainingJumps = 2;
            movement.y = 0;
        }
        else movement.y += gravity * Time.deltaTime; // apply gravity

        if (remainingJumps > 0 && jumpAction.action.WasPressedThisFrame())
        {
            remainingJumps--;
            movement.y = 0;
            movement.y += jumpHeight;
        }
        controller.Move(movement * Time.deltaTime);

        // dash
        if (dashCd > 0)
        {
            dashCd -= Time.deltaTime;
            if (dashCd <= 0) dashPs.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (IsGrounded() && dashCd <= 0 && dashAction.action.WasPressedThisFrame())
        {
            float stickX = stickAction.action.ReadValue<Vector2>().x;
            float stickY = stickAction.action.ReadValue<Vector2>().y;
            Quaternion headRotationY = new Quaternion(0, head.rotation.y, 0, head.rotation.w);
            Vector3 direction = (stickX == 0 && stickY == 0) ? headRotationY * (-transform.forward) : headRotationY * new Vector3(stickX, 0, stickY).normalized;
            StartCoroutine(DashCo(direction));
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
