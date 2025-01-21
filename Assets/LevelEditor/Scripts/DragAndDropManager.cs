using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class DragAndDropManager : MonoBehaviour
{
    public XRRayInteractor rayInteractor; // Ray interactor for detecting inventory slots
    public XRRayInteractor inventoryRayInteractor;
    public InputActionReference rightGrabAction; // Input action for grab
    public GameObject inventory; // Inventory GameObject

    private GameObject currentHoveredSlot; // The currently hovered inventory slot
    private GameObject spawnedObject; // The spawned prefab
    private bool isHovering = false;

    void Start()
    {
        if (rayInteractor == null)
        {
            Debug.LogError("RayInteractor is not assigned!");
            return;
        }

        if (rightGrabAction != null && rightGrabAction.action != null)
        {
            rightGrabAction.action.performed += OnGrabPerformed;
            rightGrabAction.action.canceled += OnGrabCanceled;
        }
        else
        {
            Debug.LogError("Right Grab Action is not set.");
        }
        inventoryRayInteractor.enabled = true;
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
        // Check if the ray is hovering over an inventory slot
        isHovering = CheckRayHover();
    }

    private bool CheckRayHover()
    {
        currentHoveredSlot = null;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.transform.CompareTag("UIElement"))
            {
                currentHoveredSlot = hit.transform.gameObject;
                Debug.Log("Hovering over slot: " + currentHoveredSlot.GetComponent<DragAndDropHandler>().prefab.name);
                return true;
            }
        }
        return false;
    }

    private void OnGrabPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("GrabPerformed");
        if (isHovering && currentHoveredSlot != null)
        {
            DragAndDropHandler slotHandler = currentHoveredSlot.GetComponent<DragAndDropHandler>();
            if (slotHandler != null && slotHandler.prefab != null)
            {
                Debug.Log("Spawning prefab: " + slotHandler.prefab.name);
                SpawnObject(slotHandler.prefab);
                DeactivateInventory();
            }
            else
            {
                Debug.LogWarning("No prefab assigned to this slot.");
            }
        }
    }

    private void OnGrabCanceled(InputAction.CallbackContext context)
    {
        if (spawnedObject != null)
        {
            ReleaseObject();
            ActivateInventory();
        }
    }

    private void SpawnObject(GameObject prefabToSpawn)
    {
        Transform attachTransform = rayInteractor.attachTransform;

        // Instantiate the prefab at the interactor's position and rotation
        spawnedObject = Instantiate(prefabToSpawn, attachTransform.position, attachTransform.rotation);
        Debug.Log("Spawned object: " + spawnedObject.name);

        // Ensure it has XRGrabInteractable
        XRGrabInteractable grabInteractable = spawnedObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = spawnedObject.AddComponent<XRGrabInteractable>();
        }

        // Initiate grab interaction
        XRInteractionManager interactionManager = rayInteractor.interactionManager;
        if (interactionManager != null)
        {
            interactionManager.SelectEnter(rayInteractor as IXRSelectInteractor, grabInteractable as IXRSelectInteractable);
        }
    }

    private void ReleaseObject()
    {
        if (spawnedObject != null)
        {
            spawnedObject.transform.SetParent(null);
            spawnedObject = null;
        }
    }

    private void DeactivateInventory()
    {
        if (inventory != null)
        {
            inventoryRayInteractor.enabled = true;
            inventory.SetActive(false);
        }
    }

    private void ActivateInventory()
    {
        if (inventory != null)
        {
            inventoryRayInteractor.enabled = false;
            inventory.SetActive(true);
        }
    }
}
