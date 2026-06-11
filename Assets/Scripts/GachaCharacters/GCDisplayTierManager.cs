using UnityEngine;
using System.Linq;

public class GCDisplayTierManager : MonoBehaviour
{
    [SerializeField] private GCDatabase database;
    [SerializeField] private GameObject gachaCellPrefab;
    [SerializeField] private Transform unitDisplayGrid;
    [SerializeField] private Color commonUnit = new Color(0.86f, 0.86f, 0.86f);
    [SerializeField] private Color rareUnit =  new Color(0.9f, 0.75f, 0.2f);
    [SerializeField] private Color epicUnit = new Color(0.718f, 0.431f, 0.475f);
    [SerializeField] private Color legendaryUnit = new Color(1f, 0.843f, 0f);
    [SerializeField] private Color errorColor = new Color(0f, 0f, 0f);
    [SerializeField] private int numUnitsPerTier = 1;
    [SerializeField] private GCRosterManager roster;

    
    public void PopulateTier(int row1Tier, int row2Tier)
    {
        if (database == null || unitDisplayGrid == null || gachaCellPrefab == null) return;

        foreach (Transform child in unitDisplayGrid)
        {
            Destroy(child.gameObject);
        }

        var row1 = database.gachaCharacters.Where(c => c.tier == row1Tier).Take(numUnitsPerTier);
        var row2 = database.gachaCharacters.Where(c => c.tier == row2Tier).Take(numUnitsPerTier);
        foreach (GCData data in row1)
        {
            Color bgColor =  GetTierColor(row1Tier) ;
            int level = roster != null ? roster.GetLevel(data) : 0;
            GameObject cellObj = Instantiate(gachaCellPrefab, unitDisplayGrid);
            GCCell cell = cellObj.GetComponent<GCCell>();
            cell.Setup(data, bgColor, level); 
        }
        foreach (GCData data in row2)
        {
            Color bgColor =  GetTierColor(row2Tier) ;
            int level = roster != null ? roster.GetLevel(data) : 0;
            GameObject cellObj = Instantiate(gachaCellPrefab, unitDisplayGrid);
            GCCell cell = cellObj.GetComponent<GCCell>();
            cell.Setup(data, bgColor, level); 
        }
    }

    private Color GetTierColor(int rowTier)
    {
        switch (rowTier)
        {
            case 1:
                return commonUnit;
            case 2:
                return rareUnit;
            case 3: 
                return epicUnit;
            case 4:
                return legendaryUnit;
            default:
                print("Unknown Tier...");
                return errorColor;
        }
        
    }
}
