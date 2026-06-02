using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CoffeeManagerScript : MonoBehaviour
{

    // [SerializeField] private float beansPerCoffee = 10f;
    // [SerializeField] private float coffeePrice = 5f;
    // [SerializeField] private float coffeeBrewTime = 2f;

    public CMData coffeeMakerData;

    private ResourceManagerScript resourceManager;

    private Image makerSprite;

    [SerializeField] private bool brewing = false;
    
    [SerializeField] private bool readyToSell = false;

    private int currentIconIndex = 0;


    void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        GameObject spriteObj = transform.GetChild(0).gameObject;
        makerSprite = spriteObj.GetComponent<Image>();
        if (coffeeMakerData != null && coffeeMakerData.icons.Length > 0)
        {
            makerSprite.sprite = coffeeMakerData.icons[0];
        } else
        {
            makerSprite.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (!brewing && !readyToSell)
        {
            BrewCoffee();
            print("Brewing coffee...");
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
            StartCoroutine(BrewCoffeeCoroutine());
            StartCoroutine(UpdateMakerSpriteCoroutine());
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
            resourceManager.AddGold(coffeeMakerData.sellPrice);
            readyToSell = false;
            currentIconIndex = 0;
            makerSprite.sprite = coffeeMakerData.icons[currentIconIndex];
        }
    }
}
