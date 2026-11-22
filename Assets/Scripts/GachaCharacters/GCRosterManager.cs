using System;
using System.Collections.Generic;
using UnityEngine;

public class GCRosterManager : MonoBehaviour
{
    [SerializeField] private GCDatabase database;
    [SerializeField] private GCLevelingConfig levelingConfig;
    [SerializeField] private List<OwnedCharacter> ownedCharacters = new();

    public event Action OnRosterChanged;
    public event Action BeforeRosterChanged;
    public Func<int, GCData> AssignedEmployeeQuery { get; set; }
    public Func<GCData, bool> ToggleAssignmentOverride { get; set; }
    public GCData GetCharacterById(string id) => string.IsNullOrEmpty(id) || database == null ? null : database.GetById(id);
    public void NotifyAssignmentsChanged() { OnRosterChanged?.Invoke(); }

    public const int EmployeeSlotCount = 3;
    [SerializeField] private GCData[] assignedEmployees = new GCData[EmployeeSlotCount];

    public GCData GetAssignedEmployee(int slot) => AssignedEmployeeQuery != null ? AssignedEmployeeQuery(slot) :
        slot >= 0 && slot < EmployeeSlotCount && slot < assignedEmployees.Length ? assignedEmployees[slot] : null;

    public bool IsAssigned(GCData data)
    {
        if (data == null) return false;
        for (int i = 0; i < EmployeeSlotCount; i++) if (GetAssignedEmployee(i) == data) return true;
        return false;
    }

    public bool TryToggleAssignment(GCData data)
    {
        if (ToggleAssignmentOverride != null) return ToggleAssignmentOverride(data);
        if (data == null || GetLevel(data) <= 0) return false;
        System.Array.Resize(ref assignedEmployees, EmployeeSlotCount);
        int slot = System.Array.IndexOf(assignedEmployees, data);
        if (slot >= 0) assignedEmployees[slot] = null;
        else
        {
            slot = System.Array.IndexOf(assignedEmployees, null);
            if (slot < 0) return false;
            assignedEmployees[slot] = data;
        }
        OnRosterChanged?.Invoke();
        return true;
    }

    public float GetBuffBonus(EmployeeBuffType type) => GetBuffBonus(type, null);

    public float GetBuffBonus(EmployeeBuffType type, CMData machine)
    {
        float bonus = 0f;
        for (int i = 0; i < EmployeeSlotCount; i++)
        {
            GCData data = GetAssignedEmployee(i);
            if (data != null && data.BuffType == type && data.AppliesTo(machine))
                bonus += data.GetBuffBonus(GetLevel(data));
        }
        return bonus;
    }

    public string GetTeamSummary()
    {
        var text = new System.Text.StringBuilder("Assigned staff: ");
        for (int i = 0; i < EmployeeSlotCount; i++)
        {
            if (i > 0) text.Append(" | ");
            GCData data = GetAssignedEmployee(i);
            text.Append(data != null ? data.characterName : "Empty");
        }
        bool hasBuff = false;
        for (int type = 0; type <= (int)EmployeeBuffType.SignatureRoast; type++)
        {
            GCData example = null;
            for (int i = 0; i < EmployeeSlotCount; i++)
            {
                GCData data = GetAssignedEmployee(i);
                if (data != null && (int)data.BuffType == type && data.GetBuffBonus(GetLevel(data)) > 0f)
                { example = data; break; }
            }
            if (example == null) continue;
            // Machine bonuses remain target-qualified; do not present them as global totals.
            var targets = new HashSet<CMData>();
            for (int i = 0; i < EmployeeSlotCount; i++)
            {
                GCData data = GetAssignedEmployee(i);
                if (data == null || (int)data.BuffType != type || !targets.Add(data.TargetMachine)) continue;
                float percent = GetBuffBonus(data.BuffType, data.TargetMachine) * 100f;
                if (percent <= 0f) continue;
                text.Append(hasBuff ? " | " : "\n");
                hasBuff = true;
                string label = data.BuffType switch
                {
                    EmployeeBuffType.BeanGeneration => "Beans",
                    EmployeeBuffType.BrewingSpeed or EmployeeBuffType.EspressoVirtuoso => "Brew",
                    EmployeeBuffType.SalePrice or EmployeeBuffType.HouseBlendExpert or EmployeeBuffType.ManaResonance => "Sales",
                    EmployeeBuffType.FameXP or EmployeeBuffType.SignatureRoast => "Fame",
                    EmployeeBuffType.BeanStorage => "Storage",
                    EmployeeBuffType.BeanCostReduction or EmployeeBuffType.ArcaneEfficiency => "Bean cost",
                    EmployeeBuffType.TicketChance => "Tickets",
                    EmployeeBuffType.TipChance => "Tips",
                    _ => "Buff"
                };
                if (data.IsMachineSpecialty) text.Append(data.TargetMachine.coffeeMakerName).Append(' ');
                text.Append(label).Append(data.BuffType == EmployeeBuffType.BeanCostReduction || data.BuffType == EmployeeBuffType.ArcaneEfficiency ? " -" : " +");
                text.Append(percent.ToString("0.#")).Append(data.BuffType == EmployeeBuffType.TicketChance ? " pp" : "%");
                if (data.BuffType == EmployeeBuffType.TipChance) text.Append(" chance (+50% gold)");
            }
        }
        if (!hasBuff) text.Append("\nNo active employee buffs");
        return text.ToString();
    }
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
        BeforeRosterChanged?.Invoke();
        AddCopy(data);
        OnRosterChanged?.Invoke();
    }

    public void AddCharacters(IEnumerable<GCData> characters)
    {
        BeforeRosterChanged?.Invoke();
        foreach (GCData data in characters)
        {
            AddCopy(data);
        }
        OnRosterChanged?.Invoke();
    }

    public void Clear()
    {
        ownedCharacters.Clear();
        assignedEmployees = new GCData[EmployeeSlotCount];
        OnRosterChanged?.Invoke();
    }

    public RosterSaveData ToSaveData()
    {
        RosterSaveData saveData = new RosterSaveData();
        for (int i = 0; i < EmployeeSlotCount; i++)
        {
            GCData data = GetAssignedEmployee(i);
            saveData.assignedCharacterIds.Add(data != null ? data.id : "");
        }
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

        assignedEmployees = new GCData[EmployeeSlotCount];
        if (database != null && saveData != null && saveData.assignedCharacterIds != null)
        {
            for (int i = 0; i < Mathf.Min(EmployeeSlotCount, saveData.assignedCharacterIds.Count); i++)
            {
                string id = saveData.assignedCharacterIds[i];
                if (string.IsNullOrEmpty(id)) continue;
                GCData data = database.GetById(id);
                if (data != null && GetLevel(data) > 0 && !IsAssigned(data)) assignedEmployees[i] = data;
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
