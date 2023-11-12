using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPowerUps : MonoBehaviour
{
    public static PlayerPowerUps instance;

    [SerializeField] Transform leftHand;
    [SerializeField] PlayerGun playerGun;

    private void Awake()
    {
        instance = this;
    }

    public void GetPowerUp(POWER_UP_TYPE type)
    {
        switch (type)
        {
            case POWER_UP_TYPE.HAND_OF_THE_GIANT:
                StartCoroutine(GetHandOfTheGiant());
                break;
            case POWER_UP_TYPE.XRAY_VISION:
                break;
            case POWER_UP_TYPE.AUTOMATIC_MODE:
                playerGun.projectileType = PROJECTILE_TYPE.AUTOMATIC;
                break;
            default:
                break;
        }
    }

    IEnumerator GetHandOfTheGiant()
    {
        float initialScale = leftHand.localScale.x;
        float currentScale = initialScale;
        while (currentScale < initialScale * 2.0f)
        {
            currentScale += Time.deltaTime;
            if (currentScale > initialScale * 2.0f) currentScale = initialScale * 2.0f;
            leftHand.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
    }
}
