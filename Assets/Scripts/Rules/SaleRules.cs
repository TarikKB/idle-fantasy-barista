using UnityEngine;

public class SaleRules
{
    public static bool CanSellMachine(CoffeeManagerScript maker, int purchasedMachineCount)
    {
        if (maker == null || maker.IsEmpty())
            return false;

        if (!maker.CanSellMachine())
            return false;

        return purchasedMachineCount > 1;
    }
}