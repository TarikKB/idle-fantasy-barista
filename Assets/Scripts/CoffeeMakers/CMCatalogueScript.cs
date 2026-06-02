using UnityEngine;

public class CMCatalogueScript : MonoBehaviour
{
    [SerializeField] private CMDatabase database;
    [SerializeField] private GameObject listingPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void OnEnable()
    // {
        
    //     if (database != null)
    //     {
    //         PopulateCatalogue();
    //     }
        
    // }

    public void PopulateCatalogue(CoffeeManagerScript coffeeMaker = null)
    {
        // Clear existing listings
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Create new listings based on the database
        foreach (CMData data in database.coffeeMakers)
        {
            GameObject listingObj = Instantiate(listingPrefab, transform);
            CMListing listing = listingObj.GetComponent<CMListing>();
            listing.coffeeMakerData = data;
            listing.currentCoffeeMaker = coffeeMaker;
            listing.PopulateListing();
        }
    }
}
