using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatusMenuDescription : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] info;
    [SerializeField] string[] frontLabel;
    [SerializeField] string[] backLabel;

    public void UpdateData(float[] value)
    {
        if (value.Length != info.Length)
        {
            Debug.LogError("Data array has wrong lenght");
            return;
        }
        for (int i = 0; i < info.Length; i++)
        {
            info[i].text = frontLabel[i] + value[i].ToString() + backLabel[i];
        }
    }
}
