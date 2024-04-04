using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LEVEL_UP
{
    // Power
    ATTACK,
    DEFENSE,
    CHARGE_SPEED,
    DASH_CD,
    PROYECTILE_SPEED,
    MAX_HEALTH,
    LIFE_REGEN,
    LIFE_CHARGE,

    // Mechanic
    XRAY_VISION,
    AUTOMATIC_MODE,
    TRIPLE_SHOT_MODE,
    MISSILE_MODE,
    SHIELD,
    BLUE_BEAM,
    RED_BEAM,
    GREEN_BEAM
}

public class IncreasePanel : MonoBehaviour
{
    LobbyPanel[] panels;
    [SerializeField] float triggerDistance;
    [SerializeField] GameObject selectIcon;
    bool showing = false;

    GameObject selectedButton;
    public Color availableColor;
    public Color blockedColor;
    public Color obtainedColor;

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
            selectedButton.GetComponent<Image>().raycastTarget = true;
            selectedButton.GetComponent<IncreaseButton>().CalculateColor(playerSkills);
            selectedButton = null;
            selectIcon.SetActive(false);
        }
    }

    public void SelectButton(GameObject go)
    {
        if (selectedButton)
        {
            if (selectedButton.GetInstanceID() != go.GetInstanceID())
            {
                selectedButton.GetComponent<Image>().raycastTarget = true;
                selectedButton.GetComponent<IncreaseButton>().CalculateColor(playerSkills);
                selectedButton = go;
            }
        }
        else
        {
            selectedButton = go;
            selectIcon.SetActive(true);
        }

        selectedButton.GetComponent<IncreaseButton>().SelectButton();
        selectIcon.transform.position = selectedButton.transform.position;
    }

    public void Deselect(bool purchased)
    {
        if (purchased)
        {
            foreach (IncreaseButton button in buttons) button.CalculateColor(playerSkills);
        }
        else
        {
            selectedButton.GetComponent<IncreaseButton>().CalculateColor(playerSkills);
        }
        selectedButton.GetComponent<Image>().raycastTarget = true;
        selectedButton = null;
        selectIcon.SetActive(false);
    }
}
