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
    MAX_HEALTH,
    LIFE_REGEN,
    LIFE_CHARGE
}

public class IncreasePanel : MonoBehaviour
{
    LobbyPanel[] panels;
    [SerializeField] float triggerDistance;
    bool showing = false;

    GameObject selectedButton;
    public Color unselectedColor;
    public Color blockedColor;
    public Color obtainedColor;
    public Color selectedColor;

    IncreaseButton[] buttons;

    PlayerSkills playerSkills;

    // Start is called before the first frame update
    void Start()
    {
        panels = GetComponentsInChildren<LobbyPanel>();
        buttons = GetComponentsInChildren<IncreaseButton>();
        playerSkills = PlayerSkills.instance;
        foreach (IncreaseButton button in buttons) button.CalculateColor(playerSkills);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 distance = Camera.main.transform.position - transform.position;
        if (distance.magnitude < triggerDistance)
        {
            if (showing) return;
            foreach (LobbyPanel panel in panels) panel.Display(true);
            showing = true;
        }
        else
        {
            if (!showing) return;
            foreach (LobbyPanel panel in panels) panel.Display(false);
            showing = false;
            if (!selectedButton) return;
            Image image = selectedButton.GetComponent<Image>();
            image.color = unselectedColor;
            image.raycastTarget = true;
            selectedButton = null;
        }
    }

    public void SelectButton(GameObject go)
    {
        if (selectedButton)
        {
            if (selectedButton.GetInstanceID() != go.GetInstanceID())
            {
                Image image = selectedButton.GetComponent<Image>();
                image.color = unselectedColor;
                image.raycastTarget = true;
                selectedButton = go;
            }
        }
        else
        {
            selectedButton = go;
        }

        selectedButton.GetComponent<IncreaseButton>().SelectButton();
    }

    public void Deselect(bool obtained)
    {
        Image image = selectedButton.GetComponent<Image>();
        if (obtained)
        {
            foreach (IncreaseButton button in buttons) button.CalculateColor(playerSkills);
        }
        else
        {
            image.color = unselectedColor;
            image.raycastTarget = true;
        }
        selectedButton = null;
    }
}
