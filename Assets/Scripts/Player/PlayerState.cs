using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public enum LEFT_HAND_POSE
{
    OPEN,
    CLOSE,
    INDEX,
    OK
}

public class PlayerState : MonoBehaviour
{
    public static PlayerState instance;

    [NonEditable] public LEFT_HAND_POSE leftHandPose;
    [HideInInspector] public float headSpeed;
    public Transform head;
    Vector3 lastFrameHeadPosition = Vector3.zero;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // head speed
        //headSpeed = (head.position - lastFrameHeadPosition).magnitude / Time.deltaTime;
        //lastFrameHeadPosition = head.position;
    }
}
