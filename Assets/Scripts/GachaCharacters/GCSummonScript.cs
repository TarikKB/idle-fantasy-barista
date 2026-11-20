using System.Collections.Generic;
using UnityEngine;

public class GCSummonScript : MonoBehaviour
{

    [SerializeField] private ResourceManagerScript resourceManager;
    [SerializeField] private GameObject summonResultPanel;
    [SerializeField] private GCRosterManager roster;

    [Header("Tier Catalogues")]
    [SerializeField] private GCDatabase characterDatabase;
    [SerializeField] private List<GCData> tier1Catalogue;
    [SerializeField] private List<GCData> tier2Catalogue;
    [SerializeField] private List<GCData> tier3Catalogue;
    [SerializeField] private List<GCData> tier4Catalogue;
    [SerializeField] private List<GCData> tier5Catalogue;

    private GCDisplayTierManager displayTierManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        displayTierManager = GetComponent<GCDisplayTierManager>();
        tier1Catalogue = new List<GCData>();
        tier2Catalogue = new List<GCData>();
        tier3Catalogue = new List<GCData>();
        tier4Catalogue = new List<GCData>();
        tier5Catalogue = new List<GCData>();
        if (characterDatabase == null) return;
        var seen = new HashSet<string>();
        foreach (GCData charData in characterDatabase.gachaCharacters)
        {
            if (charData == null || string.IsNullOrEmpty(charData.id) || !seen.Add(charData.id)) continue;
            switch (charData.tier)
            {
                case 1:
                    tier1Catalogue.Add(charData);
                    break;
                case 2:
                    tier2Catalogue.Add(charData);
                    break;
                case 3:
                    tier3Catalogue.Add(charData);
                    break;
                case 4:
                    tier4Catalogue.Add(charData);
                    break;
                case 5:
                    tier5Catalogue.Add(charData);
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TrySummon(int count)
    {
        if (!resourceManager) { return; }
        if (resourceManager.tickets >= count)
        {
            resourceManager.AddTickets(-count);
            PerformSummon(count);
            return;
        }
        return;
    }

    private void PerformSummon(int count)
    {
        int resultTier = 0;
        GCData[] results = new GCData[count];
        for (int i = 0; i < count; i++)
        {
            
            switch (Random.Range(0f, 100f))
            {
                case < 60f:
                    resultTier = 1;
                    break;
                case < 88f:
                    resultTier = 2;
                    break;
                case < 97f:
                    resultTier = 3;
                    break;
                case < 99.5f:
                    resultTier = 4;
                    break;
                case < 100f:
                    resultTier = 5;
                    break;
            }
            switch (resultTier)
            {
                case 1:
                    results[i] = tier1Catalogue[Random.Range(0, tier1Catalogue.Count)];
                    break;
                case 2:
                    results[i] = tier2Catalogue[Random.Range(0, tier2Catalogue.Count)];
                    break;
                case 3:
                    results[i] = tier3Catalogue[Random.Range(0, tier3Catalogue.Count)];
                    break;
                case 4:
                    results[i] = tier4Catalogue[Random.Range(0, tier4Catalogue.Count)];
                    break;
                case 5:
                    results[i] = tier5Catalogue[Random.Range(0, tier5Catalogue.Count)];
                    break;
            }
        }

        // Add to the roster before displaying so result cells show the updated levels
        if (roster != null)
        {
            roster.AddCharacters(results);
        }

        // Save immediately so a crash can't lose spent tickets and their pulls
        if (MobileSaveManager.Instance != null)
        {
            MobileSaveManager.Instance.SaveGame();
        }

        if (summonResultPanel != null)
        {
            summonResultPanel.SetActive(true);
            if (displayTierManager != null)
            {
                displayTierManager.PopulateSummonResult(count, results);
            }
        }
    }
}
