using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Coins
    public int biomatter;
    public int gear;

    // Skills
    // tier 0
    public bool dashUnlocked;

    // tier 1
    public bool blueUnlocked;
    public bool redUnlocked;
    public bool blockingFistUnlocked;

    // tier 2
    public bool purpleUnlocked;
    public bool superJumpUnlocked;

    // tier 3
    public bool greenUnlocked;

    // Powers
    public int attackLevel;
    public int defenseLevel;
    public int chargeSpeedLevel;
    public int dashCdLevel;
    public int proyectileSpeedLevel;
    public int maxHealthLevel;
    public int lifeRegenLevel;
    public int lifeChargeLevel;

    // Mechanics
    public int xRayVisionLevel;
    public int automaticModeLevel;
    public int tripleShotModeLevel;
    public int missileModeLevel;
    public int shieldLevel;
    public int blueBeamLevel;
    public int redBeamLevel;
    public int greenBeamLevel;

    // new game values
    public GameData()
    {
        biomatter = 0;
        gear = 0;

        dashUnlocked = false;
        blueUnlocked = false;
        redUnlocked = false;
        blockingFistUnlocked = false;
        purpleUnlocked = false;
        superJumpUnlocked = false;
        greenUnlocked = false;

        attackLevel = 0;
        defenseLevel = 0;
        chargeSpeedLevel = 0;
        dashCdLevel = 0;
        proyectileSpeedLevel = 0;
        maxHealthLevel = 0;
        lifeRegenLevel = 0;
        lifeChargeLevel = 0;

        xRayVisionLevel = 0;
        automaticModeLevel = 0;
        tripleShotModeLevel = 0;
        missileModeLevel = 0;
        shieldLevel = 0;
        blueBeamLevel = 0;
        redBeamLevel = 0;
        greenBeamLevel = 0;
    }
}
