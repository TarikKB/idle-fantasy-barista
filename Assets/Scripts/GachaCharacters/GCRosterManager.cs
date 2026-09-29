using System.Collections.Generic;
using UnityEngine;

public class GCRosterManager : MonoBehaviour
{
    [SerializeField] private List<OwnedCharacter> ownedCharacters = new();

    public bool TryGetOwned(GCData data, out OwnedCharacter owned)
    {
        owned = ownedCharacters.Find(c => c.data == data);
        return owned != null;
    }

    public int GetLevel(GCData data)
    {
        return TryGetOwned(data, out OwnedCharacter owned) ? owned.level : 0;
    }

    public void AddCharacter(GCData data)
    {
        if (TryGetOwned(data, out OwnedCharacter existing))
            existing.level++;
        else
            ownedCharacters.Add(new OwnedCharacter(data));
    }
}