using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Leveling Config", menuName = "Gacha/Leveling Config")]
public class GCLevelingConfig : ScriptableObject
{
    // Each entry is the copies needed to go from level (index + 1) to level (index + 2)
    // Max level for a tier = array length + 1
    [SerializeField] private int[] tier1Costs = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    [SerializeField] private int[] tier2Costs = { 1, 1, 2, 2, 3, 3, 4, 4, 5 };
    [SerializeField] private int[] tier3Costs = { 1, 1, 1, 2, 2, 2, 3, 3, 3 };
    [SerializeField] private int[] tier4Costs = { 1, 1, 1, 1, 2, 2, 2, 2, 2 };
    [SerializeField] private int[] tier5Costs = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };

    public int GetMaxLevel(int tier)
    {
        int[] costs = GetCosts(tier);
        return costs != null ? costs.Length + 1 : 1;
    }

    // Returns -1 when the character is already at max level
    public int GetCopiesForNextLevel(int tier, int currentLevel)
    {
        int[] costs = GetCosts(tier);
        int index = currentLevel - 1;
        if (costs == null || index < 0 || index >= costs.Length)
            return -1;

        return Mathf.Max(costs[index], 1);
    }

    private int[] GetCosts(int tier)
    {
        switch (tier)
        {
            case 1:
                return tier1Costs;
            case 2:
                return tier2Costs;
            case 3:
                return tier3Costs;
            case 4:
                return tier4Costs;
            case 5:
                return tier5Costs;
            default:
                Debug.LogWarning($"No leveling costs for tier {tier}");
                return null;
        }
    }
}
