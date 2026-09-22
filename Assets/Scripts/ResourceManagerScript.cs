using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResourceManagerScript : MonoBehaviour
{
    [Header("Gold")]
    public TextMeshProUGUI goldText;
    public float gold {get; private set;}
    public float sellPriceMultiplier = 1f;
    [SerializeField] private InputAction increaseGoldKey;
    [SerializeField] private InputAction increaseBeansKey;

    [Header("Beans")]
    public TextMeshProUGUI beansText;
    public float beans {get; private set;}
    public float beanRate = 1f;

    public float beanLimit = 100f;

    [Header("Tickets")]
    public TextMeshProUGUI ticketsText;
    public float tickets {get; private set;}

    [Header("Level")]
    public TextMeshProUGUI levelText;
    public int level {get; private set;}
    [SerializeField] private int[] levelUpCosts;
    public float xp {get; private set;}

    [Header("Upgrade Levels")]
    public UpgradeData upgradeData;
    private UpgradeDataTable upgradeDataTable;

    private void OnEnable()
    {
        increaseGoldKey.Enable();
        increaseGoldKey.performed += ctx => AddGold(1000);
        increaseBeansKey.Enable();
        increaseBeansKey.performed += ctx => AddBeans(1000);
    }

    void Start()
    {
        upgradeDataTable = FindFirstObjectByType<UpgradeDataTable>();
        upgradeData.beanRateLevel = 3;
        // AddGold(100);
    }

    // Update is called once per frame
    void Update()
    {
        if (beans < beanLimit)
        {
            beans += beanRate * Time.deltaTime;
            UpdateBeansText();
        }
    }

    public void AddGold(float amount)
    {
        gold += amount;
        UpdateGoldText();
    }

    public void SetGold(float amount)
    {
        gold = amount;
        UpdateGoldText();
    }

    public void AddBeans(float amount)
    {
        beans += amount;
        if (beans > beanLimit)
        {
            beans = beanLimit;
        }
        UpdateBeansText();
    }

    public void SetBeans(float amount)
    {
        beans = amount;
        if (beans > beanLimit)
        {
            beans = beanLimit;
        }
        UpdateBeansText();
    }

    public void AddTickets(float amount)
    {
        tickets += amount;
        UpdateTicketsText();
    }

    public void SetTickets(float amount)
    {
        tickets = amount;
        UpdateTicketsText();
    }

    public void AddXP(float amount)
    {
        xp += amount;
        UpdateLevelText();
    }

    private void UpdateGoldText()
    {
        goldText.text = gold.ToString("F0");
    }

    private void UpdateBeansText()
    {
        beansText.text = beans.ToString("F0");
    }

    private void UpdateTicketsText()
    {
        ticketsText.text = tickets.ToString("F0");
    }

    private void UpdateLevelText()
    {
        levelText.text = level.ToString();
    }

    public bool UpgradeBeanRate(float cost)
    {
        if (cost <= gold)
        {
            AddGold(-cost);
            upgradeData.beanRateLevel++;
            beanRate = 1f + (upgradeData.beanRateLevel * upgradeDataTable.beanRateUpgrade.valueScale);
            return true;
        }
        return false;
    }

    public bool UpgradeBeanLimit(float cost)
    {
        if (cost <= gold)
        {
            AddGold(-cost);
            upgradeData.beanLimitLevel++;
            beanLimit = 100f + (upgradeData.beanLimitLevel * upgradeDataTable.beanLimitUpgrade.valueScale);
            return true;
        }
        return false;
    }

    public bool UpgradeSellPrice(float cost)
    {
        if (cost <= gold)
        {
            AddGold(-cost);
            upgradeData.sellPriceLevel++;
            sellPriceMultiplier = 1f + (upgradeData.sellPriceLevel * upgradeDataTable.sellPriceUpgrade.valueScale);
            return true;
        }
        return false;
    }

    public bool UpgradeTicketRate(float cost)
    {
        if (cost <= gold)
        {
            AddGold(-cost);
            upgradeData.ticketRateLevel++;
            return true;
        }
        return false;
    }

    public bool UpgradeFameBonus(float cost)
    {
        if (cost <= gold)
        {
            AddGold(-cost);
            upgradeData.fameBonusLevel++;
            return true;
        }
        return false;
    }
    
}
