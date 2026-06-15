using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class MenuManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject coffeeMakerCataloguePanel;
    [SerializeField] private CMCatalogueScript coffeeMakerCatalogueScript;
    [SerializeField] private GameObject sellCoffeePanel;

    [SerializeField] private TextMeshProUGUI sellPanelText;  
    [SerializeField] private Image sellPanelImage;    
    [SerializeField] private GameObject sellPanelConfirmButton;        

    [SerializeField] public bool sellMode;
    [SerializeField] private Image sellModeButton;

    private CoffeeManagerScript pendingSellTarget;

    private ResourceManagerScript resourceManager;

    void Start()
    {
        menuPanel.SetActive(false);
        coffeeMakerCataloguePanel.SetActive(false);
        sellModeButton.color = new Color(1f, 1f, 1f);
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
    }

    public void SetCoffeeMachineSellMode()
    {
        if (sellCoffeePanel.activeSelf)
        {
            return;
        }
        sellMode = !sellMode;
        sellModeButton.color = sellMode ? Color.red : Color.white;
        foreach (var maker in FindObjectsByType<CoffeeManagerScript>(FindObjectsSortMode.None))
        {
            maker.SetSellIndicatorVisible(sellMode);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseActivePanel()
    {
        if (coffeeMakerCataloguePanel.activeSelf)
        {
            coffeeMakerCataloguePanel.SetActive(false);
        }
        if (menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
        }
        if (sellCoffeePanel.activeSelf)
        {
            sellCoffeePanel.SetActive(false);
        }
    }

    public void ToggleCatalogue(CoffeeManagerScript coffeeMaker)
    {
        if (coffeeMakerCataloguePanel.activeSelf)
        {
            coffeeMakerCataloguePanel.SetActive(false);
        }
        else
        {
            coffeeMakerCataloguePanel.SetActive(true);
            coffeeMakerCatalogueScript.PopulateCatalogue(coffeeMaker);
        }
    }

    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }



    public void ToggleSellPanel(CoffeeManagerScript maker)
    {
        // Technically can be activated  by another click
        if (!sellCoffeePanel.activeSelf)
        {
            if (maker.CanSellMachine())
            {
                //TODO: @gmaddalozzo for future reference make this auto translate
                sellPanelText.text = $"Sell for ${maker.GetSellValue()}?";
                sellPanelConfirmButton.SetActive(true);
            } else
            {
                sellPanelText.text = "Unable to sell this machine because it is either brewing or coffee ready to be sold";
                sellPanelConfirmButton.SetActive(false);
            }
            sellPanelImage.sprite = maker.coffeeMakerData.icons[0];
            sellCoffeePanel.SetActive(true);
            pendingSellTarget = maker;
        }
    }

    public void ConfirmSellMachine()
    {
        if (pendingSellTarget != null && pendingSellTarget.TrySellMachine())
        {
            sellCoffeePanel.SetActive(false);
            pendingSellTarget.sellIndicator.SetActive(false);
            pendingSellTarget = null;
        }
    }
}
