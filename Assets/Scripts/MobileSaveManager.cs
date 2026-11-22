using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CoffeeMakerSaveData
{
    // public CMData coffeeMakerData;
    public string coffeeMakerId;
    public bool isBrewing;
    public bool isReadyToSell;
    public long brewStartTime;
    public float brewDurationSeconds;
}

[Serializable]
public class GameState
{
    public List<CoffeeMakerSaveData> coffeeMakers = new List<CoffeeMakerSaveData>();
    public double gold = 100;
    public float beans = 0f;
    public int tickets = 0;
    public int fameLevel = 1;
    public float xp = 0f;
    public long saveTime = 0;
}

[Serializable]
public class UpgradeData
{
    public int beanRateLevel = 0;
    public int beanLimitLevel = 0;
    public int sellPriceLevel = 0;
    public int ticketRateLevel = 0;
    public int fameBonusLevel = 0;

}

[Serializable]
public class OwnedCharacterSaveData
{
    public string characterId;
    public int level;
    public int copies;
}

[Serializable]
public class RosterSaveData
{
    public List<OwnedCharacterSaveData> ownedCharacters = new List<OwnedCharacterSaveData>();
    public List<string> assignedCharacterIds = new List<string>();
}


[DefaultExecutionOrder(100)]
public class MobileSaveManager : MonoBehaviour
{
    public static MobileSaveManager Instance;
    [SerializeField] private List<CoffeeManagerScript> sceneCoffeeMakers = new List<CoffeeManagerScript>();
    [SerializeField] private List<CMData> coffeeMakerDatabase = new List<CMData>();
    [SerializeField] private ResourceManagerScript resourceManager;
    [SerializeField] private ShelfScript shelfManager;
    [SerializeField] private GCRosterManager rosterManager;
    [SerializeField, Tooltip("Empty uses the normal persistent save directory. Tests can inject an isolated directory.")]
    private string saveDirectoryOverride;
    public FranchiseState State { get; private set; }
    public string LastError { get; private set; }
    public bool IsInitialized { get; private set; }
    public event Action Loaded;
    public event Action BeforeSave;
    public event Action Pausing;
    public event Action<long> Resuming;
    public ResourceManagerScript Resources => resourceManager;
    public GCRosterManager Roster => rosterManager;
    public IReadOnlyList<CMData> MachineDatabase => coffeeMakerDatabase;
    public int SlotCount => sceneCoffeeMakers.Count;
    public string DirectoryPath => string.IsNullOrEmpty(saveDirectoryOverride) ? Application.persistentDataPath : saveDirectoryOverride;
    public string SavePath => Path.Combine(DirectoryPath, "franchise-save.json");

