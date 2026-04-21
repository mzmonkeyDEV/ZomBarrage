using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;
    public float lifeTime = 3f;

    private void Start()
    {
        // Automatically destroy the projectile after a few seconds so it doesn't fly forever
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Move forward constantly. The player script will ensure 'forward' is facing the enemy.
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we hit has an Enemy component
        if (other.TryGetComponent(out Enemy hitEnemy))
        {
            hitEnemy.TakeDamage(damage, transform);
            Destroy(gameObject); // Destroy the projectile upon hit
        }
    }
}