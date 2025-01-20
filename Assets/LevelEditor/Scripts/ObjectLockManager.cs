using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectLockManager : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private bool isLocked = false;

    // Public property to get the lock status
    public bool IsLocked
    {
        get { return isLocked; }
    }

    public void ToggleLock(GameObject targetedObject)
    {
        grabInteractable = targetedObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            isLocked = !isLocked;
            grabInteractable.enabled = !isLocked;

            Debug.Log("Object " + (isLocked ? "locked" : "unlocked"));
        }
        else
        {
            Debug.LogWarning("No XRGrabInteractable found on this object.");
        }
    }
}