    private void Awake()
    {
        if (Instance != null && Instance != this) { enabled = false; return; }
        Instance = this;
    }
    private void Start() { LoadGame(); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    public CMData GetCoffeeMakerById(string id) => string.IsNullOrEmpty(id) ? null : coffeeMakerDatabase.Find(data => data != null && data.id == id);

    public void LoadGame()
    {
        // Repeated calls must not overwrite live gameplay or re-credit an offline interval.
        if (IsInitialized) return;
        if (resourceManager == null || rosterManager == null) { LastError = "Save system references are missing."; Debug.LogError(LastError); return; }
        bool hasNewSave = File.Exists(SavePath) || File.Exists(SavePath + ".bak");
        State = FranchiseSaveStore.Read(SavePath) ?? FranchiseSaveStore.Read(SavePath + ".bak");
        if (State == null && hasNewSave) { LastError = "Franchise save and backup could not be read. Your files were preserved."; Debug.LogError(LastError); return; }
        try { if (State == null) State = MigrateLegacy(); }
        catch (Exception exception) { LastError = "Unable to migrate existing save: " + exception.Message; Debug.LogError(LastError); return; }
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var active = State.locations.Find(location => location.id == State.activeLocationId);
        long elapsed = State.settledThroughUtc > 0 ? Math.Max(0, now - State.settledThroughUtc) : 0;
        ApplyState(State, elapsed);
        IsInitialized = true;
        Loaded?.Invoke();
        SaveGame();
    }

    private T ReadLegacy<T>(string name) where T : class
    {
        string path = Path.Combine(DirectoryPath, name);
        if (!File.Exists(path)) return null;
        var value = JsonUtility.FromJson<T>(File.ReadAllText(path));
        if (value == null) throw new InvalidDataException("Invalid legacy file: " + name);
        if (!File.Exists(path + ".pre-franchise")) File.Copy(path, path + ".pre-franchise");
        return value;
    }

    private FranchiseState MigrateLegacy()
    {
        var old = ReadLegacy<GameState>("coffeesave.json");
        var upgrades = ReadLegacy<UpgradeData>("upgradesave.json") ?? new UpgradeData();
        var roster = ReadLegacy<RosterSaveData>("staffsave.json") ?? rosterManager.ToSaveData();
        var state = new FranchiseState();
        var cafe = new LocationState { id = Guid.NewGuid().ToString("N"), name = "Café 1", upgrades = upgrades };
        state.activeLocationId = cafe.id;
        state.locations.Add(cafe);
        state.ownedCharacters = roster.ownedCharacters ?? new List<OwnedCharacterSaveData>();
        cafe.staffIds = roster.assignedCharacterIds ?? new List<string>();
        if (old != null)
        {
            state.gold = old.gold; state.tickets = old.tickets; state.settledThroughUtc = old.saveTime;
            cafe.beans = old.beans; cafe.fameLevel = Math.Max(1, old.fameLevel); cafe.xp = old.xp;
            cafe.machines = old.coffeeMakers ?? new List<CoffeeMakerSaveData>();
        }
        else { CaptureActive(cafe); state.gold = 100; }
        state.highestFame = cafe.fameLevel;
        foreach (var machine in coffeeMakerDatabase)
            if (machine != null && (machine.unlockLevel <= state.highestFame || cafe.machines.Exists(slot => slot != null && slot.coffeeMakerId == machine.id)))
                state.unlockedMachineIds.Add(machine.id);
        return state;
    }

    public void CaptureActive(LocationState cafe)
    {
        cafe.fameLevel = Math.Max(1, resourceManager.fameLevel); cafe.xp = resourceManager.xp; cafe.beans = resourceManager.beans;
        cafe.upgrades = JsonUtility.FromJson<UpgradeData>(JsonUtility.ToJson(resourceManager.upgradeData ?? new UpgradeData()));
        cafe.machines.Clear();
        foreach (var maker in sceneCoffeeMakers)
            cafe.machines.Add(new CoffeeMakerSaveData { coffeeMakerId = maker != null && maker.coffeeMakerData != null ? maker.coffeeMakerData.id : null,
                isBrewing = maker != null && maker.IsBrewing(), isReadyToSell = maker != null && maker.IsReadyToSell(),
                brewStartTime = maker != null ? maker.GetBrewStartTime() : 0, brewDurationSeconds = maker != null ? maker.GetBrewDurationSeconds() : 0 });
    }

    public void ApplyState(FranchiseState state, long elapsed = 0)
    {
        State = state;
        var cafe = state.locations.Find(location => location.id == state.activeLocationId);
        rosterManager.LoadFromSaveData(new RosterSaveData { ownedCharacters = state.ownedCharacters, assignedCharacterIds = cafe.staffIds });
        resourceManager.upgradeData = JsonUtility.FromJson<UpgradeData>(JsonUtility.ToJson(cafe.upgrades));
        resourceManager.ApplyUpgradeEffects();
        resourceManager.SetGold(state.gold); resourceManager.SetTickets(state.tickets);
        resourceManager.SetFameLevel(cafe.fameLevel); resourceManager.SetFameXP(cafe.xp);
        resourceManager.SetBeans(cafe.beans + resourceManager.EffectiveBeanRate * elapsed);
        int count = 0;
        for (int i = 0; i < sceneCoffeeMakers.Count; i++)
        {
            if (sceneCoffeeMakers[i] == null) continue;
            var slot = i < cafe.machines.Count ? cafe.machines[i] : null;
            var definition = slot == null ? null : GetCoffeeMakerById(slot.coffeeMakerId);
            sceneCoffeeMakers[i].RestoreOfflineState(definition, slot != null && slot.isBrewing, slot != null && slot.isReadyToSell,
                slot == null ? 0 : slot.brewStartTime, slot == null ? 0 : slot.brewDurationSeconds);
            if (definition != null) count++;
        }
        // Reconstruct count even when restoring empty slots; never pay machine refunds here.
        while (resourceManager.purchasedMachineCount > count) resourceManager.RegisterSoldMachine();
        while (resourceManager.purchasedMachineCount < count) resourceManager.RegisterPurchasedMachine();
        if (shelfManager != null) shelfManager.UpdateShelfSprites();
    }

    public bool Commit(FranchiseState candidate)
    {
        if (!FranchiseSaveStore.Write(SavePath, candidate, out string error)) { LastError = error; Debug.LogWarning(error); return false; }
        LastError = null;
        return true;
    }
    public void Adopt(FranchiseState state) { State = state; }

    public void SaveGame()
    {
        if (!IsInitialized) return;
        BeforeSave?.Invoke();
        var cafe = State.locations.Find(location => location.id == State.activeLocationId);
        CaptureActive(cafe);
        State.gold = resourceManager.gold; State.tickets = (int)resourceManager.tickets;
        State.ownedCharacters = rosterManager.ToSaveData().ownedCharacters;
        State.highestFame = Math.Max(State.highestFame, cafe.fameLevel);
        Commit(State);
    }
    private void OnApplicationPause(bool pause)
    {
        if (!IsInitialized) return;
        if (pause) { Pausing?.Invoke(); SaveGame(); }
        else { Resuming?.Invoke(DateTimeOffset.UtcNow.ToUnixTimeSeconds()); SaveGame(); }
    }
    private void OnApplicationQuit() { SaveGame(); }

    public void ResetGame()
    {
        if (!IsInitialized) return;
        var fresh = new FranchiseState();
        var cafe = new LocationState { id = Guid.NewGuid().ToString("N"), name = "Café 1" };
        fresh.locations.Add(cafe); fresh.activeLocationId = cafe.id;
        fresh.settledThroughUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!Commit(fresh)) return;
        // After durable reset, replace backup too so recovery cannot resurrect pre-reset progress.
        File.Copy(SavePath, SavePath + ".bak", true);
        foreach (string name in new[] { "coffeesave.json", "upgradesave.json", "staffsave.json" })
        {
            string path = Path.Combine(DirectoryPath, name); if (File.Exists(path)) File.Delete(path);
        }
        ApplyState(fresh);
        Loaded?.Invoke();
        SaveGame();
    }
}
