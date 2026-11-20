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
    [SerializeField] private InputAction increaseTicketKey;

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
        increaseTicketKey.Enable();
        increaseTicketKey.performed += ctx => AddTickets(100);
    }

    [Header("Employees")]
    [SerializeField] private GCRosterManager employeeRoster;
    private float employeeBeanMultiplier = 1f;
    private float employeeSaleMultiplier = 1f;
    private float employeeFameMultiplier = 1f;
    private float employeeStorageMultiplier = 1f;
    private float employeeCostReduction;
    private float employeeTicketChance;
    private float employeeTipChance;
    [SerializeField, Range(0f, 1f)] private float maxBeanCostReduction = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxTipChance = 0.25f;
    [SerializeField, Min(0f)] private float tipGoldBonus = 0.5f;
    public float BrewingSpeedMultiplier { get; private set; } = 1f;
    public float EffectiveBeanRate => beanRate * employeeBeanMultiplier;
    public float EffectiveBeanLimit => beanLimit * employeeStorageMultiplier;
    public float EffectiveSellPriceMultiplier => sellPriceMultiplier * employeeSaleMultiplier;
    public float TipChance => Mathf.Clamp(employeeTipChance, 0f, maxTipChance);

    public float GetEffectiveBeanCost(CMData machine)
    {
        if (machine == null) return 0f;
        float reduction = employeeCostReduction + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.ArcaneEfficiency, machine) : 0f);
        return machine.beansRequired * (1f - Mathf.Clamp(reduction, 0f, maxBeanCostReduction));
    }

    public float GetBrewingSpeedMultiplier(CMData machine) => BrewingSpeedMultiplier +
        (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.EspressoVirtuoso, machine) : 0f);

    public float GetSellPriceMultiplier(CMData machine) => sellPriceMultiplier * (employeeSaleMultiplier +
        (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.HouseBlendExpert, machine) + employeeRoster.GetBuffBonus(EmployeeBuffType.ManaResonance, machine) : 0f));

    public float GetFameXPMultiplier(CMData machine) => upgradeDataTable.fameBonusUpgrade.GetValue(upgradeData.fameBonusLevel) *
        (employeeFameMultiplier + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.SignatureRoast, machine) : 0f));

    // Isolated roll evaluation permits deterministic verification without changing global random state.
    public float GetTipGold(float coffeeSaleValue, float roll) => roll >= 0f && roll < TipChance ? coffeeSaleValue * tipGoldBonus : 0f;

    private void RefreshEmployeeBuffs()
    {
        employeeBeanMultiplier = 1f + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.BeanGeneration) : 0f);
        employeeSaleMultiplier = 1f + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.SalePrice) : 0f);
        employeeFameMultiplier = 1f + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.FameXP) : 0f);
        BrewingSpeedMultiplier = 1f + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.BrewingSpeed) : 0f);
        employeeStorageMultiplier = 1f + (employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.BeanStorage) : 0f);
        employeeCostReduction = employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.BeanCostReduction) : 0f;
        employeeTicketChance = employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.TicketChance) : 0f;
        employeeTipChance = employeeRoster != null ? employeeRoster.GetBuffBonus(EmployeeBuffType.TipChance) : 0f;
        SetBeans(beans);
    }

    private void OnDestroy()
    {
        if (employeeRoster != null) employeeRoster.OnRosterChanged -= RefreshEmployeeBuffs;
    }

    private void Awake()
    {
        if (employeeRoster != null) employeeRoster.OnRosterChanged += RefreshEmployeeBuffs;
        RefreshEmployeeBuffs();
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
        if (beans < EffectiveBeanLimit)
        {
            beans = Mathf.Min(EffectiveBeanLimit, beans + EffectiveBeanRate * Time.deltaTime);
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
        SetBeans(beans + amount);
    }

    public void SetBeans(float amount)
    {
        beans = Mathf.Clamp(amount, 0f, EffectiveBeanLimit);
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
        if (beansText != null) beansText.text = beans.ToString("F0");
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

    public float FameXPMultiplier => upgradeDataTable.fameBonusUpgrade.GetValue(upgradeData.fameBonusLevel) * employeeFameMultiplier;

    public float TicketChance => Mathf.Clamp(upgradeDataTable.ticketRateUpgrade.GetValue(upgradeData.ticketRateLevel) + employeeTicketChance, 0f, maxTicketChance);

    // Recomputes all upgrade-derived stats from the current upgrade levels (call after loading or resetting).
    public void ApplyUpgradeEffects()
    {
        beanRate = upgradeDataTable.beanRateUpgrade.GetValue(upgradeData.beanRateLevel);
        beanLimit = upgradeDataTable.beanLimitUpgrade.GetValue(upgradeData.beanLimitLevel);
        sellPriceMultiplier = upgradeDataTable.sellPriceUpgrade.GetValue(upgradeData.sellPriceLevel);
        SetBeans(beans);
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
