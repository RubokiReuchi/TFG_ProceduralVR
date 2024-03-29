using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Health & Shield")]
    [NonEditable][SerializeField] float currentHealth;
    float maxHealth = 250;
    [NonEditable][SerializeField] float shieldCooldown = 5.0f;
    [HideInInspector] public bool shieldObtained = false;
    [SerializeField] Material shieldMaterial;
    [SerializeField] HandHealth displayHealth;
    [SerializeField] Material takeDamageMaterial;

    [Header("XRay")]
    [SerializeField] InputActionProperty xRayAction;
    [HideInInspector] public bool xRayVisionObtained = false;
    [HideInInspector] public bool xRayVisionActive = false;
    [SerializeField] Material xRayMaterial;
    IEnumerator xRayCoroutine = null;
    float xRayBattery = 50;
    [SerializeField] Material xRayBatteryMaterial;

    [SerializeField] Material fadeMaterial;
    [SerializeField] InputActionProperty temporalySaveGame;

    [Header("AreaDamage")]
    List<string> activeUUIDs = new();
    float cdUUID = 0;

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
        shieldMaterial.SetFloat("_Opacity", 0);
        takeDamageMaterial.SetFloat("_Opacity", 0);
        xRayMaterial.SetFloat("_ApertureSize", 1);
        xRayBatteryMaterial.SetFloat("_FillPercentage", 50.0f);
        fadeMaterial.SetFloat("_Opacity", 1);
        StartCoroutine(FadeOut());
    }

    void Update()
    {
        // temp
        if (temporalySaveGame.action.WasPressedThisFrame() || Input.GetKeyDown(KeyCode.Space))
        {
            DataPersistenceManager.instance.SaveGame();
        }
        //

        if (cdUUID > 0)
        {
            cdUUID -= Time.deltaTime;
            if (cdUUID <= 0)
            {
                cdUUID = 0;
                activeUUIDs.Clear();
            }
        }

        if (xRayVisionObtained && xRayAction.action.WasPressedThisFrame())
        {
            if (xRayBattery >= 5)
            {
                xRayVisionActive = !xRayVisionActive;
                if (xRayCoroutine != null) StopCoroutine(xRayCoroutine);

                if (xRayVisionActive) xRayCoroutine = XRayOn();
                else xRayCoroutine = XRayOff();
                StartCoroutine(xRayCoroutine);
            }
            else
            {
                // no battery sound
            }
        }
        if (xRayVisionActive)
        {
            xRayBattery -= Time.deltaTime * 10; // 5 secs to empty
            if (xRayBattery <= 0)
            {
                xRayVisionActive = false;
                StopCoroutine(xRayCoroutine);
                xRayCoroutine = XRayOff();
                StartCoroutine(xRayCoroutine);
                xRayBattery = 0;
            }
            xRayBatteryMaterial.SetFloat("_FillPercentage", xRayBattery);
        }
        else if (xRayBattery < 50)
        {
            xRayBattery += Time.deltaTime * 2.5f; // 20 secs to fill
            if (xRayBattery > 50) xRayBattery = 50;
            xRayBatteryMaterial.SetFloat("_FillPercentage", xRayBattery);
        }

        if (shieldObtained && shieldCooldown > 0)
        {
            shieldCooldown -= Time.deltaTime;
            if (shieldCooldown < 0) shieldCooldown = 0;
        }
    }

    public void TakeDamage(float amount)
    {
        if (shieldCooldown == 0)
        {
            shieldCooldown = 5.0f;
            StartCoroutine(ShieldCo());
            return;
        }
        else shieldCooldown = 5.0f;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            // game over
        }
        displayHealth.UpdateHealthDisplay(currentHealth);

        StopCoroutine("TakeDamegeCo");
        StartCoroutine(TakeDamegeCo());
    }

    public void TakeAreaDamage(float amount, string UUID)
    {
        if (activeUUIDs.Contains(UUID)) return;
        else
        {
            activeUUIDs.Add(UUID);
            cdUUID = 1;
            TakeDamage(amount);
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        displayHealth.UpdateHealthDisplay(currentHealth);
    }

    IEnumerator ShieldCo()
    {
        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 5;
            if (opacity > 1) opacity = 1;
            shieldMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);
        opacity = 1.0f;
        while (opacity > 0)
        {
            opacity -= Time.deltaTime * 2;
            if (opacity < 0) opacity = 0;
            shieldMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
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

    IEnumerator XRayOn()
    {
        float apertureSize = xRayMaterial.GetFloat("_ApertureSize");
        while (apertureSize > 0)
        {
            apertureSize -= Time.deltaTime * 2;
            if (apertureSize < 0) apertureSize = 0;
            xRayMaterial.SetFloat("_ApertureSize", apertureSize);
            yield return null;
        }
    }

    IEnumerator XRayOff()
    {
        float apertureSize = xRayMaterial.GetFloat("_ApertureSize");
        while (apertureSize < 1)
        {
            apertureSize += Time.deltaTime * 2;
            if (apertureSize > 1) apertureSize = 1;
            xRayMaterial.SetFloat("_ApertureSize", apertureSize);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float opacity = 1.0f;
        while (opacity > 0)
        {
            opacity -= Time.deltaTime * 1;
            if (opacity < 0) opacity = 0;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
    }
}
