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
    [SerializeField] Material takeDamageMaterial;

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
        takeDamageMaterial.SetFloat("_Opacity", 0);
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

        StopAllCoroutines();
        StartCoroutine(TakeDamegeCo());
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        displayHealth.UpdateHealthDisplay(currentHealth);
    }

    IEnumerator TakeDamegeCo()
    {
        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 5;
            if (opacity > 1) opacity = 1;
            takeDamageMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);
        opacity = 1.0f;
        while (opacity > 0)
        {
            opacity -= Time.deltaTime * 2;
            if (opacity < 0) opacity = 0;
            takeDamageMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
    }
}
