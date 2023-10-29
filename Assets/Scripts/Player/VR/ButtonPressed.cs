using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public enum SELECTABLE_STATE
{
    NORMAL,
    HIGHLIGHTED,
    PRESSED,
    SELECTED,
    DISABLED
}

public class ButtonPressed : MonoBehaviour
{
    Material buttonMat;
    Selectable selectable;
    PropertyInfo selectableStateInfo = null;

    private void Awake()
    {
        selectableStateInfo = typeof(Selectable).GetProperty("currentSelectionState", BindingFlags.NonPublic | BindingFlags.Instance);
        buttonMat = new Material(GetComponent<Image>().material);
        GetComponent<Image>().material = buttonMat;
    }

    void Start()
    {
        selectable = GetComponent<Selectable>();
    }

    void Update()
    {
        if (GetState() == SELECTABLE_STATE.PRESSED) buttonMat.SetInt("_Pressed", 1);
        else buttonMat.SetInt("_Pressed", 0);
    }

    SELECTABLE_STATE GetState()
    {
        int selectableState = (int)selectableStateInfo.GetValue(selectable);
        switch (selectableState)
        {
            case 0:
                return SELECTABLE_STATE.NORMAL;
            case 1:
                return SELECTABLE_STATE.HIGHLIGHTED;
            case 2:
                return SELECTABLE_STATE.PRESSED;
            case 3:
                return SELECTABLE_STATE.SELECTED;
            case 4:
                return SELECTABLE_STATE.DISABLED;
        }
        return SELECTABLE_STATE.NORMAL;
    }
}
