using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderPurpleProyectileXRay : MonoBehaviour
{
    MeshRenderer m_renderer;
    PlayerState playerState;
    bool xRayLayer = false;

    void Start()
    {
        m_renderer = GetComponent<MeshRenderer>();
        playerState = PlayerState.instance;
    }

    void Update()
    {
        if (!xRayLayer && playerState.xRayVisionActive)
        {
            if (Physics.Raycast(transform.position, -transform.forward, out RaycastHit hit) && (hit.collider.CompareTag("FoundationsF") || hit.collider.CompareTag("FoundationsW")))
            {
                m_renderer.enabled = true;
                xRayLayer = true;
            }
        }
        else if (xRayLayer && !playerState.xRayVisionActive)
        {
            m_renderer.enabled = false;
            xRayLayer = false;
        }
    }
}
