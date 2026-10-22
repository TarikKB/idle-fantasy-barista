using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

    [Header("Fame")]
    public TextMeshProUGUI fameText;
    public int fameLevel {get; private set;}
    private float fameXPThreshold = 100f;
    private float fameXPScale = 150f;
    public float xp {get; private set;}
    [SerializeField] private Slider fameProgressSlider;

    [Header("Upgrade Levels")]
    public UpgradeData upgradeData;
    private UpgradeDataTable upgradeDataTable;

    [Header("Machine Ownership")]
    public int purchasedMachineCount { get; private set; }

    private void OnEnable()
    {
        increaseGoldKey.Enable();
        increaseGoldKey.performed += ctx => AddGold(1000);
        increaseBeansKey.Enable();
        increaseBeansKey.performed += ctx => AddBeans(1000);
    }

    private void Awake()
    {
        // Resolved in Awake so it's available when MobileSaveManager.Start loads the save.
        upgradeDataTable = GetComponent<UpgradeDataTable>();
        if (upgradeDataTable == null)
        {
            upgradeDataTable = FindFirstObjectByType<UpgradeDataTable>();
        }
    }

    void Start()
    {
        // upgradeData.beanRateLevel = 3;
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

    public void RegisterPurchasedMachine()
    {
        purchasedMachineCount++;
    }

    public void RegisterSoldMachine()
    {
        purchasedMachineCount = Mathf.Max(0, purchasedMachineCount - 1);
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

    public void AddFameXP(float amount)
    {
        xp += amount;
        if (xp >= fameXPThreshold)
        {
            xp -= fameXPThreshold;
            fameLevel++;
            fameXPThreshold += fameXPScale;
        }
        UpdateFameText();
    }
    public void SetFameLevel(int level)
    {
        fameLevel = level;
        UpdateFameText();
    }

    public void SetFameXP(float amount)
    {
        xp = amount;
        UpdateFameText();
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

    private void UpdateFameText()
    {
        fameText.text = fameLevel.ToString();
        if (fameProgressSlider != null)
        {
            fameProgressSlider.value = xp / fameXPThreshold;
        }
    }

    [Header("Upgrade Caps")]
    [Tooltip("Maximum ticket drop chance per sale (0-1).")]
    [SerializeField] private float maxTicketChance = 0.25f;

    public float FameXPMultiplier => upgradeDataTable.fameBonusUpgrade.GetValue(upgradeData.fameBonusLevel);

    public float TicketChance => Mathf.Min(upgradeDataTable.ticketRateUpgrade.GetValue(upgradeData.ticketRateLevel), maxTicketChance);

    // Recomputes all upgrade-derived stats from the current upgrade levels (call after loading or resetting).
    public void ApplyUpgradeEffects()
    {
        beanRate = upgradeDataTable.beanRateUpgrade.GetValue(upgradeData.beanRateLevel);
        beanLimit = upgradeDataTable.beanLimitUpgrade.GetValue(upgradeData.beanLimitLevel);
        sellPriceMultiplier = upgradeDataTable.sellPriceUpgrade.GetValue(upgradeData.sellPriceLevel);
    }

    public void TicketCheck()
    {
        float ticketRate = TicketChance;
        // print(ticketRate);
        if (Random.Range(0f, 1f) < ticketRate)
        {
            AddTickets(1);
        }
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

    public void ResetData()
    {
        upgradeData = new UpgradeData();
        SetGold(100);
        SetBeans(0);
        SetTickets(0);
        SetFameLevel(1);
        SetFameXP(0f);
        fameXPThreshold = 100f;
        ApplyUpgradeEffects();
    }
    
}
