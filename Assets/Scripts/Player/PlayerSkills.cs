using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkills : MonoBehaviour, IDataPersistence
{
    public static PlayerSkills instance;


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
                playerGun.SwapGunType(GUN_TYPE.NULL);
                break;
            default:
                break;
        }
    }
}
