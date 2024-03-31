using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshForXRay : MonoBehaviour
{
    PlayerState playerState;
    bool xRayLayer = false;
    MeshRenderer m_Renderer;
    [SerializeField] string layerName;

    void Start()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        playerState = PlayerState.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!xRayLayer && playerState.xRayVisionActive)
        {
            gameObject.layer = LayerMask.NameToLayer("XRayEnemy");
            xRayLayer = true;
            m_Renderer.enabled = true;
        }
        else if (xRayLayer && !playerState.xRayVisionActive)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
            xRayLayer = false;
            m_Renderer.enabled = false;
        }
    }
}
