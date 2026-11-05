using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gacha Character Database", menuName = "Gacha/Gacha Database")]
public class GCDatabase : ScriptableObject
{
    public List<GCData> gachaCharacters;

    private Dictionary<string, GCData> idLookup;

    public GCData GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        if (idLookup == null || !idLookup.ContainsKey(id))
            BuildLookup();

        if (idLookup.TryGetValue(id, out GCData data))
            return data;

        Debug.LogWarning($"Could not find gacha character with ID '{id}'");
        return null;
    }

    private void BuildLookup()
    {
        idLookup = new Dictionary<string, GCData>();
        foreach (GCData data in gachaCharacters)
        {
            if (data != null && !string.IsNullOrEmpty(data.id))
                idLookup[data.id] = data;
        }
    }

    private void OnValidate()
    {
        idLookup = null;

        HashSet<string> seen = new HashSet<string>();
        foreach (GCData data in gachaCharacters)
        {
            if (data == null) continue;

            if (string.IsNullOrEmpty(data.id))
                Debug.LogWarning($"Gacha character '{data.name}' has no ID", data);
            else if (!seen.Add(data.id))
                Debug.LogWarning($"Duplicate gacha character ID '{data.id}' on '{data.name}'", data);
        }
    }
}
