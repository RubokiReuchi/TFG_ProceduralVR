using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;
using UnityEditor.Recorder;
using UnityEditor;
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

    [Header("Health & Shield")]
    [NonEditable][SerializeField] float currentHealth;
    float maxHealth = 250;
    [NonEditable][SerializeField] float currentShieldCooldown = 0.0f;
    [SerializeField] float shieldCooldown = 5.0f;
    [HideInInspector] public bool shieldObtained = false;
    [SerializeField] Material shieldMaterial;
    [SerializeField] HandHealth displayHealth;
    [SerializeField] Material takeDamageMaterial;
    Coroutine takeDamage;
    float defense = 0.0f;
    float lifeRegen = 0.0f;
    float lifeRegenCd = 0.0f;
    [SerializeField] Material deathMaterial;

    [Header("XRay")]
    [SerializeField] InputActionProperty xRayAction;
    [HideInInspector] public bool xRayVisionObtained = false;
    [HideInInspector] public bool xRayVisionActive = false;
    [SerializeField] Material xRayMaterial;
    IEnumerator xRayCoroutine = null;
    [NonEditable][SerializeField] float xRayBattery;
    float maxXRayBattery = 50;
    [SerializeField] Material xRayBatteryMaterial;
    int maxXRayBatteryIncrease = 0;
    float xRayBatteryRecoveryIncrease = 0;

    [SerializeField] Material fadeMaterial;
    [SerializeField] float fadeSpeed = 0.5f;
    [SerializeField] InputActionProperty temporalySaveGame;

    [Header("AreaDamage")]
    List<string> activeUUIDs = new();
    float cdUUID = 0;

    RecorderWindow recorderWindow;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Power Increase
        PlayerSkills skills = PlayerSkills.instance;
        defense = skills.defenseLevel * 0.05f;
        maxHealth += skills.maxHealthLevel * 50.0f;
        lifeRegen = skills.lifeRegenLevel * 0.002f;
        maxXRayBatteryIncrease = Mathf.CeilToInt(skills.xRayVisionLevel / 2.0f);
        maxXRayBattery += (maxXRayBatteryIncrease * 0.5f) * maxXRayBattery;
        xRayBatteryRecoveryIncrease = Mathf.FloorToInt(skills.xRayVisionLevel / 2.0f) * 0.15f;
        shieldCooldown -= skills.shieldLevel * 0.5f;

        currentHealth = maxHealth;
        displayHealth.SetCells(maxHealth);
        displayHealth.UpdateHealthDisplay(currentHealth);
        shieldMaterial.SetFloat("_Opacity", 0);
        takeDamageMaterial.SetFloat("_Opacity", 0);
        xRayMaterial.SetFloat("_ApertureSize", 1);
        xRayBattery = maxXRayBattery;
        xRayBatteryMaterial.SetFloat("_FillPercentage", maxXRayBattery / (maxXRayBatteryIncrease / 2.0f + 1.0f));
        fadeMaterial.SetFloat("_Opacity", 1);
        deathMaterial.SetFloat("_Opacity", 0);
        StartCoroutine(FadeOut());

        recorderWindow = (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
    }

    void Update()
    {
        // temp
        if (temporalySaveGame.action.WasPressedThisFrame() || Input.GetKeyDown(KeyCode.Space))
        {
            if (!recorderWindow.IsRecording()) recorderWindow.StartRecording();
            else recorderWindow.StopRecording();
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
            if (xRayBattery >= 5) // min amount to active xRay Vision
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
            xRayBattery -= Time.deltaTime * 10; // 5 secs to empty without upgrades
            if (xRayBattery <= 0)
            {
                xRayVisionActive = false;
                StopCoroutine(xRayCoroutine);
                xRayCoroutine = XRayOff();
                StartCoroutine(xRayCoroutine);
                xRayBattery = 0;
            }
            xRayBatteryMaterial.SetFloat("_FillPercentage", xRayBattery / (maxXRayBatteryIncrease / 2.0f + 1.0f));
        }
        else if (xRayBattery < maxXRayBattery)
        {
            xRayBattery += Time.deltaTime * (2.5f + (2.5f * xRayBatteryRecoveryIncrease)); // 20 secs to fill without upgrades
            if (xRayBattery > maxXRayBattery) xRayBattery = maxXRayBattery;
            xRayBatteryMaterial.SetFloat("_FillPercentage", xRayBattery / (maxXRayBatteryIncrease / 2.0f + 1.0f));
        }

        if (shieldObtained && currentShieldCooldown > 0)
        {
            currentShieldCooldown -= Time.deltaTime;
            if (currentShieldCooldown < 0) currentShieldCooldown = 0;
        }

        if (lifeRegen > 0)
        {
            if (lifeRegenCd >= 1.0f)
            {
                HealPercentage(lifeRegen);
                lifeRegenCd -= 1.0f;
            }
            lifeRegenCd += Time.deltaTime;
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentShieldCooldown == 0)
        {
            currentShieldCooldown = shieldCooldown;
            StartCoroutine(ShieldCo());
            return;
        }
        else currentShieldCooldown = shieldCooldown;

        displayHealth.UpdateHealthDisplay(currentHealth);
        currentHealth -= amount * (1 - defense);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(DeathFadeOut());
            return;
        }

        if (takeDamage != null) StopCoroutine(takeDamage);
        takeDamage = StartCoroutine(TakeDamageCo());
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

    public void HealPercentage(float amount)
    {
        currentHealth += amount * maxHealth;
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

    IEnumerator TakeDamageCo()
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
        takeDamage = null;
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
            opacity -= Time.deltaTime * fadeSpeed;
            if (opacity < 0) opacity = 0;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
    }

    IEnumerator DeathFadeOut()
    {
        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 0.2f;
            if (opacity > 1) opacity = 1;
            deathMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 0.5f;
            if (opacity > 1) opacity = 1;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(2); // lobby
    }

    public void IncreaseMaxHealth()
    {
        maxHealth += 50.0f;
        currentHealth += 50.0f;
        displayHealth.SetCells(maxHealth);
        displayHealth.UpdateHealthDisplay(currentHealth);
    }
}
