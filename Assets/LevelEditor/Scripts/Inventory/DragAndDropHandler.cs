using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DragAndDropHandler : MonoBehaviour
{
    public GameObject prefab; // Prefab to spawn
    private GameObject spawnedObject; // The object currently being dragged
    private XRRayInteractor rayInteractor; // The ray interactor from the controller

    public InputActionReference leftGrabAction; // Input action for left controller grab
    public InputActionReference rightGrabAction; // Input action for right controller grab

    public GameObject inventory; // This should be referenced in the scene

    void Start()
    {
        // Find the XRRayInteractor in the scene
        rayInteractor = FindObjectOfType<XRRayInteractor>();

        if (rayInteractor == null)
        {
            Debug.LogError("No XRRayInteractor found in the scene. Ensure your controller has one.");
        }

        // Find the Inventory in the scene if it's not already assigned
        if (inventory == null)
        {
            inventory = GameObject.Find("Inventory"); // Adjust the name to your Inventory GameObject
            if (inventory == null)
            {
                Debug.LogError("No Inventory found in the scene. Please assign an inventory.");
            }
        }
    }

    void Update()
    {
        if (rayInteractor == null || prefab == null)
            return;

        bool leftGripPressed = leftGrabAction != null && leftGrabAction.action != null && leftGrabAction.action.triggered;
        bool rightGripPressed = rightGrabAction != null && rightGrabAction.action != null && rightGrabAction.action.triggered;

        if ((leftGripPressed || rightGripPressed) && IsRayHoveringOverSlot())
        {
            if (spawnedObject == null)
            {
                SpawnObjectAtRay();
                DeactivateInventory(); // Deactivate the inventory when grabbing
            }
        }

        if (spawnedObject != null && !(leftGripPressed || rightGripPressed))
        {
            ReleaseObject();
            ActivateInventory(); // Activate the inventory when releasing
        }
    }

    private bool IsRayHoveringOverSlot()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.transform == transform) // Check if this slot is being hovered
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnObjectAtRay()
    {
        Transform attachTransform = rayInteractor.attachTransform;
        spawnedObject = Instantiate(prefab, attachTransform.position, attachTransform.rotation);
        spawnedObject.transform.SetParent(attachTransform);
    }

    private void ReleaseObject()
    {
        spawnedObject.transform.SetParent(null);
        spawnedObject = null;
    }

    private void DeactivateInventory()
    {
        if (inventory != null)
        {
            inventory.SetActive(false);
        }
    }

    private void ActivateInventory()
    {
        if (inventory != null)
        {
            inventory.SetActive(true);
        }
    }
}
