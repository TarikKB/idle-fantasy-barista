using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(90)]
public class FranchiseManager : MonoBehaviour
{
    [SerializeField] private MobileSaveManager _save;
    [SerializeField] private FranchiseConfig _config;
    [SerializeField] private MenuManagerScript _menu;
    [SerializeField, Min(1)] private float _autosaveSeconds = 30;
    private double _lastRealtime;
    private double _lastAutosave;
    private bool _suspended = true;
    private bool _restoring;
    public bool IsTransitioning { get; private set; }
    public FranchiseState State => _save == null ? null : _save.State;
    public FranchiseConfig Config => _config;
    public MobileSaveManager Save => _save;
    public LocationState Active => State == null ? null : GetLocation(State.activeLocationId);
    public bool Ready => _save != null && _save.IsInitialized && State != null && _config != null && _config.IsValid;
    public double LastOfflineGold { get; private set; }
    public event Action Changed;
    public event Action OfflineIncomeCredited;
    public event Action<GCData, string> TransferRequested;
    public event Action TransitionStarted;
    public string LastError { get; private set; }

    private void Awake()
    {
        if (_save == null) return;
        _save.Loaded += HandleLoaded;
        _save.BeforeSave += SettleOnline;
        _save.Pausing += Pause;
        _save.Resuming += Resume;
        _save.Roster.BeforeRosterChanged += SettleOnline;
        _save.Roster.OnRosterChanged += RosterChanged;
        _save.Resources.FameChanged += UpdateUnlocks;
        _save.Resources.CoffeeRevenueEarned += RecordSale;
        _save.Resources.MachineUnlockQuery = IsMachineUnlocked;
    }
    private void OnDestroy()
    {
        if (_save == null) return;
        _save.Loaded -= HandleLoaded; _save.BeforeSave -= SettleOnline;
        _save.Pausing -= Pause; _save.Resuming -= Resume;
        if (_save.Roster != null)
        {
            _save.Roster.BeforeRosterChanged -= SettleOnline; _save.Roster.OnRosterChanged -= RosterChanged;
            _save.Roster.AssignedEmployeeQuery = null; _save.Roster.ToggleAssignmentOverride = null;
        }
        if (_save.Resources != null)
        {
            _save.Resources.FameChanged -= UpdateUnlocks; _save.Resources.CoffeeRevenueEarned -= RecordSale;
            _save.Resources.MachineUnlockQuery = null;
        }
    }

