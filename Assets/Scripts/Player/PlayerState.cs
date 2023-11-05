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

    [Header("Head Speed")]
    [HideInInspector] public float headSpeed;
    public Transform head;
    Vector3 lastFrameHeadPosition = Vector3.zero;

    [Header("Health")]
    float maxHealth = 250;
    float currentHealth;
    [SerializeField] HandHealth displayHealth;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        displayHealth.SetCells(maxHealth);
        displayHealth.UpdateHealthDisplay(currentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        // head speed
        //headSpeed = (head.position - lastFrameHeadPosition).magnitude / Time.deltaTime;
        //lastFrameHeadPosition = head.position;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // game over
        }
        displayHealth.UpdateHealthDisplay(currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        displayHealth.UpdateHealthDisplay(currentHealth);
    }
}
