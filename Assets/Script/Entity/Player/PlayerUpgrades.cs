using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    private PlayerStats stats;
    private readonly Dictionary<string, int> levels = new Dictionary<string, int>();
    private readonly Dictionary<string, SubWeapon> subweapons = new Dictionary<string, SubWeapon>();

    public IReadOnlyDictionary<string, int> Levels => levels;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public int GetLevel(string id)
    {
        return levels.TryGetValue(id, out int lvl) ? lvl : 0;
    }

    public bool IsMaxed(string id)
    {
        UpgradeDefinition def = UpgradeCatalog.Get(id);
        return def != null && GetLevel(id) >= def.MaxLevel;
    }

    // Random non-maxed upgrades. Filters & shuffles; returns up to `count`.
    public List<UpgradeDefinition> PickRandom(int count)
    {
        List<UpgradeDefinition> pool = new List<UpgradeDefinition>();
        foreach (UpgradeDefinition def in UpgradeCatalog.All())
        {
            if (!IsMaxed(def.Id)) pool.Add(def);
        }
        FisherYatesShuffle(pool);
        if (pool.Count > count) pool = pool.GetRange(0, count);
        return pool;
    }

    public bool AllMaxed()
    {
        foreach (UpgradeDefinition def in UpgradeCatalog.All())
        {
            if (!IsMaxed(def.Id)) return false;
        }
        return true;
    }

    public bool ApplyUpgrade(string id)
    {
        UpgradeDefinition def = UpgradeCatalog.Get(id);
        if (def == null || IsMaxed(id)) return false;

        int newLevel = GetLevel(id) + 1;
        levels[id] = newLevel;
        def.ApplyAtLevel(newLevel, stats, this);
        return true;
    }

    public SubWeapon GetSubWeapon(string id)
    {
        return subweapons.TryGetValue(id, out SubWeapon sw) ? sw : null;
    }

    public void RegisterSubWeapon(string id, SubWeapon sw)
    {
        subweapons[id] = sw;
        if (sw != null) sw.Bind(stats);
    }

    private static void FisherYatesShuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
