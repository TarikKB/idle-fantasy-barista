public static class UpgradeRules
{
    public static UpgradeLevelData BeanRate => new UpgradeLevelData { baseCost = 10, costMultiplier = 1.15f, baseValue = 1, valueScale = .25f };
    public static UpgradeLevelData BeanLimit => new UpgradeLevelData { baseCost = 15, costMultiplier = 1.12f, baseValue = 100, valueScale = 25 };
    public static UpgradeLevelData SalePrice => new UpgradeLevelData { baseCost = 50, costMultiplier = 1.15f, baseValue = 1, valueScale = .1f };
    public static UpgradeLevelData Tickets => new UpgradeLevelData { baseCost = 100, costMultiplier = 1.2f, baseValue = .01f, valueScale = .005f };
    public static UpgradeLevelData Fame => new UpgradeLevelData { baseCost = 75, costMultiplier = 1.15f, baseValue = 1, valueScale = .1f };
}
