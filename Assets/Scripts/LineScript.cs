using UnityEngine;
using System.Collections.Generic;

public class LineScript : MonoBehaviour
{
    public Transform[] linePoints;
    public Transform spawnPoint;

    private List<CustomerScript> customersInLine;

    [SerializeField] private GameObject customerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        customersInLine = new List<CustomerScript>();
        PopulateLine();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PopulateLine()
    {
        for (int i = 0; i < linePoints.Length; i++)
        {
            GameObject newCustomerObj = Instantiate(customerPrefab, spawnPoint.position - Vector3.right * i, Quaternion.identity);
            CustomerScript newCustomer = newCustomerObj.GetComponent<CustomerScript>();
            customersInLine.Add(newCustomer);
            newCustomer.MoveUpLine(linePoints[i]);
        }
    }

    public void AddCustomerToLine()
    {

        GameObject newCustomerObj = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        CustomerScript newCustomer = newCustomerObj.GetComponent<CustomerScript>();

        customersInLine.Add(newCustomer);
        
        MoveLineUp();
        
    }

    public void MoveLineUp()
    {
        if (customersInLine.Count == 0) return;

        if (customersInLine.Count >= linePoints.Length)
        {
            customersInLine[0].ExitLine(spawnPoint);
            customersInLine.RemoveAt(0);
        }

        for (int i = 0; i < customersInLine.Count; i++)
        {
            customersInLine[i].MoveUpLine(linePoints[i]);
        }
    }

    
}
