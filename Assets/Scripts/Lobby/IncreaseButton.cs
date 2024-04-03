using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IncreaseButton : MonoBehaviour
{
    [SerializeField] LEVEL_UP type;
    [SerializeField] int level;
    Image image;
    PowerIncrease powerIncrease;
    [SerializeField] IncreaseDescription description;

    public void CalculateColor(PlayerSkills skills)
    {
        if (image == null) image = GetComponent<Image>();
        if (powerIncrease == null) powerIncrease = GetComponentInParent<PowerIncrease>();

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
            default: skillLevel = 0; break;
        }

        if (level <= skillLevel)
        {
            image.color = powerIncrease.obtainedColor;
            image.raycastTarget = false;
        }
        else if (level == skillLevel + 1)
        {
            image.color = powerIncrease.unselectedColor;
            image.raycastTarget = true;
        }
        else
        {
            image.color = powerIncrease.blockedColor;
            image.raycastTarget = false;
        }
    }

    public void SelectButton()
    {
        image.color = powerIncrease.selectedColor;
        image.raycastTarget = false;
        description.ButtonSelected(type, level);
    }
}
