using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;  // XR Interaction Toolkit for XRGrabInteractable
using UnityEngine.UI;  // For UI Button and Image

public class ObjectLockManager : MonoBehaviour
{
    public GameObject lockButtonPrefab;  // Prefab for lock/unlock button
    public Camera playerCamera;  // The player’s camera
    private GameObject currentLockButton;
    private bool isLocked = false;  // Lock state

    // Constant spawn position relative to the object
    private Vector3 buttonSpawnOffset = new Vector3(0, 1.5f, 0);  // Adjust this position as needed

    void Update()
    {
        HandleRaycasting();
    }

    private void HandleRaycasting()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);  // Ray from camera
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Platform") || hitObject.CompareTag("Trap"))
            {
                ShowLockButton(hitObject);

                if (Input.GetButtonDown("Fire1"))  // Left mouse or trigger click
                {
                    ToggleLock(hitObject);
                }
            }
        }
        else
        {
            HideLockButton();
        }
    }

    private void ShowLockButton(GameObject hitObject)
    {
        if (currentLockButton == null)
        {
            currentLockButton = Instantiate(lockButtonPrefab, hitObject.transform.position + buttonSpawnOffset, Quaternion.identity);
            currentLockButton.transform.LookAt(playerCamera.transform);
        }
    }

    private void HideLockButton()
    {
        if (currentLockButton != null)
        {
            Destroy(currentLockButton);
        }
    }

    private void ToggleLock(GameObject hitObject)
    {
        if (hitObject.CompareTag("Platform") || hitObject.CompareTag("Trap"))
        {
            isLocked = !isLocked;

            // Show lock icon (change sprite)
            Button buttonComponent = currentLockButton.GetComponent<Button>();
            Image icon = buttonComponent.GetComponentInChildren<Image>();

            if (isLocked)
            {
                icon.sprite = Resources.Load<Sprite>("Icons/Lock");  // Replace with your lock icon
                LockObject(hitObject);
            }
            else
            {
                icon.sprite = Resources.Load<Sprite>("Icons/Unlock");  // Replace with your unlock icon
                UnlockObject(hitObject);
            }
        }
    }

    private void LockObject(GameObject hitObject)
    {
        // Disable the XR Grab Interactable component
        XRGrabInteractable grabInteractable = hitObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;  // Disable grabbing
        }

        // Additional logic for locking can be added here (e.g., visual feedback)
    }

    private void UnlockObject(GameObject hitObject)
    {
        // Enable the XR Grab Interactable component again if you want to unlock
        XRGrabInteractable grabInteractable = hitObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;  // Re-enable grabbing
        }

        // Additional logic for unlocking can be added here
    }
}
