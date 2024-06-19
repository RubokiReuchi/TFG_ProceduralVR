using System.Collections;
using TMPro;
using UnityEngine;

public class IncreaseDescription : MonoBehaviour
{
    [SerializeField] IncreasePanel increasePanel;
    [SerializeField] LobbyPanel otherLobbyPanel;
    [SerializeField] COIN usedCoin;
    Animator animator;
    bool shown = false;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] TextMeshProUGUI price;
    [SerializeField] GameObject purchaseImage;
    [SerializeField] GameObject priceImage;
    [SerializeField] Animator priceAnimator;
    GameObject currentIcon;
    [SerializeField] Transform iconContainer;
    [SerializeField] GameObject[] icons;
    [SerializeField] TextAsset increasePowerDescriptionsCSV;
    static int numOfColumns = 16;
    static int numOfRows = 5;
    string[] titles = new string[numOfColumns];
    string[,] descriptions = new string[numOfColumns, numOfRows];
    int[] prices = { 50, 150, 250, 350, 450 };
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

    public void ButtonSelected(LEVEL_UP type, int level, bool displayPrice, bool displayPurchase)
    {
        if (!shown)
        {
            shown = true;
            SetInfo(type, level);
            priceImage.SetActive(displayPrice);
            purchaseImage.SetActive(displayPurchase);
            animator.SetBool("Shown", true);
        }
        else
        {
            StartCoroutine(ChangeInfo(type, level, displayPrice, displayPurchase));
            animator.SetTrigger("ChangeInfoShown");
        }
        otherLobbyPanel.MoveBack();

        selectedType = type;
    }

    void SetInfo(LEVEL_UP type, int level)
    {
        title.text = titles[(int)type].ToString() + " " + level;
        description.text = descriptions[(int)type, level - 1].ToString();
        price.text = prices[level - 1].ToString();
        int iconIndex;
        switch (type)
        {
            case LEVEL_UP.ATTACK:
            case LEVEL_UP.DEFENSE:
            case LEVEL_UP.CHARGE_SPEED:
            case LEVEL_UP.DASH_CD:
            case LEVEL_UP.PROYECTILE_SPEED:
            case LEVEL_UP.MAX_HEALTH:
            case LEVEL_UP.LIFE_REGEN:
            case LEVEL_UP.LIFE_CHARGE:
                iconIndex = (int)type;
                break;
            case LEVEL_UP.XRAY_VISION:
                if (level == 1 || level == 3 || level == 5) iconIndex = 8;
                else iconIndex = 9;
                break;
            case LEVEL_UP.AUTOMATIC_MODE:
                iconIndex = 10;
                break;
            case LEVEL_UP.TRIPLE_SHOT_MODE:
                iconIndex = 11;
                break;
            case LEVEL_UP.MISSILE_MODE:
                if (level == 1 || level == 3 || level == 5) iconIndex = 12;
                else iconIndex = 13;
                break;
            case LEVEL_UP.SHIELD:
                iconIndex = 14;
                break;
            case LEVEL_UP.BLUE_BEAM:
                iconIndex = 15;
                break;
            case LEVEL_UP.RED_BEAM:
                iconIndex = 16;
                break;
            case LEVEL_UP.GREEN_BEAM:
                iconIndex = 17;
                break;
            default:
                Debug.LogError("Level wrong asigned");
                iconIndex = 0;
                break;
        }
        currentIcon = GameObject.Instantiate(icons[iconIndex], iconContainer);
    }

    IEnumerator ChangeInfo(LEVEL_UP type, int level, bool displayPrice, bool displayPurchase)
    {
        yield return new WaitForSeconds(0.5f);
        title.text = titles[(int)type].ToString() + " " + level;
        description.text = descriptions[(int)type, level - 1].ToString();
        price.text = prices[level - 1].ToString();
        Destroy(currentIcon);
        int iconIndex;
        switch (type)
        {
            case LEVEL_UP.ATTACK:
            case LEVEL_UP.DEFENSE:
            case LEVEL_UP.CHARGE_SPEED:
            case LEVEL_UP.DASH_CD:
            case LEVEL_UP.PROYECTILE_SPEED:
            case LEVEL_UP.MAX_HEALTH:
            case LEVEL_UP.LIFE_REGEN:
            case LEVEL_UP.LIFE_CHARGE:
                iconIndex = (int)type;
                break;
            case LEVEL_UP.XRAY_VISION:
                if (level == 1 || level == 3 || level == 5) iconIndex = 8;
                else iconIndex = 9;
                break;
            case LEVEL_UP.AUTOMATIC_MODE:
                iconIndex = 10;
                break;
            case LEVEL_UP.TRIPLE_SHOT_MODE:
                iconIndex = 11;
                break;
            case LEVEL_UP.MISSILE_MODE:
                if (level == 1 || level == 3 || level == 5) iconIndex = 12;
                else iconIndex = 13;
                break;
            case LEVEL_UP.SHIELD:
                iconIndex = 14;
                break;
            case LEVEL_UP.BLUE_BEAM:
                iconIndex = 0;
                break;
            case LEVEL_UP.RED_BEAM:
                iconIndex = 0;
                break;
            case LEVEL_UP.GREEN_BEAM:
                iconIndex = 0;
                break;
            default:
                Debug.LogError("Level wrong asigned");
                iconIndex = 0;
                break;
        }
        currentIcon = GameObject.Instantiate(icons[iconIndex], iconContainer);
        priceImage.SetActive(displayPrice);
        purchaseImage.SetActive(displayPurchase);
    }

    public void HidePanel()
    {
        if (!shown) return;
        shown = false;
        animator.SetBool("Shown", false);
    }

    public void Purchase()
    {
        int currency = 0;
        switch (usedCoin)
        {
            case COIN.BIOMATTER: currency = inventory.biomatter; break;
            case COIN.GEAR: currency = inventory.gear; break;
        }
        int priceAmount = int.Parse(price.text);
        if (currency >= priceAmount)
        {
            switch (selectedType)
            {
                case LEVEL_UP.ATTACK: inventory.attackLevel++; break;
                case LEVEL_UP.DEFENSE: inventory.defenseLevel++; break;
                case LEVEL_UP.CHARGE_SPEED: inventory.chargeSpeedLevel++; break;
                case LEVEL_UP.DASH_CD: inventory.dashCdLevel++; break;
                case LEVEL_UP.PROYECTILE_SPEED: inventory.proyectileSpeedLevel++; break;
                case LEVEL_UP.MAX_HEALTH:
                    inventory.maxHealthLevel++;
                    PlayerState.instance.IncreaseMaxHealth();
                    break;
                case LEVEL_UP.LIFE_REGEN: inventory.lifeRegenLevel++; break;
                case LEVEL_UP.LIFE_CHARGE: inventory.lifeChargeLevel++; break;
                case LEVEL_UP.XRAY_VISION: inventory.xRayVisionLevel++; break;
                case LEVEL_UP.AUTOMATIC_MODE: inventory.automaticModeLevel++; break;
                case LEVEL_UP.TRIPLE_SHOT_MODE: inventory.tripleShotModeLevel++; break;
                case LEVEL_UP.MISSILE_MODE: inventory.missileModeLevel++; break;
                case LEVEL_UP.SHIELD: inventory.shieldLevel++; break;
                case LEVEL_UP.BLUE_BEAM: inventory.blueBeamLevel++; break;
                case LEVEL_UP.RED_BEAM: inventory.redBeamLevel++; break;
                case LEVEL_UP.GREEN_BEAM: inventory.greenBeamLevel++; break;
            }

            //money spend
            switch (usedCoin)
            {
                case COIN.BIOMATTER: inventory.AddBiomatter(-priceAmount); break;
                case COIN.GEAR: inventory.AddGear(-priceAmount); break;
            }

            CloseAfterPurchase();
            increasePanel.purchaseSource.Play();
        }
        else
        {
            priceAnimator.SetTrigger("Blink");
            increasePanel.errorSource.Play();
        }
    }

    public void Close()
    {
        shown = false;
        animator.SetBool("Shown", false);
        increasePanel.Deselect(false);
        otherLobbyPanel.MoveFront();
        increasePanel.closeDescriptionSource.Play();
    }

    void CloseAfterPurchase()
    {
        shown = false;
        animator.SetBool("Shown", false);
        increasePanel.Deselect(true);
        otherLobbyPanel.MoveFront();
    }
}
