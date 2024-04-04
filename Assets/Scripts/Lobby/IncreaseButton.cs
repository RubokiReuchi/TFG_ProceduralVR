using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseButton : MonoBehaviour
{
    [SerializeField] LEVEL_UP type;
    [SerializeField] int level;
    Image image;
    IncreasePanel powerIncrease;
    [SerializeField] IncreaseDescription description;
    [HideInInspector] public bool obtained;

    public void CalculateColor(PlayerSkills skills)
    {
        if (image == null) image = GetComponent<Image>();
        if (powerIncrease == null) powerIncrease = GetComponentInParent<IncreasePanel>();

        int skillLevel;
        switch (type)
        {
            case LEVEL_UP.ATTACK: skillLevel = skills.attackLevel; break;
            case LEVEL_UP.DEFENSE: skillLevel = skills.defenseLevel; break;
            case LEVEL_UP.CHARGE_SPEED: skillLevel = skills.chargeSpeedLevel; break;
            case LEVEL_UP.DASH_CD: skillLevel = skills.dashCdLevel; break;
            case LEVEL_UP.PROYECTILE_SPEED: skillLevel = skills.proyectileSpeedLevel; break;
            case LEVEL_UP.LIFE_REGEN: skillLevel = skills.lifeRegenLevel; break;
            case LEVEL_UP.LIFE_CHARGE: skillLevel = skills.lifeChargeLevel; break;
            case LEVEL_UP.XRAY_VISION: skillLevel = skills.xRayVisionLevel; break;
            case LEVEL_UP.AUTOMATIC_MODE: skillLevel = skills.automaticModeLevel; break;
            case LEVEL_UP.TRIPLE_SHOT_MODE: skillLevel = skills.tripleShotModeLevel; break;
            case LEVEL_UP.MISSILE_MODE: skillLevel = skills.missileModeLevel; break;
            case LEVEL_UP.SHIELD: skillLevel = skills.shieldLevel; break;
            case LEVEL_UP.BEAM: skillLevel = skills.beamLevel; break;
            default: skillLevel = 0; break;
        }

        if (level <= skillLevel)
        {
            image.color = powerIncrease.obtainedColor;
            image.raycastTarget = true;
            obtained = true;
        }
        else if (level == skillLevel + 1)
        {
            image.color = powerIncrease.unselectedColor;
            image.raycastTarget = true;
            obtained = false;
        }
        else
        {
            image.color = powerIncrease.blockedColor;
            image.raycastTarget = false;
            obtained = false;
        }
    }

    public void SelectButton()
    {
        image.color = obtained ? powerIncrease.obtainedSelectedColor : powerIncrease.selectedColor;
        image.raycastTarget = false;
        description.ButtonSelected(type, level, obtained);
    }
}
