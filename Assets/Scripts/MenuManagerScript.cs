using UnityEngine;
using UnityEngine.UI;

public class MenuManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject coffeeMakerCataloguePanel;
    [SerializeField] private CMCatalogueScript coffeeMakerCatalogueScript;

    [SerializeField] private bool sellMode;
    [SerializeField] private Image sellModeButton;

    void Start()
    {
        menuPanel.SetActive(false);
        coffeeMakerCataloguePanel.SetActive(false);
        sellModeButton.color = new Color(1f, 1f, 1f);
    }

    public void SetCoffeeMachineSellMode()
    {
        if (sellMode)
        {
            sellMode = false;
            sellModeButton.color = new Color(1f, 1f, 1f);
        }
        else
        {
            sellMode = true;
            sellModeButton.color = new Color(1f, 0f, 0f);
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
}
