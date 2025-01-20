using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DeleteModeHandler : MonoBehaviour
{
    private XRRayInteractor rayInteractor; // The ray interactor component
    private bool isDeleteModeActive = false; // To track if delete mode is active

    public InputActionReference toggleDeleteModeAction; // Input action to toggle delete mode
    public InputActionReference deleteObjectAction; // Input action to delete objects

    private LineRenderer lineRenderer; // Reference to the LineRenderer component

    private Material originalMaterial;
    public Material deleteMaterial;

    public HashSet<GameObject> selectedObjects = new HashSet<GameObject>();
    public ValidationDialog validationDialog; // Reference to the validation UI

    public SelectionHandler selectionHandler;

    void Start()
    {
        rayInteractor = GetComponentInChildren<XRRayInteractor>();
        lineRenderer = GetComponentInChildren<LineRenderer>();

        if (rayInteractor == null || lineRenderer == null || validationDialog == null)
        {
            Debug.LogError("Missing components! Ensure RayInteractor, LineRenderer, and ValidationDialog are assigned.");
        }

        selectionHandler = FindObjectOfType<SelectionHandler>();
        if (selectionHandler == null)
        {
            Debug.LogError("SelectionHandler not found. Ensure it is in the scene.");
        }

        originalMaterial = lineRenderer.material;
    }

    void OnEnable()
    {
        if (toggleDeleteModeAction != null && toggleDeleteModeAction.action != null)
        {
            toggleDeleteModeAction.action.performed += OnToggleDeleteMode;
            toggleDeleteModeAction.action.Enable();
        }

        if (deleteObjectAction != null && deleteObjectAction.action != null)
        {
            deleteObjectAction.action.performed += OnDeleteObject;
            deleteObjectAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (toggleDeleteModeAction != null && toggleDeleteModeAction.action != null)
        {
            toggleDeleteModeAction.action.performed -= OnToggleDeleteMode;
            toggleDeleteModeAction.action.Disable();
        }

        if (deleteObjectAction != null && deleteObjectAction.action != null)
        {
            deleteObjectAction.action.performed -= OnDeleteObject;
            deleteObjectAction.action.Disable();
        }
    }

    void Update()
    {
        UpdateRayColor();
    }

    private void OnToggleDeleteMode(InputAction.CallbackContext context)
    {
        isDeleteModeActive = !isDeleteModeActive;
        Debug.Log(isDeleteModeActive ? "Delete Mode Activated" : "Delete Mode Deactivated");
    }

    private void OnDeleteObject(InputAction.CallbackContext context)
    {
        if (isDeleteModeActive)
        {
            var selectedObjects = selectionHandler.SelectedObjects;

            Debug.Log($"Number of selected objects: {selectedObjects.Count}");

            if (selectedObjects.Count > 1)
            {
                // Show validation dialog if multiple elements are selected
                validationDialog.Show(
                    "Many elements are selected. This action will delete all selected elements. Are you sure you want to proceed?",
                    () => { DeleteAllSelectedObjects(selectedObjects); },
                    () => { Debug.Log("Delete action cancelled."); }
                );
            }
            else if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject target = hit.transform.gameObject;
                DeletePrefab(target);
            }
        }
    }

    private void DeletePrefab(GameObject prefab)
    {
        if (prefab.CompareTag("Platform") || prefab.CompareTag("Trap"))
        {
            Debug.Log($"Deleting object: {prefab.name}");
            Destroy(prefab);
            selectedObjects.Remove(prefab);
        }
    }

    private void DeleteAllSelectedObjects(HashSet<GameObject> selectedObjects)
    {
        foreach (var obj in new HashSet<GameObject>(selectedObjects))
        {
            if (obj != null)
            {
                Debug.Log($"Deleting selected object: {obj.name}");
                Destroy(obj);
            }
        }
        selectedObjects.Clear();
        Debug.Log("All selected objects have been deleted.");
    }

    private void UpdateRayColor()
    {
        if (lineRenderer != null)
        {
            lineRenderer.material = isDeleteModeActive ? deleteMaterial : originalMaterial;
        }
        else
        {
            Debug.LogWarning("LineRenderer is null, check the setup.");
        }
    }
}