    private void HandleLoaded()
    {
        if (_config == null || !_config.IsValid) { LastError = "Franchise settings or starter machine are missing."; Debug.LogError(LastError); return; }
        _restoring = true;
        // Pay stored rates first; ownership restoration may have advanced levels after a rebalance.
        CreditOffline(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        _save.Roster.AssignedEmployeeQuery = slot => GetAssignedEmployee(Active, slot);
        _save.Roster.ToggleAssignmentOverride = ToggleActiveAssignment;
        NormalizeAssignments();
        _save.Roster.NotifyAssignmentsChanged();
        UpdateUnlocks(_save.Resources.fameLevel);
        RecalculateRates();
        _restoring = false; _suspended = false;
        _lastRealtime = _lastAutosave = Time.realtimeSinceStartupAsDouble;
        Changed?.Invoke();
    }

    public LocationState GetLocation(string id) => State == null ? null : State.locations.Find(cafe => cafe.id == id);
    public GCData GetAssignedEmployee(LocationState cafe, int slot) => cafe == null || slot < 0 || slot >= 3 || slot >= cafe.staffIds.Count ? null :
        _save.Roster.GetCharacterById(cafe.staffIds[slot]);
    public LocationState Employer(GCData staff) => staff == null || State == null ? null : State.locations.Find(cafe => cafe.staffIds.Contains(staff.id));
    public bool HasSpace(LocationState cafe) => cafe != null && cafe.staffIds.Exists(string.IsNullOrEmpty);
    private void NormalizeAssignments()
    {
        var employed = new HashSet<string>();
        foreach (var cafe in State.locations)
        {
            while (cafe.staffIds.Count < 3) cafe.staffIds.Add("");
            if (cafe.staffIds.Count > 3) cafe.staffIds.RemoveRange(3, cafe.staffIds.Count - 3);
            for (int i = 0; i < 3; i++)
            {
                string id = cafe.staffIds[i]; var staff = _save.Roster.GetCharacterById(id);
                if (staff == null || _save.Roster.GetLevel(staff) <= 0 || !employed.Add(id)) cafe.staffIds[i] = "";
            }
        }
    }
    private bool ToggleActiveAssignment(GCData staff)
    {
        if (!Ready || staff == null) return false;
        var employer = Employer(staff);
        if (employer != null && employer != Active)
        {
            if (!HasSpace(Active)) return false;
            TransferRequested?.Invoke(staff, Active.id);
            return false;
        }
        return TryAssign(staff, Active.id);
    }

    public void RequestAssignment(GCData staff, string destinationId)
    {
        var destination = GetLocation(destinationId);
        if (!Ready || staff == null || destination == null || _save.Roster.GetLevel(staff) <= 0) return;
        var employer = Employer(staff);
        if (employer != null && employer != destination)
        {
            if (HasSpace(destination)) TransferRequested?.Invoke(staff, destinationId);
            return;
        }
        TryAssign(staff, destinationId);
    }

    public bool TryAssign(GCData staff, string destinationId)
    {
        var destination = GetLocation(destinationId);
        if (!Ready || IsTransitioning || _suspended || staff == null || destination == null || _save.Roster.GetLevel(staff) <= 0) return false;
        var employer = Employer(staff);
        if (employer != destination && !HasSpace(destination)) { LastError = "That café's team is full."; return false; }
        SettleOnline(); _save.CaptureActive(Active);
        var candidate = FranchiseSaveStore.Clone(State);
        var target = candidate.locations.Find(cafe => cafe.id == destinationId);
        foreach (var cafe in candidate.locations)
            for (int i = 0; i < 3; i++) if (cafe.staffIds[i] == staff.id) cafe.staffIds[i] = "";
        if (employer != destination) target.staffIds[target.staffIds.FindIndex(string.IsNullOrEmpty)] = staff.id;
        candidate.gold = _save.Resources.gold; candidate.tickets = (int)_save.Resources.tickets;
        candidate.ownedCharacters = _save.Roster.ToSaveData().ownedCharacters;
        RecalculateRates(candidate);
        if (!_save.Commit(candidate)) { LastError = _save.LastError; return false; }
        _save.Adopt(candidate);
        _save.Roster.NotifyAssignmentsChanged();
        LastError = null; Changed?.Invoke();
        return true;
    }
    public double PreviewRate(LocationState cafe, GCData staff, bool add)
    {
        if (cafe == null) return 0;
        var copy = JsonUtility.FromJson<LocationState>(JsonUtility.ToJson(cafe));
        for (int i = 0; i < copy.staffIds.Count; i++) if (staff != null && copy.staffIds[i] == staff.id) copy.staffIds[i] = "";
        if (add && staff != null && copy.staffIds.Exists(string.IsNullOrEmpty)) copy.staffIds[copy.staffIds.FindIndex(string.IsNullOrEmpty)] = staff.id;
        return CalculateRate(copy);
    }

    public bool IsMachineUnlocked(CMData machine) => machine != null && State != null &&
        (State.unlockedMachineIds.Contains(machine.id) || machine.unlockLevel <= State.highestFame);
    private void UpdateUnlocks(int fame)
    {
        if (State == null) return;
        State.highestFame = Math.Max(State.highestFame, fame);
        foreach (var machine in _save.MachineDatabase)
            if (machine != null && machine.unlockLevel <= State.highestFame && !State.unlockedMachineIds.Contains(machine.id)) State.unlockedMachineIds.Add(machine.id);
        Changed?.Invoke();
    }
    private void RecordSale(double amount) { if (Ready && amount > 0) Active.lifetimeRevenue += amount; }
    private void RosterChanged()
    {
        if (!Ready || _restoring) return;
        RecalculateRates(); Changed?.Invoke();
    }
    public double CalculateRate(LocationState cafe) => LocationIncomeCalculator.Calculate(cafe, _save.GetCoffeeMakerById,
        _save.Roster.GetCharacterById, _save.Roster.GetLevel, _config.PassiveEfficiency,
        _save.Resources.MaxCostReduction, _save.Resources.MaxTipChance, _save.Resources.TipGoldBonus);
    private void RecalculateRates() { RecalculateRates(State); }
    private void RecalculateRates(FranchiseState state)
    {
        foreach (var cafe in state.locations) cafe.goldPerSecond = cafe.retired ? CalculateRate(cafe) : 0;
    }
    public double TotalRetiredRate
    {
        get { double total = 0; if (State != null) foreach (var cafe in State.locations) if (cafe.retired) total += cafe.goldPerSecond; return total; }
    }
    public void SettleOnline()
    {
        if (!Ready || _suspended || _restoring) return;
        double now = Time.realtimeSinceStartupAsDouble;
        CreditSeconds(Math.Max(0, now - _lastRealtime)); _lastRealtime = now;
        State.settledThroughUtc = Math.Max(State.settledThroughUtc, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }
    private double CreditSeconds(double seconds)
    {
        double gold = 0;
        foreach (var cafe in State.locations)
        {
            if (!cafe.retired) continue;
            double amount = cafe.goldPerSecond * seconds;
            cafe.lifetimeRevenue += amount; gold += amount;
        }
        _save.Resources.AddGold(gold); State.gold = _save.Resources.gold;
        return gold;
    }
    private void CreditOffline(long now)
    {
        double seconds = LocationIncomeCalculator.OfflineSeconds(State.settledThroughUtc, now, _config.OfflineCapSeconds);
        LastOfflineGold = CreditSeconds(seconds);
        State.settledThroughUtc = Math.Max(State.settledThroughUtc, now);
        if (LastOfflineGold > 0) OfflineIncomeCredited?.Invoke();
    }
    private void Pause() { SettleOnline(); _suspended = true; }
    private void Resume(long now)
    {
        if (!Ready || !_suspended) return;
        long elapsed = Math.Max(0, now - State.settledThroughUtc);
        CreditOffline(now);
        _restoring = true; _save.ApplyState(State, elapsed); _restoring = false;
        RecalculateRates(); _save.Roster.NotifyAssignmentsChanged();
        _lastRealtime = _lastAutosave = Time.realtimeSinceStartupAsDouble; _suspended = false;
        Changed?.Invoke();
    }
    public bool CanExpand => Ready && !_suspended && !IsTransitioning &&
        _save.Resources.fameLevel >= _config.RequiredFame && _save.Resources.gold >= _config.OpeningCost;

    public bool TryOpenLocation(string expectedActiveId = null)
    {
        if (!CanExpand || (expectedActiveId != null && Active.id != expectedActiveId))
        { LastError = "Reach the current café's required Fame and afford the opening price."; return false; }
        SettleOnline();
        _save.CaptureActive(Active);
        var candidate = FranchiseSaveStore.Clone(State);
        var old = candidate.locations.Find(cafe => cafe.id == candidate.activeLocationId);
        old.retired = true;
        // Retired cafés are rate-based, not additional simulated brewing slots.
        foreach (var machine in old.machines)
        { if (machine == null) continue; machine.isBrewing = false; machine.isReadyToSell = false; machine.brewStartTime = 0; }
        candidate.gold = _save.Resources.gold - _config.OpeningCost;
        candidate.tickets = (int)_save.Resources.tickets;
        candidate.ownedCharacters = _save.Roster.ToSaveData().ownedCharacters;
        var cafe = new LocationState { id = Guid.NewGuid().ToString("N"), name = "Café " + (candidate.locations.Count + 1) };
        for (int i = 0; i < _save.SlotCount; i++) cafe.machines.Add(new CoffeeMakerSaveData
        { coffeeMakerId = i == 0 ? _config.StarterMachine.id : null });
        candidate.locations.Add(cafe); candidate.activeLocationId = cafe.id;
        candidate.settledThroughUtc = Math.Max(candidate.settledThroughUtc, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        RecalculateRates(candidate);
        IsTransitioning = true;
        try
        {
            if (!_save.Commit(candidate)) { LastError = _save.LastError; return false; }
            _restoring = true;
            if (_menu != null) _menu.PrepareForLocationChange();
            TransitionStarted?.Invoke();
            _save.ApplyState(candidate);
            _restoring = false;
            _save.Roster.NotifyAssignmentsChanged();
            _lastRealtime = Time.realtimeSinceStartupAsDouble;
            LastError = null; Changed?.Invoke();
            return true;
        }
        finally { _restoring = false; IsTransitioning = false; }
    }

    private void Update()
    {
        if (!Ready || _suspended || IsTransitioning) return;
        SettleOnline();
        if (Time.realtimeSinceStartupAsDouble - _lastAutosave >= _autosaveSeconds)
        { _lastAutosave = Time.realtimeSinceStartupAsDouble; _save.SaveGame(); }
    }
}
