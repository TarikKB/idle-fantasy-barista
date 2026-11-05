using System;
using System.Collections.Generic;
using UnityEngine;

public class GCRosterManager : MonoBehaviour
{
    [SerializeField] private GCDatabase database;
    [SerializeField] private GCLevelingConfig levelingConfig;
    [SerializeField] private List<OwnedCharacter> ownedCharacters = new();

    public event Action OnRosterChanged;

    public bool TryGetOwned(GCData data, out OwnedCharacter owned)
    {
        owned = ownedCharacters.Find(c => c.data == data);
        return owned != null;
    }

    public int GetLevel(GCData data)
    {
        return TryGetOwned(data, out OwnedCharacter owned) ? owned.level : 0;
    }

    public int GetCopies(GCData data)
    {
        return TryGetOwned(data, out OwnedCharacter owned) ? owned.copies : 0;
    }

    // Returns -1 when not owned or already at max level
    public int GetCopiesForNextLevel(GCData data)
    {
        if (!TryGetOwned(data, out OwnedCharacter owned)) return -1;
        return GetCopiesForNextLevel(owned);
    }

    public void AddCharacter(GCData data)
    {
        AddCopy(data);
        OnRosterChanged?.Invoke();
    }

    public void AddCharacters(IEnumerable<GCData> characters)
    {
        foreach (GCData data in characters)
        {
            AddCopy(data);
        }
        OnRosterChanged?.Invoke();
    }

    public void Clear()
    {
        ownedCharacters.Clear();
        OnRosterChanged?.Invoke();
    }

    public RosterSaveData ToSaveData()
    {
        RosterSaveData saveData = new RosterSaveData();
        foreach (OwnedCharacter owned in ownedCharacters)
        {
            if (owned.data == null || string.IsNullOrEmpty(owned.data.id)) continue;

            saveData.ownedCharacters.Add(new OwnedCharacterSaveData
            {
                characterId = owned.data.id,
                level = owned.level,
                copies = owned.copies
            });
        }
        return saveData;
    }

    public void LoadFromSaveData(RosterSaveData saveData)
    {
        ownedCharacters.Clear();

        if (saveData != null && database != null)
        {
            foreach (OwnedCharacterSaveData saved in saveData.ownedCharacters)
            {
                GCData data = database.GetById(saved.characterId);
                if (data == null) continue;

                OwnedCharacter owned = new OwnedCharacter(data, Mathf.Max(saved.level, 1), Mathf.Max(saved.copies, 0));
                // Spend banked copies in case the cost table was rebalanced cheaper
                TryLevelUp(owned);
                ownedCharacters.Add(owned);
            }
        }

        OnRosterChanged?.Invoke();
    }

    private void AddCopy(GCData data)
    {
        if (data == null) return;

        if (TryGetOwned(data, out OwnedCharacter existing))
        {
            existing.copies++;
            TryLevelUp(existing);
        }
        else
        {
            ownedCharacters.Add(new OwnedCharacter(data));
        }
    }

    private void TryLevelUp(OwnedCharacter owned)
    {
        int needed = GetCopiesForNextLevel(owned);
        while (needed > 0 && owned.copies >= needed)
        {
            owned.copies -= needed;
            owned.level++;
            needed = GetCopiesForNextLevel(owned);
        }
    }

    private int GetCopiesForNextLevel(OwnedCharacter owned)
    {
        if (levelingConfig == null)
        {
            Debug.LogWarning("GCRosterManager has no leveling config assigned");
            return -1;
        }
        return levelingConfig.GetCopiesForNextLevel(owned.data.tier, owned.level);
    }
}
