using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DragAndDropHandler : MonoBehaviour
{
    public GameObject prefab; // Prefab to spawn
    private GameObject spawnedObject; // The object currently being dragged
    public XRRayInteractor rayInteractor; // The ray interactor from the controller

    public InputActionReference rightGrabAction; // Input action for right controller grab

    public GameObject inventory; // This should be referenced in the scene

    void Start()
    {
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

        if (rightGrabAction != null && rightGrabAction.action != null)
        {
            rightGrabAction.action.performed += OnGrabPerformed;
            rightGrabAction.action.canceled += OnGrabCanceled;
        }
        else
        {
            Debug.LogError("Right Grab Action is not set correctly.");
        }
    }

    private void OnDestroy()
    {
        if (rightGrabAction != null && rightGrabAction.action != null)
        {
            rightGrabAction.action.performed -= OnGrabPerformed;
            rightGrabAction.action.canceled -= OnGrabCanceled;
        }
    }

    void Update()
    {
        if (rayInteractor == null || prefab == null)
            return;
    }

    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        if (IsRayHoveringOverSlot())
        {
            if (spawnedObject == null)
            {
                Debug.Log("Spawning object at ray...");
                SpawnObjectAtRay();
                DeactivateInventory(); // Deactivate the inventory when grabbing
            }
        }
    }

    private void OnGrabCanceled(InputAction.CallbackContext context)
    {
        if (spawnedObject != null)
        {
            ReleaseObject();
            ActivateInventory(); // Activate the inventory when releasing
        }
    }

    private bool IsRayHoveringOverSlot()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.transform == transform)
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnObjectAtRay()
    {
        // Get the attachTransform of the ray interactor
        Transform attachTransform = rayInteractor.attachTransform;

        // Instantiate the prefab at the interactor's position and rotation
        spawnedObject = Instantiate(prefab, attachTransform.position, attachTransform.rotation);
        Debug.Log("Spawned object: " + spawnedObject.name);

        // Ensure the spawned object has an XRGrabInteractable component
        XRGrabInteractable grabInteractable = spawnedObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = spawnedObject.AddComponent<XRGrabInteractable>();
        }

        // Manually initiate the grab interaction using the Interaction Manager
        XRInteractionManager interactionManager = rayInteractor.interactionManager;
        if (interactionManager != null)
        {
            interactionManager.SelectEnter(rayInteractor as IXRSelectInteractor, grabInteractable as IXRSelectInteractable);
            Debug.Log("Object successfully grabbed.");
        }
        else
        {
            Debug.LogWarning("Interaction Manager is missing or not assigned to the Ray Interactor.");
        }
    }

    private void ReleaseObject()
    {
        Debug.Log("Releasing object...");
        if (spawnedObject != null)
        {
            spawnedObject.transform.SetParent(null);
            Debug.Log("Object released");
            spawnedObject = null;
        }
    }

    private void DeactivateInventory()
    {
        if (inventory != null)
        {
            Debug.Log("Deactivating inventory...");
            inventory.SetActive(false);
        }
    }

    private void ActivateInventory()
    {
        if (inventory != null)
        {
            Debug.Log("Activating inventory...");
            inventory.SetActive(true);
        }
    }
}
