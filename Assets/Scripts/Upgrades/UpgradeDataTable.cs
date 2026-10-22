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

        // Idle-game standard: exponential cost growth of 1.07-1.15 per level, linear effect growth.

        // Core income driver (beans are the production bottleneck). +0.25 beans/s per level.
        beanRateUpgrade = new UpgradeLevelData
        {
            baseCost = 10.0f,
            costMultiplier = 1.15f,
            baseValue = 1.0f,
            valueScale = 0.25f
        };
        beanRateListing.upgradeLevelData = beanRateUpgrade;


        // Storage cap (mainly governs offline/idle accumulation). +25 beans per level, gentler curve.
        beanLimitUpgrade = new UpgradeLevelData
        {
            baseCost = 15.0f,
            costMultiplier = 1.12f,
            baseValue = 100.0f,
            valueScale = 25.0f
        };
        beanLimitListing.upgradeLevelData = beanLimitUpgrade;
        // beanLimitListing.currentLevel = resourceManager.upgradeData.beanLimitLevel;

        // Global income multiplier. +10% sell price per level.
        sellPriceUpgrade = new UpgradeLevelData
        {
            baseCost = 50.0f,
            costMultiplier = 1.15f,
            baseValue = 1.0f,
            valueScale = 0.10f
        };
        sellPriceListing.upgradeLevelData = sellPriceUpgrade;
        // sellPriceListing.currentLevel = resourceManager.upgradeData.sellPriceLevel;

        // Premium-currency drop chance. 1% base, +0.5% per level, steeper curve since tickets are premium.
        ticketRateUpgrade = new UpgradeLevelData
        {
            baseCost = 100.0f,
            costMultiplier = 1.20f,
            baseValue = 0.01f,
            valueScale = 0.005f
        };
        ticketRateListing.upgradeLevelData = ticketRateUpgrade;
        // ticketRateListing.currentLevel = resourceManager.upgradeData.ticketRateLevel;

        // Fame XP multiplier. +10% XP per level.
        fameBonusUpgrade = new UpgradeLevelData
        {
            baseCost = 75.0f,
            costMultiplier = 1.15f,
            baseValue = 1.0f,
            valueScale = 0.10f
        };
        fameBonusListing.upgradeLevelData = fameBonusUpgrade;
        // fameBonusListing.currentLevel = resourceManager.upgradeData.fameBonusLevel;
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
