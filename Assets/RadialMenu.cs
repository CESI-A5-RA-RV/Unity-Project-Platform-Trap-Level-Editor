using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public class RadialPartData
{
    public Sprite icon; // Icon to display
    public string name; // Name to display
}

public class RadialMenu : MonoBehaviour
{
    public bool IsRadialMenuActive { get; private set; } // flag added

    [Range(2, 10)]
    public int numberOfRadialPart;
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public float angleBetweenPart = 10f;
    public Transform handTransform;
    public XRRayInteractor rayInteractor;
    public InputActionReference toggleMenuAction;
    public InputActionReference selectRadialPartAction;

    public Sprite lockedSprite;    // Public variable for the locked icon
    public Sprite unlockedSprite;  // Public variable for the unlocked icon

    //public UnityEvent<int> OnPartSelected;
    public ObjectLockManager objectLockManager;

    public List<RadialPartData> radialPartDataList = new List<RadialPartData>();

    private List<GameObject> spawnedParts = new List<GameObject>();
    private int currentSelectedRadialPart = -1;
    private GameObject targetObject;

    private Color? previousOutlineColor;
    private float previousOutlineWidth;

    void Awake()
    {
        // Ensure toggleMenuAction and selectRadialPartAction are properly assigned
        if (toggleMenuAction != null)
        {
            toggleMenuAction.action.performed += ctx => ToggleMenu();
        }
        else
        {
            Debug.LogWarning("toggleMenuAction is not assigned!");
        }

        if (selectRadialPartAction != null)
        {
            selectRadialPartAction.action.performed += ctx => SelectRadialPart();
        }
        else
        {
            Debug.LogWarning("selectRadialPartAction is not assigned!");
        }
    }

    private void OnEnable()
    {
        if (toggleMenuAction != null) toggleMenuAction.action.Enable();
        if (selectRadialPartAction != null) selectRadialPartAction.action.Enable();
    }

    private void OnDisable()
    {
        if (toggleMenuAction != null) toggleMenuAction.action.Disable();
        if (selectRadialPartAction != null) selectRadialPartAction.action.Disable();
    }

