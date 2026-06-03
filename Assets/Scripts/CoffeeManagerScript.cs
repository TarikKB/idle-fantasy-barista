using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CoffeeManagerScript : MonoBehaviour
{

    public CMData coffeeMakerData;

    private ResourceManagerScript resourceManager;

    private Image makerSprite;

    private Slider brewProgressSlider;

    private MenuManagerScript menuManager;

    [SerializeField] private bool brewing = false;
    
    [SerializeField] private bool readyToSell = false;

    private int currentIconIndex = 0;

    private bool empty = true;


    void Start()
    {
        menuManager = FindFirstObjectByType<MenuManagerScript>();
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();

        GameObject spriteObj = transform.GetChild(0).gameObject;
        makerSprite = spriteObj.GetComponent<Image>();
        
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

    public void SetCoffeeMakerData(CMData data)
    {
        coffeeMakerData = data;
        if (coffeeMakerData != null && coffeeMakerData.icons.Length > 0)
        {
            makerSprite.sprite = coffeeMakerData.icons[0];
            makerSprite.preserveAspect = true;
            makerSprite.gameObject.SetActive(true);
            empty = false;
        }
    }

    public void OnClick()
    {
        if (empty)
        {
            // make this pop up the coffee maker catalogue
            menuManager.ToggleCatalogue(this);
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
            print("Brewing coffee...");
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
            print("Selling coffee...");
            resourceManager.AddGold(coffeeMakerData.sellPrice);
            readyToSell = false;
            currentIconIndex = 0;
            makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
        }
    }
}
