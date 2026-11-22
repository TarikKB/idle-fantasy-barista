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
    // Distinct cool/warm hues without relying on red-green differences.
    [SerializeField] private Color commonUnit = new Color32(217, 221, 227, 255); // Silver
    [SerializeField] private Color rareUnit = new Color32(86, 180, 233, 255); // Sky blue
    [SerializeField] private Color epicUnit = new Color32(204, 121, 167, 255); // Violet
    [SerializeField] private Color legendaryUnit = new Color32(230, 159, 0, 255); // Amber
    [SerializeField] private Color mythicalUnit = new Color32(36, 59, 107, 255); // Deep blue

    [SerializeField] private Color errorColor = new Color(0f, 0f, 0f);
    [SerializeField] private int numUnitsPerTier = 4;
    [SerializeField] private GCRosterManager roster;
    [SerializeField] private FranchiseManager franchise;
    private string assignmentLocationId;
    public void SetAssignmentLocation(string locationId)
    {
        assignmentLocationId = locationId;
        needsRefresh = true;
        if (isActiveAndEnabled) { needsRefresh = false; PopulateTier(1, 2); }
    }
    [SerializeField] private TMPro.TextMeshProUGUI employeeTeamText;
    [Header("Collection Paging")]
    [SerializeField] private UnityEngine.UI.Button previousPageButton;
    [SerializeField] private UnityEngine.UI.Button nextPageButton;
    [SerializeField] private TMPro.TextMeshProUGUI pageText;
    private int currentPage;
    private int[] selectedTiers = { 1, 2 };
    private bool mythicalPage;
    public int CurrentPage => currentPage;
    public int PageCount => database == null ? 1 : Mathf.Max(1, selectedTiers.Max(tier =>
        Mathf.CeilToInt(database.gachaCharacters.Count(data => data != null && data.tier == tier) / (float)Mathf.Max(1, numUnitsPerTier))));

    public void PreviousPage()
    {
        currentPage = Mathf.Max(0, currentPage - 1);
        RedrawCurrentPage();
    }

    public void NextPage()
    {
        currentPage = Mathf.Min(PageCount - 1, currentPage + 1);
        RedrawCurrentPage();
    }

    private void RedrawCurrentPage()
    {
        currentPage = Mathf.Clamp(currentPage, 0, PageCount - 1);
        PopulateGrid(mythicalPage ? mythicalColumns : regularColumns, mythicalPage ? mythicalRows : regularRows, selectedTiers);
        if (pageText != null) pageText.text = $"Page {currentPage + 1} / {PageCount}";
        if (previousPageButton != null) previousPageButton.interactable = currentPage > 0;
        if (nextPageButton != null) nextPageButton.interactable = currentPage + 1 < PageCount;
    }

    private void RefreshTeamSummary()
    {
        if (employeeTeamText != null && roster != null)
            employeeTeamText.text = roster.GetTeamSummary() + "\nTap Assign on an owned employee. Remove one to change a full team.";
    }

    [SerializeField] private int regularColumns= 4;
    [SerializeField] private int regularRows = 2;
    [SerializeField] private int mythicalColumns = 2;
    [SerializeField] private int mythicalRows = 2;
    [SerializeField] private float cellHeightToWidthRatio = 2.6f;

    [Header("Summon Result")]
    [SerializeField] private Transform summonResultDisplay;
    private GridLayoutGroup summonResultGridLayout;
    private RectTransform summonResultGridRect;

    // Last page shown, so it can be redrawn when the roster changes
    private System.Action refreshCurrentPage;
    private bool needsRefresh;

    void Awake()
    {
        EnsureReferences();
        if (roster != null) roster.OnRosterChanged += HandleRosterChanged;
        if (previousPageButton != null) previousPageButton.onClick.AddListener(PreviousPage);
        if (nextPageButton != null) nextPageButton.onClick.AddListener(NextPage);
    }

    void OnDestroy()
    {
        if (roster != null) roster.OnRosterChanged -= HandleRosterChanged;
        if (previousPageButton != null) previousPageButton.onClick.RemoveListener(PreviousPage);
        if (nextPageButton != null) nextPageButton.onClick.RemoveListener(NextPage);
    }

    void OnEnable()
    {
        RefreshTeamSummary();
        if (needsRefresh)
        {
            needsRefresh = false;
            refreshCurrentPage?.Invoke();
        }
    }

    private void HandleRosterChanged()
    {
        RefreshTeamSummary();
        if (isActiveAndEnabled)
            refreshCurrentPage?.Invoke();
        else
            needsRefresh = true;
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
        if (mythicalPage || selectedTiers.Length != 2 || selectedTiers[0] != row1Tier || selectedTiers[1] != row2Tier) currentPage = 0;
        mythicalPage = false;
        selectedTiers = new[] { row1Tier, row2Tier };
        refreshCurrentPage = RedrawCurrentPage;
        RedrawCurrentPage();
    }

    public void PopulateMythicalTier(int mythicalTier = 5)
    {
        EnsureReferences();
        if (!mythicalPage || selectedTiers[0] != mythicalTier) currentPage = 0;
        mythicalPage = true;
        selectedTiers = new[] { mythicalTier };
        refreshCurrentPage = RedrawCurrentPage;
        RedrawCurrentPage();
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
        // Keep staff frame dimensions consistent, even on the two-column Mythical page.
        Vector2 cellSize = CalculateCellSize(regularColumns, regularRows);
        ApplyGridLayout(columns, cellSize);

        foreach (int tierId in tierIds)
        {
            var characters = database.gachaCharacters
                .Where(c => c != null && c.tier == tierId)
                .Skip(currentPage * Mathf.Max(1, numUnitsPerTier))
                .Take(Mathf.Max(1, numUnitsPerTier));

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
        int copies = roster != null ? roster.GetCopies(data) : 0;
        int copiesNeeded = roster != null ? roster.GetCopiesForNextLevel(data) : -1;
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
        cell.Setup(data, bgColor, level, copies, copiesNeeded);
        cell.SetupAssignment(data, roster, unitDisplayGrid != summonResultDisplay);
        if (franchise != null && franchise.Ready && unitDisplayGrid != summonResultDisplay)
            cell.SetupLocationAssignment(franchise, string.IsNullOrEmpty(assignmentLocationId) ? franchise.Active.id : assignmentLocationId);
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
