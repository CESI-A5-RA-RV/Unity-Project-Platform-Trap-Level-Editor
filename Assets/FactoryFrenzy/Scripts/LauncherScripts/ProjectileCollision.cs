using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    [SerializeField] float knockbackForce = 30f; // Total knockback force
    [SerializeField] float maxVerticalKnockback = 5f; // Maximum allowed vertical force

    private Rigidbody projectileRb; // To track the projectile's velocity

    private void Awake()
    {
        projectileRb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Rigidbody playerRb = collision.collider.GetComponent<Rigidbody>();

            if (playerRb != null)
            {
                // Use the projectile's velocity as the base knockback direction
                Vector3 knockbackDirection = projectileRb.velocity.normalized;

                // Optional: Clamp the vertical component of the knockback direction
                knockbackDirection.y = Mathf.Clamp(knockbackDirection.y, 0f, maxVerticalKnockback / knockbackForce);

                // Apply adjusted knockback force
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

                // Destroy the projectile
                Destroy(gameObject);
            }
        }
    }
}