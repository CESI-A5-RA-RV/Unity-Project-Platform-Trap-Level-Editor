using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
    public float detectionRadius = 10f;
    public float rotationSpeed = 5f;
    public float shootingForce = 1000f;
    public Transform shootingAxis;

    private Transform targetPlayer = null;
    private Vector3 startRotation;
    private Vector3 endRotation;
    private bool isShooting = false;

    [SerializeField] GameObject projectile;
    [SerializeField] float projectileSpeed = 20f;
    [SerializeField] float cooldownTime = 2f;
    [SerializeField] ParticleSystem shootParticles;

    private float rotationDirection = 1f; // For back-and-forth rotation
    private float lastShotTime;

    // Start is called before the first frame update
    void Start()
    {
        startRotation = transform.eulerAngles - new Vector3(0, 90, 0);
        endRotation = transform.eulerAngles + new Vector3(0, 90, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isShooting) return;

        // Detect objects with tag "Player" in the detection zone
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectionRadius, LayerMask.GetMask("Player"));

        if (playersInRange.Length == 0)
        {
            // No players in range, enter search state
            targetPlayer = null;
            SearchState();
        }
        else
        {
            // Get the closest player
            targetPlayer = GetClosestPlayer(playersInRange);
            OrientationState();
            ShootingState();
        }
    }

    void SearchState()
    {
        // Rotate back and forth between startRotation and endRotation
        float step = rotationSpeed * Time.deltaTime * rotationDirection;
        transform.Rotate(0, step, 0);

        // Reverse direction if rotation exceeds bounds
        float currentYRotation = transform.eulerAngles.y;
        if (currentYRotation > endRotation.y || currentYRotation < startRotation.y)
        {
            rotationDirection *= -1f;
        }
    }

    void OrientationState()
    {
        // Rotate towards the target player
        Vector3 directionToPlayer = (targetPlayer.position - transform.position).normalized;
        directionToPlayer.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void ShootingState()
    {
        if (!isShooting && targetPlayer != null && Time.time - lastShotTime >= cooldownTime)
        {
            // Calculate the direction to the player
            Vector3 directionToPlayer = targetPlayer.position - transform.position;
            directionToPlayer.y = 0; // Ignore vertical difference for alignment
            directionToPlayer.Normalize();

            // Check the alignment between the cannon's forward direction and the direction to the player
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle > 10f) // Allow firing only if the angle is less than 10 degrees
            {
                return; // Exit without shooting
            }

            isShooting = true;

            // Spawn the projectile
            GameObject spawnedProjectile = Instantiate(projectile, shootingAxis.position, Quaternion.identity);
            Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 shootDirection = shootingAxis.forward; // Use the forward direction of the shootingAxis
                rb.AddForce(shootDirection * projectileSpeed, ForceMode.Impulse);
            }

            // Play particle effect when shooting
            if (shootParticles != null)
            {
                shootParticles.Play();
            }

            lastShotTime = Time.time;

            ResetShooting();

            // Destroy the projectile after a certain time to prevent lingering in the game world
            Destroy(spawnedProjectile, detectionRadius / projectileSpeed);
        }
    }

    void ResetShooting()
    {
        isShooting = false;
    }

    Transform GetClosestPlayer(Collider[] players)
    {
        Transform closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player.transform;
            }
        }

        return closestPlayer;
    }

    public void InitializeLauncher(float detectionRadius, float rotationSpeed, float shootingForce)
    {
        this.detectionRadius = detectionRadius;
        this.rotationDirection = rotationSpeed;
        this.shootingForce = shootingForce;

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
