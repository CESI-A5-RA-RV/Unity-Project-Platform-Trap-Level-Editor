using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moving_wall : MonoBehaviour
{

    public Vector3 movementDirection = Vector3.right; // Direction to move (e.g., right)
    public float speed = 2.0f;                        // Speed of movement
    public float range = 5.0f;                        // Total distance to move

    private Vector3 startingPosition;                 // Initial position of the wall
    private bool movingForward = true;                // Movement state
    
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromStart = Vector3.Distance(transform.position, startingPosition);

        if (distanceFromStart >= range)               // Reverse direction if out of range
        {
            movingForward = !movingForward;
        }

        // Determine movement direction
        Vector3 direction = movingForward ? movementDirection : -movementDirection;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}
