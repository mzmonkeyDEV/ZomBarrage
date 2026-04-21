using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int xpAmount = 20;
    public float flySpeed = 2f;
    public float acceleration = 1.05f;

    private Transform targetPlayer;
    private bool isFlying = false;

    void Update()
    {
        if (isFlying && targetPlayer != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, flySpeed * Time.deltaTime);
            flySpeed *= acceleration; // Zips faster as it gets closer
        }
        else
        {
            CheckDistance();
        }
    }

    void CheckDistance()
    {
        // Finding the player via Tag is common for prototypes
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            float range = player.GetComponent<Entity>().pickupRange;

            if (dist <= range)
            {
                targetPlayer = player.transform;
                isFlying = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out TopDownPlayerController p))
            {
                GameManager.Instance.AddXP(xpAmount);
                Destroy(gameObject);
            }
        }
    }
}