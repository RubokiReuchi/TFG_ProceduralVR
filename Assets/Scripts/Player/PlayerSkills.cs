using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum COIN
{
    BIOMATTER,
    GEAR
}

public class PlayerSkills : MonoBehaviour, IDataPersistence
{
    public static PlayerSkills instance;

    [Header("Coins")]
    [NonEditable] public int biomatter;
    [NonEditable] public int gear;

    [Header("Skills")]
    [NonEditable] public bool dashUnlocked;
    [NonEditable] public bool blueUnlocked;
    [NonEditable] public bool redUnlocked;
    [NonEditable] public bool blockingFistUnlocked;
    [NonEditable] public bool purpleUnlocked;
    [NonEditable] public bool superJumpUnlocked;
    [NonEditable] public bool greenUnlocked;

    [SerializeField] GameObject leftHandCollision;
    [SerializeField] PlayerGun playerGun;
    [SerializeField] PlayerMovement playerMovement;

    [Header("Powers")]
    [NonEditable] public int attackLevel;
    [NonEditable] public int defenseLevel;
    [NonEditable] public int chargeSpeedLevel;
    [NonEditable] public int dashCdLevel;
    [NonEditable] public int proyectileSpeedLevel;
    [NonEditable] public int maxHealthLevel;
    [NonEditable] public int lifeRegenLevel;
    [NonEditable] public int lifeChargeLevel;

    [Header("Mechanics")]
    [NonEditable] public int xRayVisionLevel;
    [NonEditable] public int automaticModeLevel;
    [NonEditable] public int tripleShotModeLevel;
    [NonEditable] public int missileModeLevel;
    [NonEditable] public int shieldLevel;
    [NonEditable] public int blueBeamLevel;
    [NonEditable] public int redBeamLevel;
    [NonEditable] public int greenBeamLevel;

    StatusMenuPoints points;

    private void Awake()
    {
        instance = this;
    }

