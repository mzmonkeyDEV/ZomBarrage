using UnityEngine;

public abstract class SubWeapon : MonoBehaviour
{
    protected PlayerStats playerStats;
    public int Level { get; protected set; }

    public virtual void Bind(PlayerStats stats)
    {
        playerStats = stats;
    }

    public abstract void SetLevel(int level);
}
