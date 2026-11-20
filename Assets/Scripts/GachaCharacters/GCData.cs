using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Character Data", menuName = "Gacha/Character Info")]
public class GCData : ScriptableObject
{
    [Header("Identity")]
    // Stable save key; do not rename after shipping.
    public string id;
    public string characterName;
    public int tier;

    [Header("Employee Buff")]
    [SerializeField] private EmployeeBuffType buffType;
    [SerializeField, Min(0f)] private float baseBonus = 0.05f;
    [SerializeField, Min(0f)] private float bonusPerLevel = 0.02f;
    [SerializeField, Tooltip("Required for machine specialties; ignored for global buffs.")]
    private CMData targetMachine;

    public EmployeeBuffType BuffType => buffType;
    public CMData TargetMachine => targetMachine;
    public bool IsMachineSpecialty => buffType >= EmployeeBuffType.HouseBlendExpert;
    public bool AppliesTo(CMData machine) => !IsMachineSpecialty || targetMachine != null && targetMachine == machine;

    public float GetBuffBonus(int level)
    {
        if (level <= 0) return 0f;
        bool gentle = buffType == EmployeeBuffType.BeanCostReduction || buffType == EmployeeBuffType.TicketChance ||
                      buffType == EmployeeBuffType.TipChance || buffType == EmployeeBuffType.ArcaneEfficiency;
        float rarityMultiplier;
        switch (tier)
        {
            case 2: rarityMultiplier = gentle ? 1.10f : 1.15f; break;
            case 3: rarityMultiplier = gentle ? 1.20f : 1.30f; break;
            case 4: rarityMultiplier = gentle ? 1.30f : 1.50f; break;
            case 5: rarityMultiplier = gentle ? 1.40f : 1.75f; break;
            default: rarityMultiplier = 1f; break;
        }
        return (baseBonus + (level - 1) * bonusPerLevel) * rarityMultiplier;
    }

    public string BuffName => buffType switch
    {
        EmployeeBuffType.BeanGeneration => "Green Thumb",
        EmployeeBuffType.BrewingSpeed => "Quick Brew",
        EmployeeBuffType.SalePrice => "Latte Art",
        EmployeeBuffType.FameXP => "Word of Mouth",
        EmployeeBuffType.BeanStorage => "Bottomless Pantry",
        EmployeeBuffType.BeanCostReduction => "Perfect Portions",
        EmployeeBuffType.TicketChance => "Lucky Service",
        EmployeeBuffType.TipChance => "Generous Tips",
        EmployeeBuffType.HouseBlendExpert => "House Blend Expert",
        EmployeeBuffType.EspressoVirtuoso => "Espresso Virtuoso",
        EmployeeBuffType.ManaResonance => "Mana Resonance",
        EmployeeBuffType.ArcaneEfficiency => "Arcane Efficiency",
        EmployeeBuffType.SignatureRoast => "Signature Roast",
        _ => "Employee Buff"
    };

    public string GetEffectDescription(int level)
    {
        float percent = GetBuffBonus(level) * 100f;
        string effect = buffType switch
        {
            EmployeeBuffType.BeanGeneration => $"+{percent:0.#}% bean rate",
            EmployeeBuffType.BrewingSpeed or EmployeeBuffType.EspressoVirtuoso => $"+{percent:0.#}% brew speed",
            EmployeeBuffType.SalePrice or EmployeeBuffType.HouseBlendExpert or EmployeeBuffType.ManaResonance => $"+{percent:0.#}% sale value",
            EmployeeBuffType.FameXP or EmployeeBuffType.SignatureRoast => $"+{percent:0.#}% fame XP",
            EmployeeBuffType.BeanStorage => $"+{percent:0.#}% storage",
            EmployeeBuffType.BeanCostReduction or EmployeeBuffType.ArcaneEfficiency => $"-{percent:0.#}% beans",
            EmployeeBuffType.TicketChance => $"+{percent:0.#} pp ticket chance",
            EmployeeBuffType.TipChance => $"{percent:0.#}% chance: +50% sale gold",
            _ => "No effect"
        };
        return IsMachineSpecialty ? effect + "\n" + (targetMachine != null ? targetMachine.coffeeMakerName : "No machine assigned") : effect;
    }
}

public enum EmployeeBuffType
{
    BeanGeneration = 0,
    BrewingSpeed = 1,
    SalePrice = 2,
    FameXP = 3,
    BeanStorage = 4,
    BeanCostReduction = 5,
    TicketChance = 6,
    TipChance = 7,
    HouseBlendExpert = 8,
    EspressoVirtuoso = 9,
    ManaResonance = 10,
    ArcaneEfficiency = 11,
    SignatureRoast = 12
}
