using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseMechanicShadow : MonoBehaviour
{
    [SerializeField] LEVEL_UP type;
    [SerializeField] GameObject increaseName;
    [SerializeField] GameObject increaseButtons;

    void Start()
    {
        switch (type)
        {
            case LEVEL_UP.BLUE_BEAM:
                if (!PlayerSkills.instance.blueUnlocked)
                {
                    increaseName.SetActive(false);
                    increaseButtons.SetActive(false);
                }
                else gameObject.SetActive(false);
                break;
            case LEVEL_UP.RED_BEAM:
                if (!PlayerSkills.instance.redUnlocked)
                {
                    increaseName.SetActive(false);
                    increaseButtons.SetActive(false);
                }
                else gameObject.SetActive(false);
                break;
            case LEVEL_UP.GREEN_BEAM:
                if (!PlayerSkills.instance.greenUnlocked)
                {
                    increaseName.SetActive(false);
                    increaseButtons.SetActive(false);
                }
                else gameObject.SetActive(false);
                break;
            default:
                Debug.LogError("Level wrong asigned");
                break;
        }
    }
}