    public void LoadData(GameData data)
    {
        biomatter = data.biomatter;
        gear = data.gear;

        dashUnlocked = data.dashUnlocked;
        blueUnlocked = data.blueUnlocked;
        redUnlocked = data.redUnlocked;
        blockingFistUnlocked = data.blockingFistUnlocked;
        purpleUnlocked = data.purpleUnlocked;
        superJumpUnlocked = data.superJumpUnlocked;
        greenUnlocked = data.greenUnlocked;

        attackLevel = data.attackLevel;
        defenseLevel = data.defenseLevel;
        chargeSpeedLevel = data.chargeSpeedLevel;
        dashCdLevel = data.dashCdLevel;
        proyectileSpeedLevel = data.proyectileSpeedLevel;
        maxHealthLevel = data.maxHealthLevel;
        lifeRegenLevel = data.lifeRegenLevel;
        lifeChargeLevel = data.lifeChargeLevel;

        xRayVisionLevel = data.xRayVisionLevel;
        automaticModeLevel = data.automaticModeLevel;
        tripleShotModeLevel = data.tripleShotModeLevel;
        missileModeLevel = data.missileModeLevel;
        shieldLevel = data.shieldLevel;
        blueBeamLevel = data.blueBeamLevel;
        redBeamLevel = data.redBeamLevel;
        greenBeamLevel = data.greenBeamLevel;

        LateStart();

        CoinDisplay coinDisplay = GameObject.Find("CoinDisplay").GetComponent<CoinDisplay>();
        coinDisplay.biomatterText.text = biomatter.ToString();
        coinDisplay.gearText.text = gear.ToString();

        // Status Menu
        points = GameObject.FindObjectOfType<StatusMenuPoints>();
        if (points == null) return;
        StatusMenuPanel panel0 = points.panels[0].GetComponent<StatusMenuPanel>();
        float[] beamValues = { attackLevel * 5, proyectileSpeedLevel * 10 };
        panel0.UpdateInformation(0, beamValues);
        float[] chargedValues = { chargeSpeedLevel * 7, lifeChargeLevel * 1 };
        panel0.UpdateInformation(1, chargedValues);
        float[] yellowValues = { 25 + 25 * (attackLevel * 0.05f) };
        panel0.UpdateInformation(2, yellowValues);
        if (blueUnlocked)
        {
            float[] blueValues = { 20 + 20 * (attackLevel * 0.05f), 1 + (blueBeamLevel * 0.05f) };
            panel0.UpdateInformation(3, blueValues);
        }
        if (redUnlocked)
        {
            float[] redValues = { 20 + 20 * (attackLevel * 0.05f), 100 + (redBeamLevel * 10) };
            panel0.UpdateInformation(4, redValues);
        }
        if (purpleUnlocked)
        {
            float[] purpleValues = { 20 + 20 * (attackLevel * 0.05f) };
            panel0.UpdateInformation(5, purpleValues);
        }
        if (greenUnlocked)
        {
            float[] greenValues = { 30 + 30 * (attackLevel * 0.05f), (greenBeamLevel * 20) };
            panel0.UpdateInformation(6, greenValues);
        }

        if (superJumpUnlocked)
        {
            points.panels[1].GetComponent<StatusMenuPanel>().UpdateInformation(1, null);
        }

        if (dashUnlocked)
        {
            float[] dashValues = { (dashCdLevel * 3) };
            points.panels[2].GetComponent<StatusMenuPanel>().UpdateInformation(0, dashValues);
        }
        if (defenseLevel > 0)
        {
            float[] defenseValues = { (defenseLevel * 5) };
            points.panels[2].GetComponent<StatusMenuPanel>().UpdateInformation(2, defenseValues);
        }

        if (maxHealthLevel > 0)
        {
            float[] healthValues = { 250 + (maxHealthLevel * 50) };
            points.panels[3].GetComponent<StatusMenuPanel>().UpdateInformation(0, healthValues);
        }
        if (lifeRegenLevel > 0)
        {
            float[] regenValues = { (lifeRegenLevel * 0.2f) };
            points.panels[3].GetComponent<StatusMenuPanel>().UpdateInformation(1, regenValues);
        }

        if (blockingFistUnlocked)
        {
            points.panels[4].GetComponent<StatusMenuPanel>().UpdateInformation(0, null);
        }
    }

    public void SaveData(GameData data)
    {
        data.biomatter = biomatter;
        data.gear = gear;

        data.dashUnlocked = dashUnlocked;
        data.blueUnlocked = blueUnlocked;
        data.redUnlocked = redUnlocked;
        data.blockingFistUnlocked = blockingFistUnlocked;
        data.purpleUnlocked = purpleUnlocked;
        data.superJumpUnlocked = superJumpUnlocked;
        data.greenUnlocked = greenUnlocked;

        data.attackLevel = attackLevel;
        data.defenseLevel = defenseLevel;
        data.chargeSpeedLevel = chargeSpeedLevel;
        data.dashCdLevel = dashCdLevel;
        data.proyectileSpeedLevel = proyectileSpeedLevel;
        data.maxHealthLevel = maxHealthLevel;
        data.lifeRegenLevel = lifeRegenLevel;
        data.lifeChargeLevel = lifeChargeLevel;

        data.xRayVisionLevel = xRayVisionLevel;
        data.automaticModeLevel = automaticModeLevel;
        data.tripleShotModeLevel = tripleShotModeLevel;
        data.missileModeLevel = missileModeLevel;
        data.shieldLevel = shieldLevel;
        data.blueBeamLevel = blueBeamLevel;
        data.redBeamLevel = redBeamLevel;
        data.greenBeamLevel = greenBeamLevel;
    }

    void LateStart()
    {
        if (blockingFistUnlocked) leftHandCollision.tag = "ShieldedHand";
    }

