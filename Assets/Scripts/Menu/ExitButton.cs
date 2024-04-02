using System;
using System.IO;
using TMPro;
using UnityEngine;

public class ExitButton : MenuButton
{
    [SerializeField] TextMeshPro text;

    bool sure = false;
    bool paused = false;

    public override void ButtonHitted()
    {
        if (paused) return;

        if (!sure)
        {
            paused = true;
            text.text = "Are you SURE\n to exit?";
            Invoke("AreYouSure", 0.5f);
        }
        else
        {
            Application.Quit();
            Debug.Log("Exit");
        }
    }

    void AreYouSure()
    {
        paused = false;
        sure = true;
        Invoke("NotSure", 3.0f);
    }

    void NotSure()
    {
        sure = false;
        text.text = "Exit";
    }
}
