using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    [Header("Core Stats")]
    public float maxHp = 100;
    public float currentHp;
    public float moveSpeed = 5f;

    [Header("Damage Effects")]
    public float knockbackForce = 10f; // 3D usually requires higher force values than 2D
    public float flashDuration = 0.15f;

    private Renderer meshRenderer; // Use Renderer for 3D meshes
    private Rigidbody rb;          // 3D Rigidbody
    private Color originalColor;

    [Header("Scalable Multipliers")]
    public float mightMultiplier = 1f;
    public float areaMultiplier = 1f;
    public float pickupRange = 3f;

    protected virtual void Awake()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody>();

        // Try to get the renderer from this object or its children
        meshRenderer = GetComponentInChildren<Renderer>();

        if (meshRenderer != null)
        {
            // Use .material.color for standard shaders
            originalColor = meshRenderer.material.color;
        }
    }

    public virtual void TakeDamage(float damage, Transform attacker)
    {
        currentHp -= damage;

        // 1. Visual Flash
        StopCoroutine(nameof(FlashRed));
        StartCoroutine(FlashRed());

        // 2. 3D Horizontal Knockback
        if (rb != null && attacker != null)
        {
            Apply3DKnockback(attacker);
        }

        if (currentHp <= 0) Die();
    }

    private void Apply3DKnockback(Transform attacker)
    {
        // Calculate direction from attacker to this entity
        Vector3 direction = (transform.position - attacker.position);

        // Flatten the direction so there is no vertical (Y) force
        direction.y = 0;
        direction = direction.normalized;

        // Reset velocity to ensure the hit feels snappy
        rb.linearVelocity = Vector3.zero;

        // Apply the force
        rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
    }

    private IEnumerator FlashRed()
    {
        if (meshRenderer == null) yield break;

        meshRenderer.material.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        meshRenderer.material.color = originalColor;
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }
}