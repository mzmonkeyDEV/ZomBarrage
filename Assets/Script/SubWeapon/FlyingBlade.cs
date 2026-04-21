using System.Collections.Generic;
using UnityEngine;

public class FlyingBlade : SubWeapon
{
    [Header("Visual")]
    [Tooltip("Optional. If null, a primitive cube is generated at runtime.")]
    public GameObject bladePrefab;
    public Vector3 bladeScale = new Vector3(0.4f, 0.4f, 0.8f);

    [Header("Orbit")]
    public float orbitRadius = 2f;
    public float baseOrbitDegPerSec = 60f;
    [Tooltip("Contribution from the player's walk speed to the blade's orbit degrees-per-second.")]
    public float walkSpeedCoefficient = 35f;

    [Header("Damage")]
    public int baseDamage = 5;
    public int damagePerLevel = 3;
    public float hitCooldownPerEnemy = 0.5f;

    private readonly List<Transform> bladeTransforms = new List<Transform>();
    private readonly List<BladeHitbox> bladeHitboxes = new List<BladeHitbox>();
    private float currentAngle = 0f;

    public override void SetLevel(int level)
    {
        Level = level;
        int desired = GetBladeCount(level);
        while (bladeTransforms.Count < desired) SpawnBlade();
        RefreshBladeDamage();
    }

    private static int GetBladeCount(int level)
    {
        if (level >= 5) return 3;
        if (level >= 3) return 2;
        return 1;
    }

    public int CurrentDamage => baseDamage + damagePerLevel * Mathf.Max(0, Level - 1);

    private void RefreshBladeDamage()
    {
        int dmg = CurrentDamage;
        for (int i = 0; i < bladeHitboxes.Count; i++)
        {
            if (bladeHitboxes[i] != null) bladeHitboxes[i].damage = dmg;
        }
    }

    private void SpawnBlade()
    {
        GameObject blade;
        if (bladePrefab != null)
        {
            blade = Instantiate(bladePrefab, transform);
        }
        else
        {
            blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Blade";
            blade.transform.SetParent(transform, false);
            blade.transform.localScale = bladeScale;
        }

        Collider col = blade.GetComponent<Collider>();
        if (col == null) col = blade.AddComponent<BoxCollider>();
        col.isTrigger = true;

        Rigidbody rb = blade.GetComponent<Rigidbody>();
        if (rb == null) rb = blade.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        BladeHitbox hitbox = blade.GetComponent<BladeHitbox>();
        if (hitbox == null) hitbox = blade.AddComponent<BladeHitbox>();
        hitbox.damage = CurrentDamage;
        hitbox.hitCooldown = hitCooldownPerEnemy;

        bladeTransforms.Add(blade.transform);
        bladeHitboxes.Add(hitbox);
    }

    void Update()
    {
        if (bladeTransforms.Count == 0) return;

        float walkSpeed = playerStats != null ? playerStats.walkSpeed : 0f;
        float orbitDegPerSec = baseOrbitDegPerSec + walkSpeed * walkSpeedCoefficient;
        currentAngle = (currentAngle + orbitDegPerSec * Time.deltaTime) % 360f;

        Vector3 center = transform.position;
        int count = bladeTransforms.Count;
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            Transform bt = bladeTransforms[i];
            if (bt == null) continue;
            float angleDeg = currentAngle + step * i;
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * orbitRadius;
            bt.position = center + offset;
            if (offset.sqrMagnitude > 0.0001f)
            {
                bt.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
            }
        }
    }
}

public class BladeHitbox : MonoBehaviour
{
    [HideInInspector] public int damage = 1;
    [HideInInspector] public float hitCooldown = 0.5f;

    private readonly Dictionary<Enemy, float> lastHitAt = new Dictionary<Enemy, float>();

    private void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out Enemy enemy)) return;
        if (lastHitAt.TryGetValue(enemy, out float last) && Time.time - last < hitCooldown) return;
        lastHitAt[enemy] = Time.time;
        enemy.TakeDamage(damage, transform);
    }
}
