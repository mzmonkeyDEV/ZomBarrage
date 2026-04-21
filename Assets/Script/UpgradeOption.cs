using System;
using System.Collections.Generic;

public class UpgradeOption
{
    public string Id { get; }
    public string DisplayName { get; }
    public Action<PlayerAttack> Apply { get; }

    public UpgradeOption(string id, string displayName, Action<PlayerAttack> apply)
    {
        Id = id;
        DisplayName = displayName;
        Apply = apply;
    }
}

public static class UpgradeRegistry
{
    private static Dictionary<string, UpgradeOption> options;

    public static Dictionary<string, UpgradeOption> Options
    {
        get
        {
            if (options == null) Initialize();
            return options;
        }
    }

    private static void Initialize()
    {
        options = new Dictionary<string, UpgradeOption>
        {
            ["health"] = new UpgradeOption("health", "+20 Max HP", player =>
            {
                player.maxHp += 20;
                player.currentHp += 20;
            }),
            ["might"] = new UpgradeOption("might", "+0.1x Damage", player =>
            {
                player.mightMultiplier += 0.1f;
            }),
        };
    }

    public static UpgradeOption Get(string id)
    {
        return Options.TryGetValue(id, out UpgradeOption option) ? option : null;
    }
}
