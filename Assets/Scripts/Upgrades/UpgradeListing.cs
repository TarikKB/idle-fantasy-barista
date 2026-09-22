using UnityEngine;

public class UpgradeListing : MonoBehaviour
{

    public UpgradeLevelData upgradeLevelData;
    public int currentLevel = 0;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private TMPro.TextMeshProUGUI costText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateText();
    }

    // Update is called once per frame
    public void UpdateText()
    {
        levelText.text = $"Level: {currentLevel + 1}";
        float nextCost = upgradeLevelData.baseCost * Mathf.Pow(upgradeLevelData.costMultiplier, currentLevel);
        costText.text = $"Cost: ${nextCost}";
    }
}
