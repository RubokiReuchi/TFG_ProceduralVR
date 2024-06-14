using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUps : MonoBehaviour
{
    public static PlayerPowerUps instance;

    [SerializeField] Transform leftHand;
    [SerializeField] PlayerGun playerGun;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] GameObject xRayBattery;

    [Header("Audio")]
    AudioManager audioManager;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        audioManager = AudioManager.instance;
    }

    public void GetPowerUp(POWER_UP_TYPE type)
    {
        switch (type)
        {
            case POWER_UP_TYPE.HAND_OF_THE_GIANT:
                StartCoroutine(GetHandOfTheGiant());
                StatusMenuPoints.instance.panels[4].GetComponent<StatusMenuPanel>().UpdateInformation(1, null);
                StatusMenuPoints.instance.SetExclamation(4);
                audioManager.PlaySound("IncreaseHandSize");
                break;
            case POWER_UP_TYPE.XRAY_VISION:
                PlayerState.instance.xRayVisionObtained = true;
                xRayBattery.SetActive(true);
                float maxBattery = 50 + 50 * (Mathf.CeilToInt(PlayerSkills.instance.xRayVisionLevel / 2.0f) * 0.5f);
                float[] xRayValues = { maxBattery / 10.0f, maxBattery / 2.5f + 2.5f * (Mathf.FloorToInt(PlayerSkills.instance.xRayVisionLevel / 2.0f)) };
                StatusMenuPoints.instance.panels[3].GetComponent<StatusMenuPanel>().UpdateInformation(2, xRayValues);
                StatusMenuPoints.instance.SetExclamation(3);
                break;
            case POWER_UP_TYPE.AUTOMATIC_MODE:
                playerGun.projectileType = PROJECTILE_TYPE.AUTOMATIC;
                float[] automaticValues = { 8 + 8 * (PlayerSkills.instance.automaticModeLevel * 0.1f) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(8, automaticValues);
                StatusMenuPoints.instance.SetExclamation(0);
                break;
            case POWER_UP_TYPE.TRIPLE_SHOT:
                playerGun.projectileType = PROJECTILE_TYPE.TRIPLE;
                float[] tripleShotValues = { 50 + (PlayerSkills.instance.tripleShotModeLevel * 10) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(9, tripleShotValues);
                StatusMenuPoints.instance.SetExclamation(0);
                break;
            case POWER_UP_TYPE.missile_MODE:
                playerGun.projectileType = PROJECTILE_TYPE.MISSILE;
                float[] missileValues = { (Mathf.CeilToInt(PlayerSkills.instance.missileModeLevel / 2.0f) * 10), (Mathf.FloorToInt(PlayerSkills.instance.missileModeLevel / 2.0f) * 15) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(10, missileValues);
                StatusMenuPoints.instance.SetExclamation(0);
                playerGun.SwapGunType(GUN_TYPE.NULL);
                break;
            case POWER_UP_TYPE.SHOCKWAVE:
                playerGun.shockwaveObtained = true;
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(7, null);
                StatusMenuPoints.instance.SetExclamation(0);
                break;
            case POWER_UP_TYPE.AIR_DASH:
                playerMovement.airDashObtained = true;
                StatusMenuPoints.instance.panels[2].GetComponent<StatusMenuPanel>().UpdateInformation(1, null);
                StatusMenuPoints.instance.SetExclamation(2);
                break;
            case POWER_UP_TYPE.DOBLE_JUMP:
                playerMovement.dobleJumpObtained = true;
                StatusMenuPoints.instance.panels[1].GetComponent<StatusMenuPanel>().UpdateInformation(2, null);
                StatusMenuPoints.instance.SetExclamation(1);
                break;
            case POWER_UP_TYPE.SHIELD:
                PlayerState.instance.shieldObtained = true;
                float[] shieldValues = { (PlayerSkills.instance.shieldLevel * 10) };
                StatusMenuPoints.instance.panels[2].GetComponent<StatusMenuPanel>().UpdateInformation(3, shieldValues);
                StatusMenuPoints.instance.SetExclamation(2);
                break;
            default:
                break;
        }
        audioManager.PlaySound("PickPowerUp");
    }

    IEnumerator GetHandOfTheGiant()
    {
        float initialScale = leftHand.localScale.x;
        float currentScale = initialScale;
        while (currentScale < initialScale * 2.0f)
        {
            currentScale += Time.deltaTime;
            if (currentScale > initialScale * 2.0f) currentScale = initialScale * 2.0f;
            leftHand.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
    }
}
