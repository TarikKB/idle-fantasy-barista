using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LocationsPanel : MonoBehaviour
{
    [SerializeField] private FranchiseManager _franchise;
    [SerializeField] private MenuManagerScript _menu;
    [SerializeField] private GameObject _root;
    [SerializeField] private RectTransform _safeArea;
    [SerializeField] private TMPro.TextMeshProUGUI _summary;
    [SerializeField] private TMPro.TextMeshProUGUI _detail;
    [SerializeField] private TMPro.TextMeshProUGUI _requirements;
    [SerializeField] private Transform _locationContent;
    [SerializeField] private UnityEngine.UI.Button _locationTemplate;
    [SerializeField] private Transform _machineContent;
    [SerializeField] private GameObject _machineTemplate;
    [SerializeField] private GameObject _machineView;
    [SerializeField] private GameObject _staffView;
    [SerializeField] private GCDisplayTierManager _staffPicker;
    [SerializeField] private UnityEngine.UI.Button _expandButton;
    [SerializeField] private GameObject _modal;
    [SerializeField] private TMPro.TextMeshProUGUI _modalText;
    [SerializeField] private UnityEngine.UI.Button _confirmButton;
    [SerializeField] private GameObject _offlineModal;
    [SerializeField] private TMPro.TextMeshProUGUI _offlineText;
    private readonly List<UnityEngine.UI.Button> _locationRows = new List<UnityEngine.UI.Button>();
    private readonly List<GameObject> _machineRows = new List<GameObject>();
    private string _selectedId;
    private string _pendingExpansion;
    private GCData _pendingStaff;
    private string _pendingDestination;
    private double _lastRefresh;
    private Rect _lastSafeArea;

    private void Awake()
    {
        _franchise.Changed += Refresh;
        _franchise.TransferRequested += ShowTransfer;
        _franchise.OfflineIncomeCredited += ShowOffline;
        _confirmButton.onClick.AddListener(Confirm);
    }
    private void OnDestroy()
    {
        if (_franchise != null) { _franchise.Changed -= Refresh; _franchise.TransferRequested -= ShowTransfer; _franchise.OfflineIncomeCredited -= ShowOffline; }
        if (_confirmButton != null) _confirmButton.onClick.RemoveListener(Confirm);
    }
    private void Update()
    {
        if (_safeArea != null && (Screen.safeArea != _lastSafeArea) && Screen.width > 0 && Screen.height > 0)
        {
            _lastSafeArea = Screen.safeArea;
            _safeArea.anchorMin = new Vector2(_lastSafeArea.xMin / Screen.width, _lastSafeArea.yMin / Screen.height);
            _safeArea.anchorMax = new Vector2(_lastSafeArea.xMax / Screen.width, _lastSafeArea.yMax / Screen.height);
        }
        if (_root.activeInHierarchy && Time.realtimeSinceStartupAsDouble - _lastRefresh >= 1)
        { _lastRefresh = Time.realtimeSinceStartupAsDouble; Refresh(); }
    }
    public void Toggle()
    {
        if (_root.activeSelf) { Close(); return; }
        _menu.CloseActivePanel(); _root.SetActive(true);
        _selectedId = _franchise.Ready ? _franchise.Active.id : null;
        ShowMachines(); Refresh();
    }
    public void Close() { _root.SetActive(false); _modal.SetActive(false); }
    public void Cancel() { _modal.SetActive(false); _pendingStaff = null; _pendingExpansion = null; }
    public void DismissOffline() { _offlineModal.SetActive(false); }
    public void ShowMachines() { _machineView.SetActive(true); _staffView.SetActive(false); Refresh(); }
    public void ShowStaff()
    {
        if (!_franchise.Ready) return;
        _machineView.SetActive(false); _staffPicker.SetAssignmentLocation(_selectedId);
        _staffView.SetActive(true); _staffPicker.PopulateTier(1, 2); Refresh();
    }
    public void CommonRare() { _staffPicker.PopulateTier(1, 2); }
    public void EpicLegendary() { _staffPicker.PopulateTier(3, 4); }
    public void Mythical() { _staffPicker.PopulateMythicalTier(); }

    public void Refresh()
    {
        if (_root == null || !_root.activeInHierarchy) return;
        if (!_franchise.Ready)
        {
            _summary.text = _franchise.Save.LastError ?? _franchise.LastError ?? "Loading franchise...";
            _expandButton.interactable = false; return;
        }
        if (_franchise.GetLocation(_selectedId) == null) _selectedId = _franchise.Active.id;
        _franchise.Save.CaptureActive(_franchise.Active);
        _summary.text = $"Shared gold: {_franchise.Save.Resources.gold:N0}\nRetired income: {_franchise.TotalRetiredRate * 60:N1} gold/min  |  Highest Fame: {_franchise.State.highestFame}";
        int n = 0;
        for (int index = _franchise.State.locations.Count - 1; index >= 0; index--)
        {
            var cafe = _franchise.State.locations[index];
            if (n >= _locationRows.Count) _locationRows.Add(Instantiate(_locationTemplate, _locationContent));
            var row = _locationRows[n++]; row.gameObject.SetActive(true);
            row.GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text = $"{cafe.name}  |  {(cafe.retired ? "Retired" : "Current")}  |  Fame {cafe.fameLevel}" +
                (cafe.retired ? $"  |  {cafe.goldPerSecond * 60:N1}/min" : "") + (_selectedId == cafe.id ? "  • Viewing" : "  • View");
            string id = cafe.id; row.onClick.RemoveAllListeners(); row.onClick.AddListener(() => Select(id));
        }
        for (; n < _locationRows.Count; n++) _locationRows[n].gameObject.SetActive(false);
        var selected = _franchise.GetLocation(_selectedId);
        var text = new StringBuilder();
        text.Append(selected.name).Append(selected.retired ? " — Retired" : " — Active").Append("\n");
        text.Append("Fame ").Append(selected.fameLevel).Append("  |  Café revenue: ").Append(selected.lifetimeRevenue.ToString("N0")).Append("\n");
        var u = selected.upgrades;
        text.Append($"Local upgrades: Beans {u.beanRateLevel} • Storage {u.beanLimitLevel} • Sales {u.sellPriceLevel}\nTickets {u.ticketRateLevel} • Fame XP {u.fameBonusLevel}\nTeam: ");
        for (int i = 0; i < 3; i++) { if (i > 0) text.Append(" | "); var staff = _franchise.GetAssignedEmployee(selected, i); text.Append(staff == null ? "Empty" : staff.characterName); }
        text.Append(selected.retired ? "\nGold only. Fame, tickets and storage do not boost steady passive income." : "\nViewing other cafés does not change your active brewing location.");
        _detail.text = text.ToString();
        _requirements.text = $"Current café Fame {_franchise.Save.Resources.fameLevel}/{_franchise.Config.RequiredFame} • Opening: {_franchise.Config.OpeningCost:N0} gold";
        _expandButton.interactable = _franchise.CanExpand;
        if (_machineView.activeSelf) RefreshMachines(selected);
    }
    private void Select(string id)
    {
        _selectedId = id;
        if (_staffView.activeSelf) _staffPicker.SetAssignmentLocation(id);
        Refresh();
    }
    private void RefreshMachines(LocationState cafe)
    {
        int index = 0;
        foreach (var slot in cafe.machines)
        {
            if (index >= _machineRows.Count) _machineRows.Add(Instantiate(_machineTemplate, _machineContent));
            var row = _machineRows[index++]; row.SetActive(true);
            var machine = slot == null ? null : _franchise.Save.GetCoffeeMakerById(slot.coffeeMakerId);
            var icon = row.GetComponentInChildren<UnityEngine.UI.Image>(true);
            icon.sprite = machine != null && machine.icons != null && machine.icons.Length > 0 ? machine.icons[0] : null;
            icon.enabled = icon.sprite != null;
            row.GetComponentInChildren<TMPro.TextMeshProUGUI>(true).text = machine == null ? $"Slot {index}: Empty" : machine.coffeeMakerName;
        }
        for (; index < _machineRows.Count; index++) _machineRows[index].SetActive(false);
    }
    public void ShowExpansion()
    {
        if (!_franchise.CanExpand) { Refresh(); return; }
        _franchise.Save.CaptureActive(_franchise.Active);
        _pendingExpansion = _franchise.Active.id; _pendingStaff = null;
        _modalText.text = $"Open Café {_franchise.State.locations.Count + 1}?\n\nCost: {_franchise.Config.OpeningCost:N0} gold\nRemaining gold: {_franchise.Save.Resources.gold - _franchise.Config.OpeningCost:N0}\n\n{_franchise.Active.name} keeps its machines, upgrades and staff.\nProjected income: {_franchise.CalculateRate(_franchise.Active) * 60:N1} gold/min\n\nNew café: Fame 1, upgrade levels 0, zero beans, empty team and one basic starter machine.\n\nKeep gold remainder, tickets, staff collection and permanent unlocks.";
        _confirmButton.interactable = true; _modal.SetActive(true);
    }
    private void ShowTransfer(GCData staff, string destination)
    {
        _pendingStaff = staff; _pendingDestination = destination; _pendingExpansion = null;
        var source = _franchise.Employer(staff); var target = _franchise.GetLocation(destination);
        if (source == null || target == null) return;
        if (source == _franchise.Active || target == _franchise.Active) _franchise.Save.CaptureActive(_franchise.Active);
        _modalText.text = $"Transfer {staff.characterName}?\n\nFrom {source.name} to {target.name}.\nThis employee can only work at one café.\n\n" +
            (source.retired ? $"{source.name}: {source.goldPerSecond * 60:N1} → {_franchise.PreviewRate(source, staff, false) * 60:N1} gold/min\n" : $"{source.name} loses their active buff.\n") +
            (target.retired ? $"{target.name}: {target.goldPerSecond * 60:N1} → {_franchise.PreviewRate(target, staff, true) * 60:N1} gold/min" : $"{target.name} gains their active buff.");
        _confirmButton.interactable = true; _modal.SetActive(true);
    }
    private void Confirm()
    {
        _confirmButton.interactable = false;
        bool success = _pendingStaff != null ? _franchise.TryAssign(_pendingStaff, _pendingDestination) :
            _pendingExpansion != null && _franchise.TryOpenLocation(_pendingExpansion);
        if (success) { Cancel(); if (_root.activeSelf) Refresh(); }
        else { _modalText.text = _franchise.LastError ?? "The operation could not be completed. Please cancel and retry."; }
    }
    private void ShowOffline()
    {
        _offlineText.text = $"Welcome back!\n\nYour retired cafés earned {_franchise.LastOfflineGold:N1} gold.\nAlready added to your shared wallet.\n\nOffline production is capped at {_franchise.Config.OfflineCapSeconds / 3600f:0.#} hours.";
        _offlineModal.SetActive(true);
    }
}
