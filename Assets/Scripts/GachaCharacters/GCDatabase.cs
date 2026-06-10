using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Character Database", menuName = "Gacha/Gacha Database")]
public class GCDatabase : ScriptableObject
{
    public List<GCData> gachaCharacters;
}