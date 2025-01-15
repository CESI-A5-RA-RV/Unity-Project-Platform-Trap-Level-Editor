using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;

public class BuildingZoneManager : MonoBehaviour
{
    public float gridSize = 1.0f; // Size of the grid for snapping
    public Transform gridCenter; // Center of the grid (empty GameObject)
    public float rotationSnapAngle = 90.0f; // Angle increment for rotation snapping

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object is a valid platform or trap
        if (other.CompareTag("Platform") || other.CompareTag("Trap"))
        {
            // Add the event listener to snap on drop
            var grabInteractable = other.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.selectExited.AddListener(OnDrop);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Platform") || other.CompareTag("Trap"))
        {
            // Remove the snap listener when exiting the zone
            var grabInteractable = other.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.selectExited.RemoveListener(OnDrop);
            }
        }
    }

    private void OnDrop(SelectExitEventArgs args)
    {
        // Get the object being dropped
        var droppedObject = args.interactableObject.transform;

        // Snap the position and rotation to the grid
        droppedObject.position = CalculateClosestSnapPoint(droppedObject.position);
        droppedObject.rotation = CalculateSnappedRotation(droppedObject.rotation.eulerAngles);

        // Set the object as a child of the building zone
        droppedObject.SetParent(transform, true);
    }

    private Vector3 CalculateClosestSnapPoint(Vector3 position)
    {
        Vector3 gridPosition = gridCenter.position;

        float snapX = Mathf.Round((position.x - gridPosition.x) / gridSize) * gridSize + gridPosition.x;
        float snapY = Mathf.Round((position.y - gridPosition.y) / gridSize) * gridSize + gridPosition.y;
        float snapZ = Mathf.Round((position.z - gridPosition.z) / gridSize) * gridSize + gridPosition.z;

        return new Vector3(snapX, snapY, snapZ);
    }

    private Quaternion CalculateSnappedRotation(Vector3 currentRotation)
    {
        // Snap each rotation axis to the nearest multiple of the snap angle
        float snappedX = Mathf.Round(currentRotation.x / rotationSnapAngle) * rotationSnapAngle;
        float snappedY = Mathf.Round(currentRotation.y / rotationSnapAngle) * rotationSnapAngle;
        float snappedZ = Mathf.Round(currentRotation.z / rotationSnapAngle) * rotationSnapAngle;

        // Return the snapped rotation as a Quaternion
        return Quaternion.Euler(snappedX, snappedY, snappedZ);
    }
}
