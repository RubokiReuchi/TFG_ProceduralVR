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
    [SerializeField] InputActionProperty dashAction;
    float dashCd;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        remainingJumps = 0;
        dashCd = 0;
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
        if (dashCd <= 0 && dashAction.action.ReadValue<float>() == 1)
        {
            Debug.Log("Dash");
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position - Vector3.down * 0.1f, Vector3.down, 0.1f);
    }
}
