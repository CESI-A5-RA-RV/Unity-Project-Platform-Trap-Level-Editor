using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWall : MonoBehaviour
{
    public Vector3 movementDirection = Vector3.right; // Direction to move (e.g., right)
    public float speed = 2.0f;                        // Speed of movement
    public float range = 5.0f;                        // Total distance to move

    private Vector3 startingPosition;                 // Initial position of the wall
    private bool movingForward = true;                // Movement state
    private Rigidbody rb;                             // Rigidbody for physics interaction

    void Start()
    {
        startingPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning("No Rigidbody found on the moving platform. Add one for better physics handling.");
        }
    }

    void Update()
    {
        Debug.DrawLine(startingPosition, startingPosition + movementDirection * range, Color.red);

        float distanceFromStart = (transform.position - startingPosition).magnitude;

        if (distanceFromStart >= range)
        {
            movingForward = !movingForward;
        }

        Vector3 direction = movingForward ? movementDirection : -movementDirection;

        if (rb != null)
        {
            rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }
}