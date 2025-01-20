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

    //public GameObject pieMenuPrefab; // Reference to the pie menu prefab
    //private GameObject activePieMenu; // Active instance of the pie menu
    //public Transform playerCamera; // Reference to the player's camera (for facing the menu)
    //public float proximityThreshold = 4f; // Distance to trigger the pie menu

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
        // CheckProximityForPieMenu();
    }

    private void OnToggleSelectionMode(InputAction.CallbackContext context)
    {
        isSelectionModeActive = !isSelectionModeActive;
        if (!isSelectionModeActive)
        {
            Debug.Log("Clearing all selections since selection mode is off.");
            ClearAllSelections(); // Deselect all objects when switching to single-selection mode
        }

        Debug.Log(isSelectionModeActive ? "Selection Mode Activated" : "Selection Mode Deactivated");
    }

    private void OnSelectObject(InputAction.CallbackContext context)
    {
        if (FindObjectOfType<RadialMenu>().IsRadialMenuActive)
        {
            Debug.Log("Radial menu is active; ignoring selection.");
            return;
        }

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            GameObject targetObject = hit.transform.gameObject;

            if (targetObject.CompareTag("Platform") || targetObject.CompareTag("Trap"))
            {
                if (!isSelectionModeActive)
                {
                    // Single-selection mode
                    if (selectedObjects.Contains(targetObject))
                    {
                        Debug.Log("Deselect object");
                        OutlineManager.RemoveOutline(targetObject);
                        selectedObjects.Remove(targetObject);
                    }
                    else
                    {
                        ClearAllSelections();
                        Debug.Log("Select single object");
                        OutlineManager.AddOutline(targetObject);
                        selectedObjects.Add(targetObject);
                    }
                }
                else
                {
                    // Multi-selection mode
                    if (selectedObjects.Contains(targetObject))
                    {
                        Debug.Log("Deselect object");
                        OutlineManager.RemoveOutline(targetObject);
                        selectedObjects.Remove(targetObject);
                    }
                    else
                    {
                        Debug.Log("Select object");
                        OutlineManager.AddOutline(targetObject);
                        selectedObjects.Add(targetObject);
                    }
                }
            }
        }
    }

    private void ClearAllSelections()
    {
        foreach (GameObject obj in new HashSet<GameObject>(selectedObjects))
        {
            OutlineManager.RemoveOutline(obj);
        }
        selectedObjects.Clear();
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

    /*private void CheckProximityForPieMenu()
    {
        foreach (var obj in selectedObjects)
        {
            if (Vector3.Distance(transform.position, obj.transform.position) <= proximityThreshold)
                // || (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit) && hit.transform.gameObject == obj))
            {
                if (activePieMenu == null)
                {
                    activePieMenu = Instantiate(pieMenuPrefab);
                    activePieMenu.SetActive(true);
                }

                PositionPieMenu(obj);
                return;
            }
        }

        if (activePieMenu != null)
        {
            Destroy(activePieMenu);
        }
    }*/

    /*private void PositionPieMenu(GameObject obj)
    {
        // Calculate the direction from the object to the player
        Vector3 directionToPlayer = playerCamera.position - obj.transform.position;

        // Use the Collider to determine object size
        Vector3 objectSize;
        if (obj.TryGetComponent<Collider>(out Collider collider))
        {
            objectSize = collider.bounds.size;  // Use Collider bounds
        }
        else
        {
            objectSize = Vector3.one;
        }

        Vector3 dynamicOffset = directionToPlayer.normalized * (objectSize.magnitude * 0.5f + 1.0f);
        dynamicOffset += new Vector3(0, 0, -0.5f);  // Adding a little offset to bring it closer

        Vector3 menuPosition = obj.transform.position + dynamicOffset;
        activePieMenu.transform.position = menuPosition;

        activePieMenu.transform.rotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0, 180, 0);
    }*/
}
