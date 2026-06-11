using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CMListing : MonoBehaviour
{

    public CMData coffeeMakerData;

    private ResourceManagerScript resourceManager;

    public CoffeeManagerScript currentCoffeeMaker;

    private MenuManagerScript menuManager;


    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI costText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        menuManager = FindFirstObjectByType<MenuManagerScript>();

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PopulateListing()
    {
        if (coffeeMakerData != null)
        {
            iconImage.sprite = coffeeMakerData.icons[0];
            statsText.text = $"{coffeeMakerData.coffeeMakerName}\n" +
                             $"Brew Time: {coffeeMakerData.brewTimeSeconds}s\n" +
                             $"Sell Price: ${coffeeMakerData.sellPrice}\n" +
                             $"Beans Required: {coffeeMakerData.beansRequired}\n" +
                             $"Level Required: {coffeeMakerData.unlockLevel}";
            costText.text = $"Cost: ${coffeeMakerData.purchaseCost}";
        }
    }

    public void PurchaseCoffeeMaker()
    {
        if (currentCoffeeMaker != null && coffeeMakerData != null)
        {
            // print($"Attempting to purchase {coffeeMakerData.coffeeMakerName} for ${coffeeMakerData.purchaseCost}");
            if (resourceManager.gold >= coffeeMakerData.purchaseCost)
            {
                resourceManager.AddGold(-coffeeMakerData.purchaseCost);
                currentCoffeeMaker.SetCoffeeMakerData(coffeeMakerData);
                menuManager.CloseActivePanel();
            }
            else
            {
                // Debug.Log("Not enough money to purchase this coffee maker.");
            }
        }
    }
}
