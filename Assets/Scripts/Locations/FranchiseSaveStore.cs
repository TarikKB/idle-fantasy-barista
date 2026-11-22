using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class FranchiseSaveStore
{
    public static FranchiseState Clone(FranchiseState state) => JsonUtility.FromJson<FranchiseState>(JsonUtility.ToJson(state));

    public static bool Valid(FranchiseState state)
    {
        if (state == null || state.schemaVersion != 1 || state.locations == null || state.locations.Count == 0 ||
            state.ownedCharacters == null || state.unlockedMachineIds == null || !Finite(state.gold) || state.gold < 0) return false;
        var ids = new HashSet<string>();
        int active = 0;
        foreach (var cafe in state.locations)
        {
            if (cafe == null || string.IsNullOrEmpty(cafe.id) || !ids.Add(cafe.id) || cafe.machines == null || cafe.upgrades == null || cafe.staffIds == null ||
                cafe.fameLevel < 1 || !Finite(cafe.goldPerSecond) || cafe.goldPerSecond < 0 || !Finite(cafe.lifetimeRevenue) || cafe.lifetimeRevenue < 0 ||
                float.IsNaN(cafe.beans) || float.IsInfinity(cafe.beans) || float.IsNaN(cafe.xp) || float.IsInfinity(cafe.xp)) return false;
            if (!cafe.retired) { active++; if (cafe.id != state.activeLocationId) return false; }
        }
        return active == 1;
    }
    private static bool Finite(double number) => !double.IsNaN(number) && !double.IsInfinity(number);

    public static FranchiseState Read(string path)
    {
        try { var state = JsonUtility.FromJson<FranchiseState>(File.ReadAllText(path)); return Valid(state) ? state : null; }
        catch (Exception) { return null; }
    }

    public static bool Write(string path, FranchiseState state, out string error)
    {
        error = null;
        string temp = path + ".tmp";
        try
        {
            if (!Valid(state)) throw new InvalidDataException("Invalid franchise save state.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(state, true));
                stream.Write(data, 0, data.Length);
                stream.Flush(true);
            }
            if (Read(temp) == null) throw new InvalidDataException("Save verification failed.");
            // Never overwrite a good backup with a known-corrupt primary.
            if (File.Exists(path))
            {
                if (Read(path) != null) File.Replace(temp, path, path + ".bak");
                else File.Replace(temp, path, null);
            }
            else File.Move(temp, path);
            return true;
        }
        catch (Exception exception) { error = "Unable to save franchise: " + exception.Message; return false; }
    }
}
