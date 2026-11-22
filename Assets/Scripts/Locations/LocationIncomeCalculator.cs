using System;
using UnityEngine;

public static class LocationIncomeCalculator
{
    public static double Calculate(LocationState location, Func<string, CMData> machineById, Func<string, GCData> staffById,
        Func<GCData, int> staffLevel, float efficiency, float maxCostReduction = .25f, float maxTipChance = .25f, float tipBonus = .5f)
    {
        if (location == null || location.machines == null || float.IsNaN(efficiency)) return 0;
        var upgrades = location.upgrades ?? new UpgradeData();
        double supply = UpgradeRules.BeanRate.GetValue(Math.Max(0, upgrades.beanRateLevel)) *
            (1 + Bonus(location, EmployeeBuffType.BeanGeneration, null, staffById, staffLevel));
        double demand = 0, sales = 0;
        foreach (var slot in location.machines)
        {
            var machine = slot == null ? null : machineById(slot.coffeeMakerId);
            if (machine == null || machine.brewTimeSeconds <= 0 || float.IsNaN(machine.brewTimeSeconds) || float.IsInfinity(machine.brewTimeSeconds)) continue;
            double speed = 1 + Bonus(location, EmployeeBuffType.BrewingSpeed, null, staffById, staffLevel) +
                Bonus(location, EmployeeBuffType.EspressoVirtuoso, machine, staffById, staffLevel);
            double cycles = Math.Max(0, speed) / machine.brewTimeSeconds;
            float reduction = Mathf.Clamp(Bonus(location, EmployeeBuffType.BeanCostReduction, null, staffById, staffLevel) +
                Bonus(location, EmployeeBuffType.ArcaneEfficiency, machine, staffById, staffLevel), 0, maxCostReduction);
            double value = machine.sellPrice * UpgradeRules.SalePrice.GetValue(Math.Max(0, upgrades.sellPriceLevel)) *
                (1 + Bonus(location, EmployeeBuffType.SalePrice, null, staffById, staffLevel) +
                Bonus(location, EmployeeBuffType.HouseBlendExpert, machine, staffById, staffLevel) +
                Bonus(location, EmployeeBuffType.ManaResonance, machine, staffById, staffLevel));
            float tips = Mathf.Clamp(Bonus(location, EmployeeBuffType.TipChance, null, staffById, staffLevel), 0, maxTipChance);
            demand += cycles * Math.Max(0, machine.beansRequired) * (1 - reduction);
            sales += cycles * Math.Max(0, value) * (1 + tips * tipBonus);
        }
        double factor = demand > 0 ? Math.Min(1, Math.Max(0, supply) / demand) : 1;
        double rate = Mathf.Clamp01(efficiency) * factor * sales;
        return double.IsNaN(rate) || double.IsInfinity(rate) ? 0 : Math.Max(0, rate);
    }

    private static float Bonus(LocationState location, EmployeeBuffType type, CMData machine, Func<string, GCData> staffById, Func<GCData, int> level)
    {
        float bonus = 0;
        if (location.staffIds == null) return bonus;
        var used = new System.Collections.Generic.HashSet<string>();
        for (int i = 0; i < Math.Min(3, location.staffIds.Count); i++)
        {
            string id = location.staffIds[i];
            if (string.IsNullOrEmpty(id) || !used.Add(id)) continue;
            var staff = staffById(id);
            if (staff != null && staff.BuffType == type && staff.AppliesTo(machine)) bonus += staff.GetBuffBonus(level(staff));
        }
        return bonus;
    }

    public static double OfflineSeconds(long checkpoint, long now, int cap) => checkpoint <= 0 ? 0 :
        Math.Min(Math.Max(0, (double)now - checkpoint), Math.Max(0, cap));
}
