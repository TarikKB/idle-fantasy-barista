using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public static class FranchiseVerification
{
    private static void Check(bool value, string message)
    {
        Debug.Log("[Test] " + message + ": " + value);
        if (!value) throw new Exception(message);
    }
    [MenuItem("Tools/Franchise/Verify Calculations and Save Recovery")]
    public static void VerifyPure()
    {
        var machine = ScriptableObject.CreateInstance<CMData>();
        machine.id = "fixture"; machine.brewTimeSeconds = 5; machine.sellPrice = 5; machine.beansRequired = 1;
        var cafe = new LocationState { id = "fixture", name = "Fixture" };
        cafe.machines.Add(new CoffeeMakerSaveData { coffeeMakerId = machine.id });
        double rate = LocationIncomeCalculator.Calculate(cafe, id => machine, id => null, staff => 0, .3f);
        Check(Math.Abs(rate - .3) < .000001, "Baseline sustainable income 0.30/sec");
        machine.beansRequired = 10;
        rate = LocationIncomeCalculator.Calculate(cafe, id => machine, id => null, staff => 0, .3f);
        Check(Math.Abs(rate - .15) < .000001, "Bean supply scales demand proportionally");
        machine.brewTimeSeconds = 0;
        Check(LocationIncomeCalculator.Calculate(cafe, id => machine, id => null, staff => 0, .3f) == 0, "Invalid duration skipped");
        Check(LocationIncomeCalculator.OfflineSeconds(100, 100 + 86400, 28800) == 28800, "24 hours caps at 8 hours");
        Check(LocationIncomeCalculator.OfflineSeconds(100, 50, 28800) == 0, "Clock rollback yields zero");
        var state = new FranchiseState { activeLocationId = cafe.id, gold = 1000000000000.25 };
        state.locations.Add(cafe);
        string directory = Path.Combine(Path.GetTempPath(), "FranchisePure-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, "save.json");
        try
        {
            Check(FranchiseSaveStore.Write(path, state, out string error), "Atomic initial write: " + error);
            Check(FranchiseSaveStore.Read(path).gold == state.gold, "Double precision save roundtrip");
            state.gold += .125;
            Check(FranchiseSaveStore.Write(path, state, out error), "Atomic replacement: " + error);
            Check(FranchiseSaveStore.Read(path + ".bak") != null, "Last-good backup exists");
            File.WriteAllText(path, "broken");
            Check(FranchiseSaveStore.Read(path) == null && FranchiseSaveStore.Read(path + ".bak") != null, "Corrupt primary can recover backup");
            Check(FranchiseSaveStore.Write(path, state, out error), "Corrupt primary repaired without destroying backup");
            Check(!FranchiseSaveStore.Write(Path.Combine(path, "invalid.json"), state, out error), "Filesystem failure reported");
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); UnityEngine.Object.DestroyImmediate(machine); }
    }

    private static FranchiseManager _manager;
    private static LocationsPanel _panel;
    private static string _screenshotRoot;
    private static int _stage;
    private static double _startGold;
    public static void Setup()
    {
        VerifyPure();
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var manager in root.GetComponentsInChildren<FranchiseManager>(true)) _manager = manager;
        Check(_manager != null && _manager.Ready, "Configured franchise initializes in Play Mode");
        var save = _manager.Save; var resources = save.Resources;
        Check(save.DirectoryPath.Contains("FranchiseIntegrationTest"), "User saves isolated");
        Check(_manager.Active.fameLevel == 49 && resources.gold == 50000, "Legacy café migrated without balance loss");
        var formerId = _manager.Active.id;
        Check(!_manager.CanExpand, "Fame 49 rejects expansion");
        resources.SetFameLevel(50); resources.AddFameXP(0); resources.SetGold(49999);
        Check(!_manager.CanExpand, "Fame 50 with insufficient gold rejects expansion");
        resources.SetGold(50000);
        var overrideField = typeof(MobileSaveManager).GetField("saveDirectoryOverride", BindingFlags.NonPublic | BindingFlags.Instance);
        string originalDirectory = save.DirectoryPath;
        overrideField.SetValue(save, Path.Combine(save.SavePath, "blocked"));
        Check(!_manager.TryOpenLocation(formerId) && resources.gold == 50000 && _manager.State.locations.Count == 1 && !_manager.Active.retired,
            "Failed durable commit cannot charge or retire café");
        overrideField.SetValue(save, originalDirectory);
        Check(_manager.TryOpenLocation(formerId), "Exactly affordable expansion succeeds");
        Check(_manager.State.locations.Count == 2 && resources.fameLevel == 1 && resources.xp == 0, "New café resets Fame and XP");
        Check(resources.gold < .1 && resources.purchasedMachineCount == 1, "Only opening cost charged; basic starter supplied");
        Check(_manager.State.locations[0].retired && _manager.State.locations[0].staffIds.Exists(id => !string.IsNullOrEmpty(id)), "Old café retains staff and machines");
        Check(!_manager.TryOpenLocation(formerId), "Stale confirmation and new Fame 1 cannot open twice");
        foreach (var machine in save.MachineDatabase) if (machine.unlockLevel <= 50) Check(resources.IsMachineUnlocked(machine), "Permanent unlock: " + machine.id);
        var old = _manager.State.locations[0];
        var employee = _manager.GetAssignedEmployee(old, 0);
        Check(employee != null && _manager.TryAssign(employee, _manager.Active.id), "Employee transfers to new café");
        Check(_manager.Employer(employee) == _manager.Active && !_manager.State.locations[0].staffIds.Contains(employee.id), "Employment exclusive across locations");
        old = _manager.State.locations[0];
        var second = _manager.GetAssignedEmployee(old, 1); var third = _manager.GetAssignedEmployee(old, 2);
        Check(_manager.TryAssign(second, _manager.Active.id) && _manager.TryAssign(third, _manager.Active.id), "Three-slot destination can fill");
        var fourth = save.Roster.GetCharacterById("t1_pippa_porter");
        save.Roster.AddCharacter(fourth);
        Check(!_manager.TryAssign(fourth, _manager.Active.id) && _manager.Employer(fourth) == null, "Full team rejects without mutation");
        Check(_manager.TryAssign(fourth, _manager.State.locations[0].id), "Retired café can be staffed");
        double expected = _manager.TotalRetiredRate * 28800;
        var pause = typeof(FranchiseManager).GetMethod("Pause", BindingFlags.NonPublic | BindingFlags.Instance);
        var resume = typeof(FranchiseManager).GetMethod("Resume", BindingFlags.NonPublic | BindingFlags.Instance);
        pause.Invoke(_manager, null);
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _manager.State.settledThroughUtc = now - 86400;
        resources.SetGold(10); _manager.State.gold = 10;
        resume.Invoke(_manager, new object[] { now });
        Check(Math.Abs(resources.gold - (10 + expected)) < .01, "Retired offline payout capped at 8 hours");
        double balance = resources.gold;
        resume.Invoke(_manager, new object[] { now });
        Check(resources.gold == balance, "Duplicate resume does not re-credit");
        save.SaveGame(); double live = resources.gold; save.LoadGame();
        Check(resources.gold == live, "Repeated load cannot overwrite live balance");
        var data = FranchiseSaveStore.Read(save.SavePath);
        Check(data != null && data.locations.Count == 2 && data.activeLocationId == _manager.Active.id, "Franchise and staff survive durable save");
        _panel = _manager.GetComponent<LocationsPanel>(); _panel.Toggle();
        var panelRoot = typeof(LocationsPanel).GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_panel) as GameObject;
        Check(panelRoot != null && panelRoot.activeInHierarchy, "Locations overlay and all ancestors active");
        _startGold = resources.gold; _stage = 0;
        _screenshotRoot = "Assets/FranchiseTest_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        SessionState.SetString("PlayModeTest.ScreenshotPath", _screenshotRoot + "_locations.png");
    }
    public static bool Tick(float elapsed)
    {
        if (_stage == 0 && elapsed >= 1)
        { ScreenCapture.CaptureScreenshot(_screenshotRoot + "_locations.png"); _stage++; return false; }
        if (_stage == 1 && elapsed >= 1.5f)
        { _panel.ShowStaff(); _stage++; return false; }
        if (_stage == 2 && elapsed >= 2)
        { ScreenCapture.CaptureScreenshot(_screenshotRoot + "_staff.png"); SessionState.SetString("FranchiseTest.StaffScreenshot", _screenshotRoot + "_staff.png"); _stage++; return false; }
        if (_stage == 3 && elapsed >= 2.5f)
        {
            Check(_manager.Save.Resources.gold > _startGold, "Online passive income continues while viewing staff");
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var text in root.GetComponentsInChildren<TMPro.TextMeshProUGUI>(false))
                    if (text.transform.IsChildOf(_panel.GetComponent<FranchiseManager>().transform)) { }
            _manager.Save.SaveGame();
            return true;
        }
        return false;
    }
}
