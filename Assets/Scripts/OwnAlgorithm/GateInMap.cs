using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GateInMap : MonoBehaviour
{
    Image image;
    bool hided;

    // Start is called before the first frame update
    public void SetUp()
    {
        hided = false;
        image = GetComponent<Image>();
    }

    public void ShowGate()
    {
        if (hided) return;

        image.enabled = true;
    }

    public void HideGate()
    {
        image.enabled = false;
        hided = true;
    }
}
