using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour, IDataPersistence
{
    public static PlayerSkills instance;

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
    [NonEditable] public int lifeRegenLevel;
    [NonEditable] public int lifeChargeLevel;

    private void Awake()
    {
        instance = this;
    }

    public void LoadData(GameData data)
    {
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
        lifeRegenLevel = data.lifeRegenLevel;
        lifeChargeLevel = data.lifeChargeLevel;

        LateStart();
    }

    public void SaveData(ref GameData data)
    {
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
        data.lifeRegenLevel = lifeRegenLevel;
        data.lifeChargeLevel = lifeChargeLevel;
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
                playerGun.blueUnlocked = true;
                break;
            case SKILL_TYPE.RED:
                redUnlocked = true;
                playerGun.redUnlocked = true;
                break;
            case SKILL_TYPE.BLOCKING_FIST:
                blockingFistUnlocked = true;
                if (blockingFistUnlocked) leftHandCollision.tag = "ShieldedHand";
                break;
            case SKILL_TYPE.PURPLE:
                purpleUnlocked = true;
                playerGun.purpleUnlocked = true;
                break;
            case SKILL_TYPE.SUPER_JUMP:
                superJumpUnlocked = true;
                playerMovement.superJumpUnlocked = true;
                break;
            case SKILL_TYPE.GREEN:
                greenUnlocked = true;
                playerGun.greenUnlocked = true;
                playerGun.UpgradeToGreen();
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
}
