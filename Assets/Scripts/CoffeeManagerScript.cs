using System.Collections;
using UnityEngine;

public class CoffeeManagerScript : MonoBehaviour
{

    [SerializeField] private float beansPerCoffee = 10f;
    [SerializeField] private float coffeePrice = 5f;
    [SerializeField] private float coffeeBrewTime = 2f;

    private ResourceManagerScript resourceManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resourceManager = FindFirstObjectByType<ResourceManagerScript>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BrewCoffee()
    {
        if (resourceManager.beans >= beansPerCoffee)
        {
            resourceManager.AddBeans(-beansPerCoffee);
            StartCoroutine(BrewCoffeeCoroutine());
        }
    }

    IEnumerator BrewCoffeeCoroutine()
    {
        yield return new WaitForSeconds(coffeeBrewTime);
        resourceManager.AddGold(coffeePrice);
    }
}
