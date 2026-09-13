using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class CoffeeMakerSaveData
{
    public CMData coffeeMakerData;
    public bool isBrewing;
    public bool isReadyToSell;
    public long brewStartTime;
}

[Serializable]
public class GameState
{
    public List<CoffeeMakerSaveData> coffeeMakers = new List<CoffeeMakerSaveData>();
    public int gold = 0;
    public int beans = 0;
    public int tickets = 0;
    public long saveTime = 0;
}



public class MobileSaveManager : MonoBehaviour
{
    public static MobileSaveManager Instance;

    [SerializeField] private List<CoffeeManagerScript> sceneCoffeeMakers = new List<CoffeeManagerScript>();

    private string saveFilePath;
    private bool isInitialized = false;

    private ResourceManagerScript resourceManager;

    private float beanRate;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        beanRate = resourceManager != null ? resourceManager.beanRate : 0f;

        saveFilePath = Path.Combine(Application.persistentDataPath, "coffeesave.json");
    }

    private void Start()
    {
        LoadGame();
        isInitialized = true;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!isInitialized) return;

        if (pauseStatus)
        {
            SaveGame();
        }
        else
        {
            LoadGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void ResetGame()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
        resourceManager.SetGold(100);
        resourceManager.SetBeans(0);
        resourceManager.SetTickets(0);
        foreach (var maker in sceneCoffeeMakers)
        {
            maker.DeleteMachine();
        }
        SaveGame();
    }

    public void SaveGame()
    {
        GameState state = new GameState();
        state.saveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var maker in sceneCoffeeMakers)
        {
            CoffeeMakerSaveData data = new CoffeeMakerSaveData
            {
                coffeeMakerData = maker.coffeeMakerData,
                isBrewing = maker.IsBrewing(),
                isReadyToSell = maker.IsReadyToSell(),
                brewStartTime = maker.GetBrewStartTime()
            };

            state.coffeeMakers.Add(data);
        }
        state.gold = resourceManager != null ? (int)resourceManager.gold : 0;
        state.beans = resourceManager != null ? (int)resourceManager.beans : 0;
        state.tickets = resourceManager != null ? (int)resourceManager.tickets : 0;
        string json = JsonUtility.ToJson(state, true);
        File.WriteAllText(saveFilePath, json);
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath)) return;

        string json = File.ReadAllText(saveFilePath);
        GameState state = JsonUtility.FromJson<GameState>(json);
        long elapsedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - state.saveTime;
        resourceManager.SetGold(state.gold);
        resourceManager.SetBeans(state.beans + beanRate * elapsedTime); // Add beans for the time elapsed since last save
        resourceManager.SetTickets(state.tickets);
        for (int i = 0; i < sceneCoffeeMakers.Count && i < state.coffeeMakers.Count; i++)
        {
            var savedData = state.coffeeMakers[i];

            sceneCoffeeMakers[i].RestoreOfflineState(
                savedData.coffeeMakerData,
                savedData.isBrewing,
                savedData.isReadyToSell,
                savedData.brewStartTime
            );
        }
    }
}