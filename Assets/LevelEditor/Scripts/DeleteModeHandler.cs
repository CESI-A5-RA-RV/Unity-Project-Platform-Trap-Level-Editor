using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DeleteModeHandler : MonoBehaviour
{
    private XRRayInteractor rayInteractor; // The ray interactor component
    private bool isDeleteModeActive = false; // To track if delete mode is active

    public InputActionReference toggleDeleteModeAction; // Input action to toggle delete mode

    private LineRenderer lineRenderer; // Reference to the LineRenderer component

    private Material originalMaterial;
    public Material deleteMaterial;


    void Start()
    {
        rayInteractor = GetComponentInChildren<XRRayInteractor>();
        lineRenderer = GetComponentInChildren<LineRenderer>();

        if (rayInteractor == null)
        {
            Debug.LogError("No XRRayInteractor found on this controller.");
        }

        if (lineRenderer == null)
        {
            Debug.LogError("No LineRenderer found on this controller.");
        }
        else
        {
            // Store the original material of the LineRenderer
            originalMaterial = lineRenderer.material;
        }
    }

    void OnEnable()
    {
        if (toggleDeleteModeAction != null && toggleDeleteModeAction.action != null)
        {
            toggleDeleteModeAction.action.performed += OnToggleDeleteMode;
            toggleDeleteModeAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleDeleteModeAction != null && toggleDeleteModeAction.action != null)
        {
            toggleDeleteModeAction.action.performed -= OnToggleDeleteMode;
            toggleDeleteModeAction.action.Disable();
        }
    }

    void Update()
    {
        if (isDeleteModeActive)
        {
            HandleDeleteMode();
        }
        UpdateRayColor();
    }

    private void OnToggleDeleteMode(InputAction.CallbackContext context)
    {
        isDeleteModeActive = !isDeleteModeActive;

        if (isDeleteModeActive)
        {
            Debug.Log("Delete Mode Activated");
        }
        else
        {
            Debug.Log("Delete Mode Deactivated");
        }
    }

    private void HandleDeleteMode()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.transform != null && (hit.transform.gameObject.CompareTag("Platform") || hit.transform.gameObject.CompareTag("Trap")))
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    DeletePrefab(hit.transform.gameObject);
                }
            }
        }
    }

    private void DeletePrefab(GameObject prefab)
    {
        if (prefab.CompareTag("Platform") || prefab.CompareTag("Trap"))
        {
            Destroy(prefab);
        }
    }

    private void UpdateRayColor()
    {
        if (lineRenderer != null)  // Ensure the lineRenderer is not null
        {
            if (isDeleteModeActive)
            {
                lineRenderer.material = deleteMaterial;
            }
            else
            {
                lineRenderer.material = originalMaterial;
            }
        }
        else
        {
            Debug.LogWarning("LineRenderer is null, check the setup.");
        }
    }
}
