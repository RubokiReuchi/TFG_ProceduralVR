using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum POWER_UP_TYPE
{
    HAND_OF_THE_GIANT,
    XRAY_VISION,
    AUTOMATIC_MODE
}

public class PowerUp : MonoBehaviour
{
    public POWER_UP_TYPE type;
    public POWER_UP_TYPE[] incompatibilities;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerPowerUps.instance.GetPowerUp(type);
        Destroy(gameObject);
    }
}
