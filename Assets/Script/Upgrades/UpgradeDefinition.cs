using System;

public abstract class UpgradeDefinition
{
    public enum Category { Stat, SubWeapon, MainWeapon }

    public string Id { get; }
    public string DisplayName { get; }
    public int MaxLevel { get; }
    public Category Kind { get; }

    protected UpgradeDefinition(string id, string displayName, int maxLevel, Category kind)
    {
        Id = id;
        DisplayName = displayName;
        MaxLevel = maxLevel;
        Kind = kind;
    }

    public abstract void ApplyAtLevel(int newLevel, PlayerStats stats, PlayerUpgrades upgrades);
    public abstract string DescribeNext(int currentLevel);
}

public class StatUpgradeDefinition : UpgradeDefinition
{
    private readonly Action<int, PlayerStats> apply;
    private readonly Func<int, string> describe;

    public StatUpgradeDefinition(
        string id,
        string displayName,
        int maxLevel,
        Action<int, PlayerStats> apply,
        Func<int, string> describe)
        : base(id, displayName, maxLevel, Category.Stat)
    {
        this.apply = apply;
        this.describe = describe;
    }

    public override void ApplyAtLevel(int newLevel, PlayerStats stats, PlayerUpgrades upgrades)
    {
        apply?.Invoke(newLevel, stats);
    }

    public override string DescribeNext(int currentLevel)
    {
        return describe != null ? describe(currentLevel + 1) : $"{DisplayName} Lv{currentLevel + 1}";
    }
}

public class SubWeaponUpgradeDefinition : UpgradeDefinition
{
    private readonly Func<PlayerUpgrades, SubWeapon> factory;
    private readonly Action<SubWeapon, int, PlayerStats> applyLevel;

    public SubWeaponUpgradeDefinition(
        string id,
        string displayName,
        int maxLevel,
        Func<PlayerUpgrades, SubWeapon> factory,
        Action<SubWeapon, int, PlayerStats> applyLevel)
        : base(id, displayName, maxLevel, Category.SubWeapon)
    {
        this.factory = factory;
        this.applyLevel = applyLevel;
    }

    public override void ApplyAtLevel(int newLevel, PlayerStats stats, PlayerUpgrades upgrades)
    {
        SubWeapon sw = upgrades.GetSubWeapon(Id);
        if (sw == null)
        {
            sw = factory(upgrades);
            upgrades.RegisterSubWeapon(Id, sw);
        }
        applyLevel?.Invoke(sw, newLevel, stats);
    }

    public override string DescribeNext(int currentLevel)
    {
        return currentLevel == 0 ? $"Unlock {DisplayName}" : $"{DisplayName} Lv{currentLevel + 1}";
    }
}
