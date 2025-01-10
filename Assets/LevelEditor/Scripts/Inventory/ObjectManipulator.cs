using UnityEngine;

public class ObjectManipulator : MonoBehaviour
{
    private float rotationSpeed = 50f;
    private float scaleSpeed = 0.5f;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        HandleRotation();
        HandleScaling();
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandleScaling()
    {
        if (Input.GetKey(KeyCode.R))
        {
            transform.localScale += Vector3.one * scaleSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.F))
        {
            transform.localScale -= Vector3.one * scaleSpeed * Time.deltaTime;
            if (transform.localScale.magnitude < originalScale.magnitude * 0.1f)
            {
                transform.localScale = originalScale * 0.1f; // Prevent too small scale
            }
        }
    }

    public void EnableManipulation()
    {
        Debug.Log($"{gameObject.name} is now manipulatable.");
    }
}
