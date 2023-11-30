using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpike : MonoBehaviour
{
    Vector3 targetScale;

    void Start()
    {
        targetScale = transform.localScale;
    }

    void Update()
    {
        if (targetScale.x < transform.localScale.x)
        {
            transform.localScale -= Vector3.one * Time.deltaTime * 0.2f;
        }
        if (transform.localScale.x <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void Melt(float amount)
    {
        targetScale -= Vector3.one * amount / 100.0f;
        if (targetScale.x < 0.4f) targetScale = Vector3.zero;
    }
}
