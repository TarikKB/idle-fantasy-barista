using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Coffee Maker Database", menuName = "Coffee/Coffee Maker Database")]
public class CMDatabase : ScriptableObject
{
    public List<CMData> coffeeMakers;
}