using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LEVEL_UP
{
    ATTACK,
    DEFENSE,
    CHARGE_SPEED,
    DASH_CD,
    PROYECTILE_SPEED,
    LIFE_REGEN,
    LIFE_CHARGE
}

public class PowerIncrease : MonoBehaviour
{
    GameObject selectedButton;
    public Color unselectedColor;
    public Color blockedColor;
    public Color obtainedColor;
    public Color selectedColor;

    IncreaseButton[] buttons;

    // Start is called before the first frame update
    void Start()
    {
        buttons = GetComponentsInChildren<IncreaseButton>();
        PlayerSkills playerSkills = PlayerSkills.instance;
        foreach (IncreaseButton button in buttons) button.CalculateColor(playerSkills);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectButton(GameObject go)
    {
        if (selectedButton)
        {
            if (selectedButton.GetInstanceID() != go.GetInstanceID())
            {
                selectedButton.GetComponent<Image>().color = unselectedColor;
                go.GetComponent<Image>().color = selectedColor;
                selectedButton = go;
            }
        }
        else
        {
            go.GetComponent<Image>().color = selectedColor;
            selectedButton = go;
        }
    }
}
