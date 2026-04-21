using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : Entity
{
    public enum TargetingMode
    {
        Nearest,
        LowestHP,
        HighestHP
    }

    [Header("Attack Setup")]
    public GameObject projectilePrefab;
    [Tooltip("Create an empty GameObject as a child of the player, place it in front of the player, and drag it here.")]
    public Transform firePoint;

    [Header("Targeting Settings")]
    public float attackRange = 10f;
    public TargetingMode targetingMode = TargetingMode.Nearest;

    [Header("Optional Weapon Data")]
    [Tooltip("If set, overrides damage/range from this WeaponData at the given level index.")]
    public WeaponData weaponData;
    public int weaponLevelIndex = 0;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f;
    public float flashInterval = 0.1f;
    private bool isInvincible = false;
    private bool isDead = false;

    [Header("UI Feedback")]
    public GameObject damagePanel;
    public float uiFlashDuration = 0.1f;

    public override void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return;

        Renderer rendi = GetComponentInChildren<Renderer>();
        Color originalColor = rendi.material.color;

        if (isInvincible) return;

        base.TakeDamage(damage, attacker);
        GameManager.Instance.UpdateUI();

        if (currentHp > 0)
        {
            StartCoroutine(InvincibilityRoutine(originalColor));
            StartCoroutine(TogglePanelRoutine());
        }
    }

    private IEnumerator TogglePanelRoutine()
    {
        damagePanel.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        damagePanel.SetActive(false);
    }

    protected override void Die()
    {
        isDead = true;
        currentHp = 0;
        GameManager.Instance.UpdateUI();
        GameManager.Instance.TriggerGameOver();
    }

    private IEnumerator InvincibilityRoutine(Color ori)
    {
        isInvincible = true;
        float elapsed = 0;
        Renderer rend = GetComponentInChildren<Renderer>();

        while (elapsed < invincibilityDuration)
        {
            rend.material.color = (rend.material.color == Color.white) ? ori : Color.white;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        rend.material.color = ori;
        isInvincible = false;
    }

    public void FireAtNearestEnemy()
    {
        float effectiveRange = GetEffectiveRange();
        Enemy target = SelectTarget(targetingMode, effectiveRange);

        if (target != null)
        {
            Vector3 directionToEnemy = target.transform.position - transform.position;
            directionToEnemy.y = 0f;
            transform.rotation = Quaternion.LookRotation(directionToEnemy);

            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (bullet.TryGetComponent(out Projectile p))
            {
                float baseDamage = GetEffectiveDamage(p.damage);
                p.damage = Mathf.RoundToInt(baseDamage * mightMultiplier);
            }
        }
        else
        {
            Debug.Log("No enemies in range!");
        }
    }

    public float GetEffectiveRange()
    {
        WeaponData.WeaponLevel? level = GetCurrentWeaponLevel();
        if (level.HasValue && level.Value.range > 0f) return level.Value.range;
        return attackRange;
    }

    public float GetEffectiveDamage(float fallback)
    {
        WeaponData.WeaponLevel? level = GetCurrentWeaponLevel();
        if (level.HasValue && level.Value.damage > 0f) return level.Value.damage;
        return fallback;
    }

    private WeaponData.WeaponLevel? GetCurrentWeaponLevel()
    {
        if (weaponData == null || weaponData.levels == null || weaponData.levels.Count == 0) return null;
        int idx = Mathf.Clamp(weaponLevelIndex, 0, weaponData.levels.Count - 1);
        return weaponData.levels[idx];
    }

    public Enemy SelectTarget(TargetingMode mode, float range)
    {
        if (Enemy.ActiveEnemies.Count == 0) return null;

        List<Enemy> candidates = new List<Enemy>();
        Vector3 myPos = transform.position;

        for (int i = 0; i < Enemy.ActiveEnemies.Count; i++)
        {
            Enemy e = Enemy.ActiveEnemies[i];
            if (e == null) continue;
            if (Vector3.Distance(myPos, e.transform.position) <= range)
            {
                candidates.Add(e);
            }
        }
        if (candidates.Count == 0) return null;

        switch (mode)
        {
            case TargetingMode.Nearest:
                candidates.Sort((a, b) =>
                    Vector3.SqrMagnitude(a.transform.position - myPos)
                        .CompareTo(Vector3.SqrMagnitude(b.transform.position - myPos)));
                break;
            case TargetingMode.LowestHP:
                candidates.Sort((a, b) => a.currentHp.CompareTo(b.currentHp));
                break;
            case TargetingMode.HighestHP:
                candidates.Sort((a, b) => b.currentHp.CompareTo(a.currentHp));
                break;
        }

        return candidates[0];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
