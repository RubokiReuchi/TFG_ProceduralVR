using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectAttack()
    {
        Debug.Log("Success");
    }
}
