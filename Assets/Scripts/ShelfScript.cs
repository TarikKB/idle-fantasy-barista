using UnityEngine;
using System.Collections.Generic;

public class ShelfScript : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] shelfSprites;
    [SerializeField] private List<CoffeeManagerScript> sceneCoffeeMakers = new List<CoffeeManagerScript>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (shelfSprites == null || shelfSprites.Length == 0)
        {
            Debug.LogWarning("ShelfScript: No shelf sprites assigned.");
            return;
        }
        UpdateShelfSprites();

    }

    public void UpdateShelfSprites()
    {
        if (shelfSprites == null || shelfSprites.Length == 0)
        {
            return;
        }

        int shelfIndex = 0;
        foreach (CoffeeManagerScript coffeeMaker in sceneCoffeeMakers)
        {
            if (coffeeMaker == null || coffeeMaker.coffeeMakerData == null ||
                coffeeMaker.coffeeMakerData.icons == null || coffeeMaker.coffeeMakerData.icons.Length == 0)
            {
                continue;
            }

            if (shelfIndex >= shelfSprites.Length)
            {
                break;
            }

            SpriteRenderer shelfSprite = shelfSprites[shelfIndex];
            if (shelfSprite == null)
            {
                continue;
            }

            shelfSprite.sprite = coffeeMaker.coffeeMakerData.icons[0];
            shelfSprite.gameObject.SetActive(true);
            shelfIndex++;
        }

        for (int i = shelfIndex; i < shelfSprites.Length; i++)
        {
            if (shelfSprites[i] == null)
            {
                continue;
            }

            shelfSprites[i].sprite = null;
            shelfSprites[i].gameObject.SetActive(false);
        }
    }
}
