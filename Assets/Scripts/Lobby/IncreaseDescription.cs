using Pico.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class IncreaseDescription : MonoBehaviour
{
    [SerializeField] IncreasePanel increasePanel;
    [SerializeField] LobbyPanel otherLobbyPanel;
    Animator animator;
    bool shown = false;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] Animator priceAnimator;
    [SerializeField] Image purchaseImage;
    [SerializeField] Image closeImage;
    GameObject currentIcon;
    [SerializeField] Transform iconContainer;
    [SerializeField] GameObject[] icons;
    [SerializeField] TextAsset increasePowerDescriptionsCSV;
    static int numOfColumns = 8;
    static int numOfRows = 5;
    string[] titles = new string[numOfColumns];
    string[,] descriptions = new string[numOfColumns, numOfRows];
    PlayerSkills inventory;
    LEVEL_UP selectedType;

    void Start()
    {
        animator = GetComponent<Animator>();
        inventory = PlayerSkills.instance;

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

        selectedType = type;
        purchaseImage.raycastTarget = true;
        closeImage.raycastTarget = true;
    }

    void SetInfo(LEVEL_UP type, int level)
    {
        title.text = titles[(int)type].ToString() + " " + level;
        description.text = descriptions[(int)type, level - 1].ToString();
        currentIcon = GameObject.Instantiate(icons[(int)type], iconContainer);
    }

    IEnumerator ChangeInfo(LEVEL_UP type, int level)
    {
        yield return new WaitForSeconds(0.5f);
        title.text = titles[(int)type].ToString() + " " + level;
        description.text = descriptions[(int)type, level - 1].ToString();
        Destroy(currentIcon);
        currentIcon = GameObject.Instantiate(icons[(int)type], iconContainer);
    }

    public void HidePanel()
    {
        if (!shown) return;
        animator.SetBool("Shown", false);
    }

    public void Purchase()
    {
        int priceAmount = int.Parse(price.text);
        if (inventory.biomatter >= priceAmount)
        {
            switch (selectedType)
            {
                case LEVEL_UP.ATTACK: inventory.attackLevel++; break;
                case LEVEL_UP.DEFENSE: inventory.defenseLevel++; break;
                case LEVEL_UP.CHARGE_SPEED: inventory.chargeSpeedLevel++; break;
                case LEVEL_UP.DASH_CD: inventory.dashCdLevel++; break;
                case LEVEL_UP.PROYECTILE_SPEED: inventory.proyectileSpeedLevel++; break;
                case LEVEL_UP.MAX_HEALTH: inventory.maxHealthLevel++; break;
                case LEVEL_UP.LIFE_REGEN: inventory.lifeRegenLevel++; break;
                case LEVEL_UP.LIFE_CHARGE: inventory.lifeChargeLevel++; break;
            }
            inventory.biomatter -= priceAmount;

            Invoke("CloseAfterPurchase", 0.5f);
        }
        else
        {
            priceAnimator.SetTrigger("Blink");
            // Play sound
        }
    }

    public void Close()
    {
        shown = false;
        animator.SetBool("Shown", false);
        purchaseImage.raycastTarget = false;
        closeImage.raycastTarget = false;
        increasePanel.Deselect(false);
        otherLobbyPanel.MoveFront();
    }

    void CloseAfterPurchase()
    {
        shown = false;
        animator.SetBool("Shown", false);
        purchaseImage.raycastTarget = false;
        closeImage.raycastTarget = false;
        increasePanel.Deselect(true);
        otherLobbyPanel.MoveFront();
    }
}
