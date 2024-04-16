using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinDisplay : MonoBehaviour
{
    public static CoinDisplay instance;

    public TextMeshProUGUI biomatterText;
    public TextMeshProUGUI gearText;

    private void Awake()
    {
        instance = this;
    }
}
