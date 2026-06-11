using UnityEngine;

public class GCTierScript : MonoBehaviour
{
    private enum TierGroup { CommonRare, EpicLegendary }

    private TierGroup currentTier = TierGroup.CommonRare;

    [SerializeField] private GCDisplayTierManager populateBoxes;

    void Start() => populateBoxes.PopulateTier(1, 2);
    
    public void ShowCommonRare()
    {
        if (currentTier == TierGroup.CommonRare)
        {
            return;
        }

        currentTier = TierGroup.CommonRare;
        populateBoxes.PopulateTier(1,2);
    }
    public void ShowEpicLegendary()
    {   
        if (currentTier == TierGroup.EpicLegendary)
        {
            return;
        }
        currentTier = TierGroup.EpicLegendary;
        populateBoxes.PopulateTier(3,4);
    }
};
    
