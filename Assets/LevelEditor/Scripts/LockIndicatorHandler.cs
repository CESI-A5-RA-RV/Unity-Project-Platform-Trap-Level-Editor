using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LockIndicatorHandler : MonoBehaviour
{
    private XRRayInteractor rayInteractor; // The ray interactor component
    private GameObject currentHitObject; // The currently hit object
    public GameObject lockIconPrefab; // Prefab for the lock icon
    private GameObject currentLockIcon; // The lock icon instance

    void Start()
    {
        rayInteractor = GetComponentInChildren<XRRayInteractor>(); // Get the XRRayInteractor component attached to this GameObject
    }

    void Update()
    {
        HandleLockIcon();
    }

    void HandleLockIcon()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            currentHitObject = hit.transform.gameObject;

            if (currentHitObject != null)
            {
                // Check if the hit object is grabbable
                XRGrabInteractable grabbable = currentHitObject.GetComponent<XRGrabInteractable>();

                if (grabbable != null && grabbable.isHovered)
                {
                    HideLockIcon(); // If the object is grabbable, hide the lock icon
                }
                else
                {
                    // If not grabbable, show the lock icon at the point of contact between the ray and the object
                    ShowLockIcon(hit.point, hit.normal);
                }
            }
        }
        else
        {
            HideLockIcon(); // Hide lock icon when no object is hit by the ray
        }
    }

    void ShowLockIcon(Vector3 position, Vector3 normal)
    {
        if (currentLockIcon == null)
        {
            // Instantiate the lock icon if it doesn't exist
            currentLockIcon = Instantiate(lockIconPrefab);
        }

        // Set the position and orientation of the lock icon
        currentLockIcon.transform.position = position;
        currentLockIcon.transform.rotation = Quaternion.LookRotation(normal);
        currentLockIcon.SetActive(true); // Ensure the lock icon is visible
    }

    void HideLockIcon()
    {
        if (currentLockIcon != null)
        {
            currentLockIcon.SetActive(false); // Hide the lock icon
        }
    }
}
