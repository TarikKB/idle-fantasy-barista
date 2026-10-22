using System.Collections.Generic;
using UnityEngine;

// Toggles automatic brewing and selling for all coffee makers in the scene.
public class AutoBrewSellManager : MonoBehaviour
{
    private const string AutoBrewPrefKey = "AutoBrewEnabled";
    private const string AutoSellPrefKey = "AutoSellEnabled";

    [Header("References")]
    [SerializeField] private List<CoffeeManagerScript> coffeeMakers = new List<CoffeeManagerScript>();
    [SerializeField] private MenuManagerScript menuManager;
    [SerializeField] private UnityEngine.UI.Image autoBrewButtonImage;
    [SerializeField] private UnityEngine.UI.Image autoSellButtonImage;

    [Header("Settings")]
    [Tooltip("Seconds between automatic brew/sell checks.")]
    [SerializeField] private float checkInterval = 0.25f;
    [SerializeField] private Color activeColor = new Color(0.55f, 1f, 0.55f);
    [SerializeField] private Color inactiveColor = Color.white;

    public bool AutoBrewEnabled { get; private set; }
    public bool AutoSellEnabled { get; private set; }

    private float _timer;

    private void Start()
    {
        AutoBrewEnabled = PlayerPrefs.GetInt(AutoBrewPrefKey, 0) == 1;
        AutoSellEnabled = PlayerPrefs.GetInt(AutoSellPrefKey, 0) == 1;
        RefreshButtonVisuals();
    }

    private void Update()
    {
        if (!AutoBrewEnabled && !AutoSellEnabled) return;

        // Pause automation while the player is selling machines so machines stay idle and sellable.
        if (menuManager != null && menuManager.sellMode) return;

        _timer += Time.deltaTime;
        if (_timer < checkInterval) return;
        _timer = 0f;

        foreach (CoffeeManagerScript maker in coffeeMakers)
        {
            if (maker == null || maker.IsEmpty()) continue;

            // Sell first so a finished machine can immediately start its next brew.
            if (AutoSellEnabled) maker.TryAutoSell();
            if (AutoBrewEnabled) maker.TryAutoBrew();
        }
    }

    public void ToggleAutoBrew()
    {
        AutoBrewEnabled = !AutoBrewEnabled;
        PlayerPrefs.SetInt(AutoBrewPrefKey, AutoBrewEnabled ? 1 : 0);
        RefreshButtonVisuals();
    }

    public void ToggleAutoSell()
    {
        AutoSellEnabled = !AutoSellEnabled;
        PlayerPrefs.SetInt(AutoSellPrefKey, AutoSellEnabled ? 1 : 0);
        RefreshButtonVisuals();
    }

    private void RefreshButtonVisuals()
    {
        if (autoBrewButtonImage != null) autoBrewButtonImage.color = AutoBrewEnabled ? activeColor : inactiveColor;
        if (autoSellButtonImage != null) autoSellButtonImage.color = AutoSellEnabled ? activeColor : inactiveColor;
    }
}
