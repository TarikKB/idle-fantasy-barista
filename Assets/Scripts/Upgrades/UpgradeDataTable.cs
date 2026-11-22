using UnityEngine;
using System.Data;

public class UpgradeLevelData
{
    public float baseCost;
    public float costMultiplier;
    public float baseValue;
    public float valueScale;

    // Standard idle-game exponential cost curve: cost = baseCost * growthRate^level (rounded to whole gold)
    public float GetCost(int level)
    {
        return Mathf.Round(baseCost * Mathf.Pow(costMultiplier, level));
    }

    // Linear effect growth: value = baseValue + level * valueScale
    public float GetValue(int level)
    {
        return baseValue + level * valueScale;
    }
}

public class UpgradeDataTable : MonoBehaviour
{
    // public UpgradeData upgradeData;
    private ResourceManagerScript resourceManager;
    [SerializeField] private UpgradeListing beanRateListing;
    [SerializeField] private UpgradeListing beanLimitListing;
    [SerializeField] private UpgradeListing sellPriceListing;
    [SerializeField] private UpgradeListing ticketRateListing;
    [SerializeField] private UpgradeListing fameBonusListing;

    public UpgradeLevelData beanRateUpgrade;
    public UpgradeLevelData beanLimitUpgrade;
    public UpgradeLevelData sellPriceUpgrade;
    public UpgradeLevelData ticketRateUpgrade;
    public UpgradeLevelData fameBonusUpgrade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        // upgradeData = resourceManager.upgradeData;

        beanRateUpgrade = UpgradeRules.BeanRate;
        beanRateListing.upgradeLevelData = beanRateUpgrade;
        beanLimitUpgrade = UpgradeRules.BeanLimit;
        beanLimitListing.upgradeLevelData = beanLimitUpgrade;
        sellPriceUpgrade = UpgradeRules.SalePrice;
        sellPriceListing.upgradeLevelData = sellPriceUpgrade;
        ticketRateUpgrade = UpgradeRules.Tickets;
        ticketRateListing.upgradeLevelData = ticketRateUpgrade;
        fameBonusUpgrade = UpgradeRules.Fame;
        fameBonusListing.upgradeLevelData = fameBonusUpgrade;
    }

    public void GetData()
    {
        beanRateListing.currentLevel = resourceManager.upgradeData.beanRateLevel;
        beanLimitListing.currentLevel = resourceManager.upgradeData.beanLimitLevel;
        sellPriceListing.currentLevel = resourceManager.upgradeData.sellPriceLevel;
        ticketRateListing.currentLevel = resourceManager.upgradeData.ticketRateLevel;
        fameBonusListing.currentLevel = resourceManager.upgradeData.fameBonusLevel;

        beanRateListing.UpdateText();
        beanLimitListing.UpdateText();
        sellPriceListing.UpdateText();
        ticketRateListing.UpdateText();
        fameBonusListing.UpdateText();
    }


    public void TryUpgradeBeanRate()
    {
        if (resourceManager.UpgradeBeanRate(beanRateUpgrade.GetCost(resourceManager.upgradeData.beanRateLevel)))
        {
            beanRateListing.currentLevel = resourceManager.upgradeData.beanRateLevel;
            beanRateListing.UpdateText();
            print(resourceManager.upgradeData.beanRateLevel);
        }
    }

    public void TryUpgradeBeanLimit()
    {
        if (resourceManager.UpgradeBeanLimit(beanLimitUpgrade.GetCost(resourceManager.upgradeData.beanLimitLevel)))
        {
            beanLimitListing.currentLevel = resourceManager.upgradeData.beanLimitLevel;
            beanLimitListing.UpdateText();
        }
    }

    public void TryUpgradeSellPrice()
    {
        if (resourceManager.UpgradeSellPrice(sellPriceUpgrade.GetCost(resourceManager.upgradeData.sellPriceLevel)))
        {
            sellPriceListing.currentLevel = resourceManager.upgradeData.sellPriceLevel;
            sellPriceListing.UpdateText();
        }
    }

    public void TryUpgradeTicketRate()
    {
        if (resourceManager.UpgradeTicketRate(ticketRateUpgrade.GetCost(resourceManager.upgradeData.ticketRateLevel)))
        {
            ticketRateListing.currentLevel = resourceManager.upgradeData.ticketRateLevel;
            ticketRateListing.UpdateText();
        }
    }

    public void TryUpgradeFameBonus()
    {
        if (resourceManager.UpgradeFameBonus(fameBonusUpgrade.GetCost(resourceManager.upgradeData.fameBonusLevel)))
        {
            fameBonusListing.currentLevel = resourceManager.upgradeData.fameBonusLevel;
            fameBonusListing.UpdateText();
        }
    }

}