    public void UnlockSkill(SKILL_TYPE type)
    {
        switch (type)
        {
            case SKILL_TYPE.DASH:
                dashUnlocked = true;
                playerMovement.dashUnlocked = true;
                break;
            case SKILL_TYPE.BLUE:
                blueUnlocked = true;
                if (playerGun)
                {
                    playerGun.blueUnlocked = true;
                    playerGun.NewBeamUnlocked(GUN_TYPE.BLUE);
                }
                float[] blueValues = { 20 + 20 * (attackLevel * 0.05f), 1 + (blueBeamLevel * 0.05f) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(3, blueValues);
                break;
            case SKILL_TYPE.RED:
                redUnlocked = true;
                if (playerGun)
                {
                    playerGun.redUnlocked = true;
                    playerGun.NewBeamUnlocked(GUN_TYPE.RED);
                }
                float[] redValues = { 20 + 20 * (attackLevel * 0.05f), 100 + (redBeamLevel * 10) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(4, redValues);
                break;
            case SKILL_TYPE.BLOCKING_FIST:
                blockingFistUnlocked = true;
                if (blockingFistUnlocked) leftHandCollision.tag = "ShieldedHand";
                StatusMenuPoints.instance.panels[4].GetComponent<StatusMenuPanel>().UpdateInformation(0, null);
                break;
            case SKILL_TYPE.PURPLE:
                purpleUnlocked = true;
                if (playerGun)
                {
                    playerGun.purpleUnlocked = true;
                    playerGun.NewBeamUnlocked(GUN_TYPE.PURPLE);
                }
                float[] purpleValues = { 20 + 20 * (attackLevel * 0.05f) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(5, purpleValues);
                break;
            case SKILL_TYPE.SUPER_JUMP:
                superJumpUnlocked = true;
                playerMovement.superJumpUnlocked = true;
                StatusMenuPoints.instance.panels[1].GetComponent<StatusMenuPanel>().UpdateInformation(1, null);
                break;
            case SKILL_TYPE.GREEN:
                greenUnlocked = true;
                if (playerGun)
                {
                    playerGun.greenUnlocked = true;
                    playerGun.NewBeamUnlocked(GUN_TYPE.GREEN);
                }
                float[] greenValues = { 30 + 30 * (attackLevel * 0.05f), (greenBeamLevel * 20) };
                StatusMenuPoints.instance.panels[0].GetComponent<StatusMenuPanel>().UpdateInformation(6, greenValues);
                break;
            default:
                break;
        }
    }

    public int GetUpgradeLevel()
    {
        int level = 0;

        if (dashUnlocked) level++;
        else return 0;
        if (redUnlocked && blueUnlocked && blockingFistUnlocked) level++;
        else return 1;
        if (purpleUnlocked && superJumpUnlocked) level++;
        else return 2;
        if (greenUnlocked) return -1; // -1 mean all skills unlocked
        else return 3;
    }

    public bool CheckIfSkillIsOwned(SKILL_TYPE type)
    {
        switch (type)
        {
            case SKILL_TYPE.DASH:
                if (dashUnlocked) return true;
                else return false;
            case SKILL_TYPE.BLUE:
                if (blueUnlocked) return true;
                else return false;
            case SKILL_TYPE.RED:
                if (redUnlocked) return true;
                else return false;
            case SKILL_TYPE.BLOCKING_FIST:
                if (blockingFistUnlocked) return true;
                else return false;
            case SKILL_TYPE.PURPLE:
                if (purpleUnlocked) return true;
                else return false;
            case SKILL_TYPE.SUPER_JUMP:
                if (superJumpUnlocked) return true;
                else return false;
            case SKILL_TYPE.GREEN:
                if (greenUnlocked) return true;
                else return false;
            default:
                return false;
        }
    }

    public void AddBiomatter(int amount)
    {
        biomatter += amount;
        CoinDisplay.instance.biomatterText.text = biomatter.ToString();
    }

    public void AddGear(int amount)
    {
        gear += amount;
        CoinDisplay.instance.gearText.text = gear.ToString();
    }
}
