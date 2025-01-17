using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class SelectionHandler : MonoBehaviour
{
    private XRRayInteractor rayInteractor; // The ray interactor component
    private bool isSelectionModeActive = false; // To track if selection mode is active

    public InputActionReference toggleSelectionModeAction; // Input action to toggle selection mode
    public InputActionReference selectObjectAction; // Input action to select objects

    private LineRenderer lineRenderer; // Reference to the LineRenderer component
    private Material originalMaterial;
    public Material selectionMaterial;

    private HashSet<GameObject> selectedObjects = new HashSet<GameObject>(); // HashSet for selected objects
    public HashSet<GameObject> SelectedObjects => selectedObjects;


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
        originalMaterial = lineRenderer.material;
    }

    void OnEnable()
    {
        if (toggleSelectionModeAction != null && toggleSelectionModeAction.action != null)
        {
            toggleSelectionModeAction.action.performed += OnToggleSelectionMode;
            toggleSelectionModeAction.action.Enable();
        }

        if (selectObjectAction != null && selectObjectAction.action != null)
        {
            selectObjectAction.action.performed += OnSelectObject;
            selectObjectAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleSelectionModeAction != null && toggleSelectionModeAction.action != null)
        {
            toggleSelectionModeAction.action.performed -= OnToggleSelectionMode;
            toggleSelectionModeAction.action.Disable();
        }

        if (selectObjectAction != null && selectObjectAction.action != null)
        {
            selectObjectAction.action.performed -= OnSelectObject;
            selectObjectAction.action.Disable();
        }
    }

    void Update()
    {
        UpdateRayColor();
    }

    private void OnToggleSelectionMode(InputAction.CallbackContext context)
    {
        isSelectionModeActive = !isSelectionModeActive;

        Debug.Log(isSelectionModeActive ? "Selection Mode Activated" : "Selection Mode Deactivated");
    }

    private void OnSelectObject(InputAction.CallbackContext context)
    {
        if (isSelectionModeActive && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            GameObject targetObject = hit.transform.gameObject;

            if (targetObject.CompareTag("Platform") || targetObject.CompareTag("Trap"))
            {
                if (selectedObjects.Contains(targetObject))
                {
                    Debug.Log("Deselect object");
                    RemoveOutline(targetObject);
                    selectedObjects.Remove(targetObject);
                }
                else
                {
                    Debug.Log("Select object");
                    AddOutline(targetObject);
                    selectedObjects.Add(targetObject);
                }
            }
        }
    }

    private void AddOutline(GameObject obj)
    {
        Debug.Log("Add outline");
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }

        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.white; // Adjust color as needed
        outline.OutlineWidth = 5.0f; // Adjust width as needed
    }

    private void RemoveOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }

    private void UpdateRayColor()
    {
        if (lineRenderer != null)
        {
            lineRenderer.material = isSelectionModeActive ? selectionMaterial : originalMaterial;
        }
        else
        {
            Debug.LogWarning("LineRenderer is null, check the setup.");
        }
    }
}