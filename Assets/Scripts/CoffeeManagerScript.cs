using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    void Start()
    {
        menuManager = FindFirstObjectByType<MenuManagerScript>();
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        lineManager = FindFirstObjectByType<LineScript>();

        // GameObject spriteObj = transform.GetChild(2).gameObject;
        // makerSprite = spriteObj.GetComponent<Image>();
        
        brewProgressSlider = GetComponentInChildren<Slider>();
        
        if (coffeeMakerData != null && coffeeMakerData.icons.Length > 0)
        {
            makerSprite.sprite = coffeeMakerData.icons[0];
            makerSprite.preserveAspect = true;
            makerSprite.gameObject.SetActive(true);
            empty = false;
        } else
        {
            makerSprite.gameObject.SetActive(false);
        }
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
            // make this pop up the coffee maker catalogue
            menuManager.ToggleCatalogue(this);
            return;
        } else if (menuManager.sellMode)
        {
            if (!empty){
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
            // print("Brewing coffee...");
            resourceManager.AddBeans(-coffeeMakerData.beansRequired);
            brewing = true;
            StartCoroutine(BrewCoffeeCoroutine());
            StartCoroutine(UpdateMakerSpriteCoroutine());
            DOTween.To(() => brewProgressSlider.value, x => brewProgressSlider.value = x, 1f, coffeeMakerData.brewTimeSeconds)
                .SetEase(Ease.Linear)
                .OnComplete(() => brewProgressSlider.value = 0f);
        }
    }

    IEnumerator UpdateMakerSpriteCoroutine()
    {
        yield return new WaitForSeconds(coffeeMakerData.brewTimeSeconds / 4f);
        currentIconIndex = currentIconIndex + 1;
        makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
        if (brewing)
        {
            StartCoroutine(UpdateMakerSpriteCoroutine());
        }
    }

    IEnumerator BrewCoffeeCoroutine()
    {
        yield return new WaitForSeconds(coffeeMakerData.brewTimeSeconds);

        brewing = false;
        readyToSell = true;
    }

    private void SellCoffee()
    {
        if (readyToSell)
        {
            // print("Selling coffee...");
            resourceManager.AddGold(coffeeMakerData.sellPrice);
            readyToSell = false;
            currentIconIndex = 0;
            makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
            lineManager.AddCustomerToLine();
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

    private void ClearSoldMachine()
    {
        StopAllCoroutines();
        if (brewProgressSlider != null)
        {
            DOTween.Kill(brewProgressSlider);
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
        currentIconIndex = 0;
        empty = true;

        makerSprite.gameObject.SetActive(false);
    }
}
