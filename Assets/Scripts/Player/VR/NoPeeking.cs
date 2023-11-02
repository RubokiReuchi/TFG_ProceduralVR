using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoPeeking : MonoBehaviour
{
    [SerializeField] float fadeSpeed;
    [SerializeField] float sphereCheckSize;
    [SerializeField] LayerMask collisionLayer;
    Material material;
    bool isFadedOut = false;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.CheckSphere(transform.position, sphereCheckSize, collisionLayer))
        {
            CameraFade(1.0f);
            isFadedOut = true;
        }
        else
        {
            if (!isFadedOut) return;
            CameraFade(0.0f);
        }
    }

    void CameraFade(float targetAlpha)
    {
        float fadeSpeed = (targetAlpha == 0.0f) ? this.fadeSpeed : this.fadeSpeed * PlayerState.instance.headSpeed * 10.0f;
        float fadeValue = Mathf.MoveTowards(material.GetFloat("_Opacity"), targetAlpha, Time.deltaTime * fadeSpeed);
        material.SetFloat("_Opacity", fadeValue);

        if (fadeValue <= 0.01f) isFadedOut = false;
    }
}
