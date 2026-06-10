using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Character Data", menuName = "Gacha/Character Info")]
public class GCData : ScriptableObject
{   
    [Header("Identity")]
    public string characterName;
    public int tier;
}