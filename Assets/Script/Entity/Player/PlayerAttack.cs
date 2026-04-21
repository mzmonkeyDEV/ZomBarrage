using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerAttack : Entity
{
    [Header("Attack Setup")]
    public GameObject projectilePrefab;
    [Tooltip("Create an empty GameObject as a child of the player, place it in front of the player, and drag it here.")]
    public Transform firePoint;

    [Header("Targeting Settings")]
    public float attackRange = 10f;

    // This is a public method so our UI Button can call it directly
    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f;
    public float flashInterval = 0.1f; // Speed of the flickering
    private bool isInvincible = false;
    private bool isDead = false;

    [Header("UI Feedback")]
    public GameObject damagePanel; // Drag your UI Panel here in the Inspector
    public float uiFlashDuration = 0.1f;


    // Overriding TakeDamage to include invincibility logic
    public override void TakeDamage(float damage, Transform attacker)
    {
        if (isDead) return;
        Renderer rendi = GetComponentInChildren<Renderer>();
        Color originalColor = rendi.material.color;
        // 1. If currently invincible, ignore all damage
        if (isInvincible) return;

        // 2. Apply base damage and knockback from Entity script
        base.TakeDamage(damage, attacker);
        GameManager.Instance.UpdateUI();

        // 3. Start the invincibility period
        if (currentHp > 0) // Only if still alive
        {
            StartCoroutine(InvincibilityRoutine(originalColor));
            StartCoroutine(TogglePanelRoutine());
        }
    }
    private IEnumerator TogglePanelRoutine()
    {
        damagePanel.SetActive(true);

        // Wait for 0.1 seconds
        yield return new WaitForSeconds(0.1f);

        damagePanel.SetActive(false);
    }
    protected override void Die()
    {
        isDead = true;
        currentHp = 0;
        GameManager.Instance.UpdateUI();

        // Tell the GameManager to show the Game Over screen
        GameManager.Instance.TriggerGameOver();

        // Optional: Play death animation or disable player movement here
    }

    private IEnumerator InvincibilityRoutine(Color ori)
    {
        isInvincible = true;

        float elapsed = 0;
        Renderer rend = GetComponentInChildren<Renderer>();
        

        while (elapsed < invincibilityDuration)
        {
            // Toggle between White and Original Color
            rend.material.color = (rend.material.color == Color.white) ? ori : Color.white;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        // Ensure we reset to the original color at the end
        rend.material.color = ori;
        isInvincible = false;
    }
    public void FireAtNearestEnemy()
    {
        Enemy target = GetNearestEnemy();

        if (target != null)
        {
            // 1. Calculate direction to the enemy
            Vector3 directionToEnemy = target.transform.position - transform.position;

            // 2. Ignore vertical difference so the player doesn't tilt up/down
            directionToEnemy.y = 0f;

            // 3. Instantly snap the player to face the enemy
            transform.rotation = Quaternion.LookRotation(directionToEnemy);

            // 4. Spawn the projectile at the fire point. 
            // It uses firePoint.rotation, which is now facing the enemy!
            GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (bullet.TryGetComponent(out Projectile p))
            {
                p.damage = Mathf.RoundToInt(p.damage * mightMultiplier);
                
            }
        }
        else
        {
            Debug.Log("No enemies in range!");
        }
    }

    private Enemy GetNearestEnemy()
    {
        Enemy closestEnemy = null;
        float shortestDistance = attackRange; // Start with max range

        // Loop through our highly efficient static list
        foreach (Enemy enemy in Enemy.ActiveEnemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        return closestEnemy; // Returns null if no enemy is within attackRange
    }

    // Optional: Draw a circle in the editor so you can see your attack range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}