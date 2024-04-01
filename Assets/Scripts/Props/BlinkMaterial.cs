using System.Collections;
using UnityEngine;

public class BlinkMaterial : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] int blinks;
    [SerializeField] float blinkCooldown;

    // Start is called before the first frame update
    void Start()
    {
        material.SetInt("_Blinking", 0);
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        yield return new WaitForSeconds(blinkCooldown);
        for (int i = 0; i < blinks; i++)
        {
            material.SetInt("_Blinking", 1);
            yield return new WaitForSeconds(0.1f);
            material.SetInt("_Blinking", 0);
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(Blink());
    }
}
