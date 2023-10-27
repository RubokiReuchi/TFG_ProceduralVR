using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ActionBasedController))]
public class GunController : MonoBehaviour
{
    ActionBasedController controller;
    [SerializeField] RightHand hand;

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
    }
}
