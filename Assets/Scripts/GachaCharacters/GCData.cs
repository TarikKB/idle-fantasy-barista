using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Character Data", menuName = "Gacha/Character Info")]
public class GCData : ScriptableObject
{   
    [Header("Identity")]
    // Stable save key, never change once shipped (renaming characterName is safe)
    public string id;
    public string characterName;
    public int tier;

    // Will use a list to simplify the buffs, each position corresponds to the buff it gives
    // Eg: Common Buffs -> Gold increase, bean increase, coffee point increase, coffee sell price increase
    // [Header("Buffs")]
    // public List<int> commonBuffs;
}