using UnityEngine;

[CreateAssetMenu(fileName = "New Coffee Maker", menuName = "Coffee/Coffee Maker")]
public class CMData : ScriptableObject
{
    [Header("Identity")]
    public string coffeeMakerName;
    public Sprite[] icons;

    [Header("Brewing")]
    public float brewTimeSeconds = 5f;
    public int coffeeProduced = 1;

    [Header("Economy")]
    public int sellPrice = 5;

    [Header("Requirements")]
    public int beansRequired = 1;

    [Header("Upgrade / Unlock")]
    public int purchaseCost = 100;
    public int unlockLevel = 1;
}