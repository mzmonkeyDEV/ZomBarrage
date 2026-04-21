using System.Collections.Generic;
using UnityEngine;

public class Enemy : Entity
{
    public static List<Enemy> ActiveEnemies = new List<Enemy>();

    [Header("Movement")]
    private Transform player;
    private Rigidbody enemyRb;

    [Header("Knockback Settings")]
    public float knockbackDuration = 0.2f;
    private float knockbackTimer = 0f; // Ensure it starts at 0

    [Header("Combat & Drops")]
    public float damage = 10f;
    public float attackCooldown = 1f;
    private float lastAttackTime;
    public GameObject xpGemPrefab;
    public int xpValue = 20;

    protected override void Awake()
    {
        base.Awake();
        enemyRb = GetComponent<Rigidbody>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    private void OnEnable() => ActiveEnemies.Add(this);
    private void OnDisable() => ActiveEnemies.Remove(this);

    public override void TakeDamage(float damage, Transform attacker)
    {
        base.TakeDamage(damage, attacker);
        // Start the timer to pause movement
        knockbackTimer = knockbackDuration;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            // If the timer JUST finished this frame:
            if (knockbackTimer <= 0)
            {
                // Kill the leftover knockback velocity so it doesn't fight our movement
                enemyRb.linearVelocity = Vector3.zero;
                enemyRb.angularVelocity = Vector3.zero;
            }
            return;
        }

        MoveTowardPlayer();
    }

    private void MoveTowardPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // Use MovePosition for smooth physics-based movement
        Vector3 targetPosition = enemyRb.position + direction * moveSpeed * Time.fixedDeltaTime;
        enemyRb.MovePosition(targetPosition);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            enemyRb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.fixedDeltaTime));
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (collision.gameObject.TryGetComponent(out PlayerAttack playerScript))
                {
                    playerScript.TakeDamage(damage, transform);
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    protected override void Die()
    {
        ActiveEnemies.Remove(this);
        if (xpGemPrefab != null)
        {
            GameObject gem = Instantiate(xpGemPrefab, transform.position, Quaternion.identity);
            if (gem.TryGetComponent(out XPGem gemScript)) gemScript.xpAmount = xpValue;
        }
        base.Die();
    }
}