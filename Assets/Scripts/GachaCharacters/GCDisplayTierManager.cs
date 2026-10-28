using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GCDisplayTierManager : MonoBehaviour
{
    [SerializeField] private GCDatabase database;
    [SerializeField] private GameObject gachaCellPrefab;
    [SerializeField] private Transform unitDisplayGrid;
    private GridLayoutGroup gridLayout;
    private RectTransform gridRect;
    [SerializeField] private Color commonUnit = new Color(0.86f, 0.86f, 0.86f);
    [SerializeField] private Color rareUnit =  new Color(0.9f, 0.75f, 0.2f);
    [SerializeField] private Color epicUnit = new Color(0.718f, 0.431f, 0.475f);
    [SerializeField] private Color legendaryUnit = new Color(1f, 0.843f, 0f);
    [SerializeField] private Color mythicalUnit = new Color(0.33f, 0f, 0.11f);

    [SerializeField] private Color errorColor = new Color(0f, 0f, 0f);
    [SerializeField] private int numUnitsPerTier = 4;
    [SerializeField] private GCRosterManager roster;

    [SerializeField] private int regularColumns = 4;
    [SerializeField] private int regularRows = 2;
    [SerializeField] private int mythicalColumns = 2;
    [SerializeField] private int mythicalRows = 2;
    [SerializeField] private float cellHeightToWidthRatio = 2.6f;

    [Header("Summon Result")]
    [SerializeField] private Transform summonResultDisplay;
    private GridLayoutGroup summonResultGridLayout;
    private RectTransform summonResultGridRect;

    

    void Awake()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (unitDisplayGrid != null)
        {
            if (gridLayout == null) gridLayout = unitDisplayGrid.GetComponent<GridLayoutGroup>();
            if (gridRect == null) gridRect = unitDisplayGrid as RectTransform;
        }
        if (summonResultDisplay != null)
        {
            if (summonResultGridLayout == null) summonResultGridLayout = summonResultDisplay.GetComponent<GridLayoutGroup>();
            if (summonResultGridRect == null) summonResultGridRect = summonResultDisplay as RectTransform;
        }
    }

    public void PopulateTier(int row1Tier, int row2Tier)
    {
        EnsureReferences();
        PopulateGrid(regularColumns, regularRows, row1Tier, row2Tier);
    }

    public void PopulateMythicalTier(int mythicalTier = 5)
    {
        EnsureReferences();
        PopulateGrid(mythicalColumns, mythicalRows, mythicalTier);
    }

    public void PopulateSummonResult(int count, GCData[] results)
    {
        EnsureReferences();
        PopulateSummonResultGrid(count, results);
    }

    private void PopulateGrid(int columns, int rows, params int[] tierIds)
    {
        if (database == null || unitDisplayGrid == null || gachaCellPrefab == null || gridLayout == null || gridRect == null)
        {
            return;
        }

        DestroyChildren();
        Vector2 cellSize = CalculateCellSize(columns, rows);
        ApplyGridLayout(columns, cellSize);

        foreach (int tierId in tierIds)
        {
            var characters = database.gachaCharacters
                .Where(c => c != null && c.tier == tierId)
                .Take(numUnitsPerTier);

            Color tierColor = GetTierColor(tierId);
            foreach (GCData data in characters)
            {
                SpawnCell(data, tierColor, cellSize);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
    }

    private void PopulateSummonResultGrid(int count, GCData[] results)
    {
        if (unitDisplayGrid == null || gridLayout == null || gridRect == null ||
            summonResultDisplay == null || gachaCellPrefab == null || results == null)
        {
            return;
        }

        
        Transform previousGrid = unitDisplayGrid;
        GridLayoutGroup previousLayout = gridLayout;
        RectTransform previousRect = gridRect;

        try
        {
            unitDisplayGrid = summonResultDisplay;
            gridLayout = summonResultDisplay.GetComponent<GridLayoutGroup>();
            gridRect = summonResultDisplay as RectTransform;
            summonResultGridLayout = gridLayout;
            summonResultGridRect = gridRect;
            Vector2 cellSize;
            
            if (count > 1)
            {
                cellSize = CalculateCellSize(count / 2, 2);
            }
            else
            {
                cellSize = CalculateCellSize(3, 2);
            }

            if (gridLayout == null || gridRect == null)
            {
                return;
            }

            DestroyChildren();
            int columns = Mathf.Min(Mathf.Max(count, 1), Mathf.Max(5, 1));
            ApplyGridLayout(columns, cellSize);

            foreach (GCData data in results)
            {
                if (data != null)
                {
                    SpawnCell(data, GetTierColor(data.tier), cellSize);
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
        }
        finally
        {
            unitDisplayGrid = previousGrid;
            gridLayout = previousLayout;
            gridRect = previousRect;
            summonResultGridLayout = null;
            summonResultGridRect = null;
        }
    }

    private Vector2 CalculateCellSize(int columns, int rows)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

        float innerWidth = GetInnerWidth();
        float innerHeight = Mathf.Max(gridRect.rect.height - gridLayout.padding.vertical, 0f);

        columns = Mathf.Max(columns, 1);
        rows = Mathf.Max(rows, 1);

        float cellWidth = (innerWidth - gridLayout.spacing.x * (columns - 1)) / columns;
        float cellHeight = cellWidth * cellHeightToWidthRatio;

        if (innerHeight > 0f)
        {
            float maxHeight = (innerHeight - gridLayout.spacing.y * (rows - 1)) / rows;
            cellHeight = Mathf.Min(cellHeight, maxHeight);
        }

        return new Vector2(Mathf.Max(cellWidth, 1f), Mathf.Max(cellHeight, 1f));
    }

    private float GetInnerWidth()
    {
        float width = gridRect.rect.width;
        if (width > 1f)
        {
            return width - gridLayout.padding.horizontal;
        }

        RectTransform current = gridRect.parent as RectTransform;
        while (current != null)
        {
            if (current.rect.width > 1f)
            {
                return current.rect.width - gridLayout.padding.horizontal;
            }

            current = current.parent as RectTransform;
        }

        return 1200f - gridLayout.padding.horizontal;
    }

    private void ApplyGridLayout(int columns, Vector2 cellSize)
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = Mathf.Max(columns, 1);
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.cellSize = cellSize;
    }

    private void DestroyChildren()
    {
        foreach (Transform child in unitDisplayGrid)
        {
            Destroy(child.gameObject);
        }
    }

    private void SpawnCell(GCData data, Color bgColor, Vector2 cellSize)
    {
        int level = roster != null ? roster.GetLevel(data) : 0;
        GameObject cellObj = Instantiate(gachaCellPrefab, unitDisplayGrid);

        var rt = (RectTransform)cellObj.transform;
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cellSize;

        var layoutElement = cellObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = cellObj.AddComponent<LayoutElement>();
        }

        layoutElement.minWidth = cellSize.x;
        layoutElement.minHeight = cellSize.y;
        layoutElement.preferredWidth = cellSize.x;
        layoutElement.preferredHeight = cellSize.y;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        GCCell cell = cellObj.GetComponent<GCCell>();
        cell.Setup(data, bgColor, level);
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
            case 5:
                return mythicalUnit;
            default:
                print("Unknown Tier...");
                return errorColor;
        }
    }
}
