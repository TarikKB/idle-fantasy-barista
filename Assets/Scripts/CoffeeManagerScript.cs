using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoffeeManagerScript : MonoBehaviour
{
    public CMData coffeeMakerData;

    private ResourceManagerScript resourceManager;
    [SerializeField] private Image makerSprite;
    private Slider brewProgressSlider;
    private MenuManagerScript menuManager;

    [SerializeField] private bool brewing = false;
    [SerializeField] private bool readyToSell = false;
    private int currentIconIndex = 0;
    private bool empty = true;

    [SerializeField] private SpriteRenderer[] shelfSprites;
    [SerializeField] public GameObject sellIndicator;
    private LineScript lineManager;

    [SerializeField] private long brewStartTime = 0;

    public long GetBrewStartTime() => brewStartTime;
    public bool IsBrewing() => brewing;
    public bool IsReadyToSell() => readyToSell;

    void Start()
    {
        menuManager = FindFirstObjectByType<MenuManagerScript>();
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        lineManager = FindFirstObjectByType<LineScript>();
        brewProgressSlider = GetComponentInChildren<Slider>();

        if (coffeeMakerData != null && coffeeMakerData.icons.Length > 0)
        {
            makerSprite.sprite = coffeeMakerData.icons[0];
            makerSprite.preserveAspect = true;
            makerSprite.gameObject.SetActive(true);
            empty = false;
        }
        else
        {
            makerSprite.gameObject.SetActive(false);
        }
        RestoreOfflineState(coffeeMakerData, brewing, readyToSell, brewStartTime);
    }

    public void SetSellIndicatorVisible(bool visible)
    {
        if (sellIndicator != null)
            sellIndicator.SetActive(visible && !empty);
    }

    public void SetCoffeeMakerData(CMData data)
    {
        coffeeMakerData = data;
        if (coffeeMakerData != null && coffeeMakerData.icons.Length > 0)
        {
            makerSprite.sprite = coffeeMakerData.icons[0];
            makerSprite.preserveAspect = true;
            makerSprite.gameObject.SetActive(true);
            empty = false;

            for (int i = 0; i < shelfSprites.Length; i++)
            {
                if (shelfSprites[i].sprite == null)
                {
                    shelfSprites[i].sprite = coffeeMakerData.icons[0];
                    break;
                }
            }
        }
    }

    public void OnClick()
    {
        if (empty && !menuManager.sellMode)
        {
            menuManager.ToggleCatalogue(this);
            return;
        }
        else if (menuManager.sellMode)
        {
            if (!empty)
            {
                menuManager.ToggleSellPanel(this);
            }
            return;
        }

        if (!brewing && !readyToSell)
        {
            BrewCoffee();
        }
        else if (readyToSell)
        {
            SellCoffee();
        }
    }

    private void BrewCoffee()
    {
        if (resourceManager.beans >= coffeeMakerData.beansRequired)
        {
            resourceManager.AddBeans(-coffeeMakerData.beansRequired);
            brewing = true;
            readyToSell = false;

            brewStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            StartBrewRoutine(coffeeMakerData.brewTimeSeconds, 0f);
        }
    }

    private void StartBrewRoutine(float remainingTime, float elapsedTime)
    {
        StopAllCoroutines();
        StartCoroutine(BrewProcessCoroutine(remainingTime, elapsedTime));
    }

    private IEnumerator BrewProcessCoroutine(float remainingTime, float elapsedTime)
    {
        float totalTime = coffeeMakerData.brewTimeSeconds;
        float timer = elapsedTime;
        int totalIcons = coffeeMakerData.icons != null ? coffeeMakerData.icons.Length : 0;

        while (timer < totalTime)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / totalTime);

            // Update Slider Directly
            if (brewProgressSlider != null)
            {
                brewProgressSlider.value = progress;
            }

            // Update Sprite Icon across mid-brew states only (excluding the final "ready" frame)
            if (totalIcons > 1)
            {
                // Map 0.0 - 0.999 progress across indices [0 ... totalIcons - 2]
                int brewingIconCount = totalIcons - 1;
                int iconIndex = Mathf.Clamp(Mathf.FloorToInt(progress * brewingIconCount), 0, brewingIconCount - 1);

                if (iconIndex != currentIconIndex)
                {
                    currentIconIndex = iconIndex;
                    makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
                }
            }

            yield return null; // Frame-by-frame update
        }

        // Brew Complete -> Explicitly set to the Final Sprite
        brewing = false;
        readyToSell = true;
        brewStartTime = 0;

        if (brewProgressSlider != null)
        {
            brewProgressSlider.value = 0f;
        }

        if (totalIcons > 0)
        {
            currentIconIndex = totalIcons - 1; // Last sprite index
            makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
        }
    }

    public void RestoreOfflineState(CMData offlineCoffeeMakerData, bool isBrewing, bool isReady, long startTime)
    {
        if (offlineCoffeeMakerData == null) return;

        SetCoffeeMakerData(offlineCoffeeMakerData);
        brewing = isBrewing;
        readyToSell = isReady;
        brewStartTime = startTime;

        // If it was already marked as ready to sell before saving
        if (readyToSell)
        {
            brewStartTime = 0; // Guard: Ensure old timestamp is cleared
            currentIconIndex = coffeeMakerData.icons.Length - 1;
            makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
            if (brewProgressSlider != null) brewProgressSlider.value = 0f;
            return;
        }

        if (brewing && brewStartTime > 0)
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedTime = currentTime - brewStartTime;

            if (elapsedTime >= coffeeMakerData.brewTimeSeconds)
            {
                StartBrewRoutine(0f, elapsedTime);
            }
            else
            {
                // Resume brewing mid-process
                float remaining = coffeeMakerData.brewTimeSeconds - elapsedTime;
                StartBrewRoutine(remaining, elapsedTime);
            }
        }
    }

    private void SellCoffee()
    {
        if (readyToSell)
        {
            resourceManager.AddGold(coffeeMakerData.sellPrice);
            readyToSell = false;
            currentIconIndex = 0;
            makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
            lineManager.AddCustomerToLine();
            resourceManager.TicketCheck();
        }
    }

    public bool CanSellMachine() => !brewing && !readyToSell;
    public int GetSellValue() => Mathf.RoundToInt(coffeeMakerData.purchaseCost * 0.25f);

    public bool TrySellMachine()
    {
        if (!CanSellMachine()) return false;
        resourceManager.AddGold(GetSellValue());
        ClearSoldMachine();
        return true;
    }

    public void DeleteMachine()
    {
        ClearSoldMachine();
    }

    private void ClearSoldMachine()
    {
        StopAllCoroutines();
        if (brewProgressSlider != null)
        {
            brewProgressSlider.value = 0f;
        }

        if (coffeeMakerData != null && coffeeMakerData.icons.Length > 0)
        {
            Sprite icon = coffeeMakerData.icons[0];
            for (int i = 0; i < shelfSprites.Length; i++)
            {
                if (shelfSprites[i].sprite == icon)
                {
                    shelfSprites[i].sprite = null;
                    break;
                }
            }
        }

        coffeeMakerData = null;
        brewing = false;
        readyToSell = false;
        brewStartTime = 0;
        currentIconIndex = 0;
        empty = true;

        makerSprite.gameObject.SetActive(false);
    }
}