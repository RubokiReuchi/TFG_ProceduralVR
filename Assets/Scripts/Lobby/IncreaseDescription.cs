using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncreaseDescription : MonoBehaviour
{
    [SerializeField] LobbyPanel otherLobbyPanel;
    Animator animator;
    bool shown = false;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;
    GameObject currentIcon;
    [SerializeField] Transform iconContainer;
    [SerializeField] GameObject[] icons;
    [SerializeField] TextAsset increasePowerDescriptionsCSV;
    static int numOfColumns = 8;
    static int numOfRows = 5;
    string[] titles = new string[numOfColumns];
    string[,] descriptions = new string[numOfColumns, numOfRows];

    void Start()
    {
        animator = GetComponent<Animator>();

        string[] data = increasePowerDescriptionsCSV.text.Split(new string[] { ";", "\r\n" }, System.StringSplitOptions.None);

        for (int i = 1; i <= numOfColumns; i++)
        {
            titles[i - 1] = data[i];
            for (int j = 1; j <= numOfRows; j++)
            {
                descriptions[i - 1, j - 1] = data[i + (numOfColumns + 1) * j];
            }
        }
    }

    public void ButtonSelected(LEVEL_UP type, int level)
    {
        if (!shown)
        {
            shown = true;
            SetInfo(type, level);
            animator.SetBool("Shown", true);
        }
        else
        {
            StartCoroutine(ChangeInfo(type, level));
            animator.SetTrigger("ChangeInfoShown");
        }
        otherLobbyPanel.MoveBack();
    }

    void SetInfo(LEVEL_UP type, int level)
    {
        title.text = titles[(int)type].ToString() + " " + level;
        description.text = descriptions[(int)type, level].ToString();
        currentIcon = GameObject.Instantiate(icons[(int)type], iconContainer);
    }

    IEnumerator ChangeInfo(LEVEL_UP type, int level)
    {
        yield return new WaitForSeconds(0.5f);
        title.text = titles[(int)type].ToString() + " " + level;
        description.text = descriptions[(int)type, level].ToString();
        Destroy(currentIcon);
        currentIcon = GameObject.Instantiate(icons[(int)type], iconContainer);
    }

    public void HidePanel()
    {
        if (!shown) return;
        animator.SetBool("Shown", false);
    }
}
