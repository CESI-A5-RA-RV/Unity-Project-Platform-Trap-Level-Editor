using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanObstacle : MonoBehaviour
{
    [SerializeField] private float pushForce = 100f; // Force applied to players
    // [SerializeField]
    // private ParticleSystem windEffect; // Assign the wind particle system in the Inspector
    [SerializeField] private Transform windDirection; // Transform indicating the direction of the wind

    private void OnTriggerStay(Collider other)
    {
        // Check if the object entering the fan's range is a player
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Apply force in the direction of the wind
                Vector3 pushDirection = windDirection.forward;

                // Gradual force application
                playerRb.AddForce(pushDirection * pushForce, ForceMode.Force);

                // Floating effect: reduce vertical velocity slightly when at the top of the wind zone
                if (Mathf.Abs(playerRb.velocity.y) < 0.5f)
                {
                    playerRb.AddForce(Vector3.down * pushForce * 0.3f, ForceMode.Force); // Gentle downward force
                }

                // Restrict horizontal movement for difficulty
                Vector3 playerVelocity = playerRb.velocity;
                playerVelocity.x *= 0.9f;
                playerVelocity.z *= 0.9f;
                playerRb.velocity = playerVelocity;
            }
        }
    }


    private void Start()
    {
        // Adjust particle system based on wind force
        // if (windEffect != null)
        // {
            // windEffect.Play();
            // AdjustParticleEffect(windEffect, pushForce);
        // }
    }

    // Adjusts the length of the particle effect
    private void AdjustParticleEffect(ParticleSystem particleSystem, float force)
    {
        // Calculate the lifetime based on force and particle speed
        float particleSpeed = particleSystem.main.startSpeed.constant; // Assuming constant speed

        // Adjust lifetime to match wind distance
        float newLifetime = force / particleSpeed;

        var mainModule = particleSystem.main;
        mainModule.startLifetime = newLifetime;

        // Optionally adjust emission rate based on force
        // var emission = particleSystem.emission;
        // emission.rateOverTime = force * 4f;
    }
}