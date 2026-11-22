using System;
using System.Collections.Generic;

[Serializable]
public class FranchiseState
{
    public int schemaVersion = 1;
    public double gold = 100;
    public int tickets;
    public int highestFame = 1;
    public string activeLocationId;
    public long settledThroughUtc;
    public List<string> unlockedMachineIds = new List<string>();
    public List<OwnedCharacterSaveData> ownedCharacters = new List<OwnedCharacterSaveData>();
    public List<LocationState> locations = new List<LocationState>();
}

[Serializable]
public class LocationState
{
    public string id;
    public string name;
    public bool retired;
    public int fameLevel = 1;
    public float xp;
    public float beans;
    public UpgradeData upgrades = new UpgradeData();
    // Slot order matches MobileSaveManager's serialized sceneCoffeeMakers list.
    public List<CoffeeMakerSaveData> machines = new List<CoffeeMakerSaveData>();
    public List<string> staffIds = new List<string> { "", "", "" };
    public double goldPerSecond;
    public double lifetimeRevenue;
}