    private void ToggleMenu()
    {
        if (!IsRadialMenuActive)
        {
            // Check if raycast is hitting a valid object
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                targetObject = hit.transform.gameObject;

                if (targetObject.CompareTag("Platform") || targetObject.CompareTag("Trap"))
                {
                    IsRadialMenuActive = true; // Activate the menu
                    radialPartCanvas.gameObject.SetActive(true);

                    AdjustRadialPartDataList();
                    SpawnRadialPart();

                    Outline currentOutline = targetObject.GetComponent<Outline>();
                    if (currentOutline != null)
                    {
                        previousOutlineColor = currentOutline.OutlineColor;
                        previousOutlineWidth = currentOutline.OutlineWidth;
                    }
                    else
                    {
                        previousOutlineColor = null;
                    }

                    OutlineManager.AddOutline(targetObject, Color.blue);

                    // Disable raycast
                    rayInteractor.enabled = false;
                }
                else
                {
                    Debug.Log("Radial menu can only be activated on objects with 'Platform' or 'Trap' tags.");
                }
            }
            else
            {
                Debug.Log("No valid object under raycast to activate radial menu.");
            }
        }
        else
        {
            IsRadialMenuActive = false; // Deactivate the menu
            radialPartCanvas.gameObject.SetActive(false);

            HideRadialParts();

            // Re-enable raycast
            rayInteractor.enabled = true;
        }
    }

    void Update()
    {
        if (IsRadialMenuActive)
        {
            GetSelectedRadialPart();
        }
    }

    private void AdjustRadialPartDataList()
    {
        if (radialPartDataList.Count < numberOfRadialPart)
        {
            // Add missing entries
            for (int i = radialPartDataList.Count; i < numberOfRadialPart; i++)
            {
                radialPartDataList.Add(new RadialPartData
                {
                    icon = null,
                    name = "Part " + (i + 1) // Default name
                });
            }
        }
        else if (radialPartDataList.Count > numberOfRadialPart)
        {
            radialPartDataList.RemoveRange(numberOfRadialPart, radialPartDataList.Count - numberOfRadialPart);
        }
    }

    public void HideRadialParts()
    {
        // Restore the previous outline if color was stored
        if (previousOutlineColor.HasValue)
        {
            OutlineManager.AddOutline(targetObject, previousOutlineColor.Value, previousOutlineWidth);
        }
        else
        {
            OutlineManager.RemoveOutline(targetObject);
        }

        targetObject = null;
        radialPartCanvas.gameObject.SetActive(false);
    }

    public void GetSelectedRadialPart()
    {
        Vector2 centerToHand = handTransform.position - radialPartCanvas.position;
        Vector3 centerToHandProjected = Vector3.ProjectOnPlane(centerToHand, radialPartCanvas.forward);

        float angle = Vector3.SignedAngle(radialPartCanvas.up, centerToHandProjected, -radialPartCanvas.forward);

        if (angle < 0)
        {
            angle += 360;
        }

        currentSelectedRadialPart = (int)angle * numberOfRadialPart / 360;

        for (int i = 0; i < spawnedParts.Count; i++)
        {
            if (i == currentSelectedRadialPart)
            {
                spawnedParts[i].GetComponent<Image>().color = Color.blue;
                spawnedParts[i].transform.localScale = 1.1f * Vector2.one;
            }
            else
            {
                spawnedParts[i].GetComponent<Image>().color = Color.white;
                spawnedParts[i].transform.localScale = 1 * Vector3.one;
            }
;       }
    }

    private void SelectRadialPart()
    {
        if (IsRadialMenuActive && currentSelectedRadialPart >= 0)
        {
            Debug.Log($"Selected Radial Part: {radialPartDataList[currentSelectedRadialPart].name}");
            //OnPartSelected.Invoke(currentSelectedRadialPart);

            if (currentSelectedRadialPart == 0) // Assuming the first radial part corresponds to the lock functionality
            {
                objectLockManager.ToggleLock(targetObject);  // Call ToggleLock from ObjectLockManager

                // Change the icon based on the lock state
                if (objectLockManager.IsLocked)
                {
                    // Update the icon to unlockedSprite
                    radialPartDataList[currentSelectedRadialPart].icon = unlockedSprite;
                }
                else
                {
                    // Update the icon to lockedSprite
                    radialPartDataList[currentSelectedRadialPart].icon = lockedSprite;
                }
                SpawnRadialPart();
            }
            else if (currentSelectedRadialPart == 1) // New addition: Delete object on selection
            {
                if (targetObject != null)
                {
                    Destroy(targetObject);
                    ToggleMenu();
                }
            }
        }
    }

    public void SpawnRadialPart()
    {
        radialPartCanvas.gameObject.SetActive(true);
        radialPartCanvas.position = handTransform.position;
        radialPartCanvas.rotation = handTransform.rotation;

        foreach (var item in spawnedParts)
        {
            Destroy(item);
        }
        spawnedParts.Clear();

        for (int i = 0; i < numberOfRadialPart; i++)
        {
            float angle = -i * 360 / numberOfRadialPart - angleBetweenPart / 2;
            Vector3 radialPartEulerAngle = new Vector3(0, 0, angle);

            GameObject spawnedRadialPart = Instantiate(radialPartPrefab, radialPartCanvas);
            spawnedRadialPart.transform.position = radialPartCanvas.position;
            spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle;

            spawnedRadialPart.GetComponent<Image>().fillAmount = (1 / (float)numberOfRadialPart) - (angleBetweenPart / 360);

            if (i < radialPartDataList.Count)
            {
                RadialPartData data = radialPartDataList[i];

                // Find the Icon GameObject within the prefab hierarchy
                Transform iconTransform = spawnedRadialPart.transform.Find("IconDir/Icon");
                if (iconTransform)
                {
                    // Set the sprite of the Image component
                    iconTransform.GetComponent<Image>().sprite = data.icon;
                }
                else
                {
                    Debug.LogWarning($"IconDir/Icon not found in radialPartPrefab at index {i}");
                }
            }
            spawnedParts.Add(spawnedRadialPart);
        }
    }
};
