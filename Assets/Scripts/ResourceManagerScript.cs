using TMPro;
using UnityEngine;

public class ResourceManagerScript : MonoBehaviour
{
    [Header("Gold")]
    public TextMeshProUGUI goldText;
    public float gold {get; private set;}
    public float sellPriceMultiplier = 1f;

    [Header("Beans")]
    public TextMeshProUGUI beansText;
    public float beans {get; private set;}
    public float beanRate = 1f;

    [Header("Tickets")]
    public TextMeshProUGUI ticketsText;
    public float tickets {get; private set;}

    [Header("Level")]
    public TextMeshProUGUI levelText;
    public int level {get; private set;}
    [SerializeField] private int[] levelUpCosts;
    public float xp {get; private set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        beans += beanRate * Time.deltaTime;
        UpdateBeansText();
        
    }

    public void AddGold(float amount)
    {
        gold += amount;
        UpdateGoldText();
    }

    public void AddBeans(float amount)
    {
        beans += amount;
        UpdateBeansText();
    }

    public void AddTickets(float amount)
    {
        tickets += amount;
        UpdateTicketsText();
    }

    public void AddXP(float amount)
    {
        xp += amount;
        UpdateLevelText();
    }

    private void UpdateGoldText()
    {
        goldText.text = "Gold: " + gold.ToString("F0");
    }

    private void UpdateBeansText()
    {
        beansText.text = "Beans: " + beans.ToString("F0");
    }

    private void UpdateTicketsText()
    {
        ticketsText.text = "Tickets: " + tickets.ToString("F0");
    }

    private void UpdateLevelText()
    {
        levelText.text = "Level: " + level.ToString();
    }
}
