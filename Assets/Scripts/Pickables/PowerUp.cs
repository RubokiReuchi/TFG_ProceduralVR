using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum POWER_UP_TYPE
{
    HAND_OF_THE_GIANT,
    XRAY_VISION,
    AUTOMATIC_MODE,
    TRIPLE_SHOT,
    MISILE_MODE,
    CHARGED_EXPLOSION,
    AIR_DASH,
    DOBLE_JUMP,
    SHIELD
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
