using UnityEngine;

public class MenuManagerScript : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject coffeeMakerCataloguePanel;
    [SerializeField] private CMCatalogueScript coffeeMakerCatalogueScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuPanel.SetActive(false);
        coffeeMakerCataloguePanel.SetActive(false);
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
