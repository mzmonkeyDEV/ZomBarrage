using System.Collections.Generic;
using UnityEngine;

public static class UpgradeCatalog
{
    private static Dictionary<string, UpgradeDefinition> registry;
    private static List<UpgradeDefinition> ordered;

    public static IEnumerable<UpgradeDefinition> All()
    {
        EnsureInitialized();
        return ordered;
    }

    public static UpgradeDefinition Get(string id)
    {
        EnsureInitialized();
        return registry.TryGetValue(id, out UpgradeDefinition d) ? d : null;
    }

    public static void Register(UpgradeDefinition def)
    {
        EnsureInitialized();
        if (def == null) return;
        registry[def.Id] = def;
        if (!ordered.Contains(def)) ordered.Add(def);
    }

    public static void ResetAll()
    {
        registry = null;
        ordered = null;
    }

    private static void EnsureInitialized()
    {
        if (registry != null) return;
        registry = new Dictionary<string, UpgradeDefinition>();
        ordered = new List<UpgradeDefinition>();

        RegisterInternal(new StatUpgradeDefinition(
            id: "health",
            displayName: "Max HP",
            maxLevel: 5,
            apply: (newLevel, stats) =>
            {
                stats.SetMaxHp(stats.maxHp + 20f, fillDelta: true);
            },
            describe: lvl => $"Max HP +20  (Lv{lvl})"
        ));

        RegisterInternal(new StatUpgradeDefinition(
            id: "might",
            displayName: "Damage",
            maxLevel: 5,
            apply: (newLevel, stats) =>
            {
                stats.SetMightMultiplier(stats.mightMultiplier + 0.1f);
            },
            describe: lvl => $"Damage +0.1x  (Lv{lvl})"
        ));

        RegisterInternal(new StatUpgradeDefinition(
            id: "speed",
            displayName: "Walk Speed",
            maxLevel: 5,
            apply: (newLevel, stats) =>
            {
                stats.SetWalkSpeed(stats.walkSpeed + 0.75f);
            },
            describe: lvl => $"Walk Speed +0.75  (Lv{lvl})"
        ));

        RegisterInternal(new SubWeaponUpgradeDefinition(
            id: "flyingblade",
            displayName: "Flying Blade",
            maxLevel: 5,
            factory: upgrades =>
            {
                GameObject host = new GameObject("FlyingBlade");
                host.transform.SetParent(upgrades.transform, false);
                host.transform.localPosition = Vector3.zero;
                return host.AddComponent<FlyingBlade>();
            },
            applyLevel: (sw, level, stats) => sw.SetLevel(level)
        ));
    }

    private static void RegisterInternal(UpgradeDefinition def)
    {
        registry[def.Id] = def;
        ordered.Add(def);
    }
}
