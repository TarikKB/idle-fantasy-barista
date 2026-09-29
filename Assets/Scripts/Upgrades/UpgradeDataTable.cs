using UnityEngine;
using System.Data;

public class UpgradeLevelData
{
    public float baseCost;
    public float costMultiplier;
    public float baseValue;
    public float valueScale;

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

        beanRateUpgrade = new UpgradeLevelData
        {
            baseCost = 5.0f,
            costMultiplier = 2.0f,
            baseValue = 1.0f,
            valueScale = 0.5f
        };
        beanRateListing.upgradeLevelData = beanRateUpgrade;


        beanLimitUpgrade = new UpgradeLevelData
        {
            baseCost = 10.0f,
            costMultiplier = 2.0f,
            baseValue = 100.0f,
            valueScale = 50.0f
        };
        beanLimitListing.upgradeLevelData = beanLimitUpgrade;
        // beanLimitListing.currentLevel = resourceManager.upgradeData.beanLimitLevel;

        sellPriceUpgrade = new UpgradeLevelData
        {
            baseCost = 25.0f,
            costMultiplier = 2.0f,
            baseValue = 1.0f,
            valueScale = 0.25f
        };
        sellPriceListing.upgradeLevelData = sellPriceUpgrade;
        // sellPriceListing.currentLevel = resourceManager.upgradeData.sellPriceLevel;

        ticketRateUpgrade = new UpgradeLevelData
        {
            baseCost = 50.0f,
            costMultiplier = 2.0f,
            baseValue = 0.01f,
            valueScale = 0.01f
        };
        ticketRateListing.upgradeLevelData = ticketRateUpgrade;
        // ticketRateListing.currentLevel = resourceManager.upgradeData.ticketRateLevel;

        fameBonusUpgrade = new UpgradeLevelData
        {
            baseCost = 50.0f,
            costMultiplier = 2.0f,
            baseValue = 1.0f,
            valueScale = 0.5f
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
        if (resourceManager.UpgradeBeanRate(beanRateUpgrade.baseCost * Mathf.Pow(beanRateUpgrade.costMultiplier, resourceManager.upgradeData.beanRateLevel)))
        {
            beanRateListing.currentLevel = resourceManager.upgradeData.beanRateLevel;
            beanRateListing.UpdateText();
            print(resourceManager.upgradeData.beanRateLevel);
        }
    }

    public void TryUpgradeBeanLimit()
    {
        if (resourceManager.UpgradeBeanLimit(beanLimitUpgrade.baseCost * Mathf.Pow(beanLimitUpgrade.costMultiplier, resourceManager.upgradeData.beanLimitLevel)))
        {
            beanLimitListing.currentLevel = resourceManager.upgradeData.beanLimitLevel;
            beanLimitListing.UpdateText();
        }
    }

    public void TryUpgradeSellPrice()
    {
        if (resourceManager.UpgradeSellPrice(sellPriceUpgrade.baseCost * Mathf.Pow(sellPriceUpgrade.costMultiplier, resourceManager.upgradeData.sellPriceLevel)))
        {
            sellPriceListing.currentLevel = resourceManager.upgradeData.sellPriceLevel;
            sellPriceListing.UpdateText();
        }
    }

    public void TryUpgradeTicketRate()
    {
        if (resourceManager.UpgradeTicketRate(ticketRateUpgrade.baseCost * Mathf.Pow(ticketRateUpgrade.costMultiplier, resourceManager.upgradeData.ticketRateLevel)))
        {
            ticketRateListing.currentLevel = resourceManager.upgradeData.ticketRateLevel;
            ticketRateListing.UpdateText();
        }
    }

    public void TryUpgradeFameBonus()
    {
        if (resourceManager.UpgradeFameBonus(fameBonusUpgrade.baseCost * Mathf.Pow(fameBonusUpgrade.costMultiplier, resourceManager.upgradeData.fameBonusLevel)))
        {
            fameBonusListing.currentLevel = resourceManager.upgradeData.fameBonusLevel;
            fameBonusListing.UpdateText();
        }
    }

}
