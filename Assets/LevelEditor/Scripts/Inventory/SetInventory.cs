using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SetInventory : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject LeftAnchor;
    public InputActionReference toggleInventoryLeftAction;
    public XRRayInteractor inventoryRayInteractor;

    private bool UIActive;
    private GameObject activeAnchor;

    private void OnEnable()
    {
        if (toggleInventoryLeftAction != null)
        {
            toggleInventoryLeftAction.action.performed += OnToggleInventoryLeft;
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryLeftAction != null)
        {
            toggleInventoryLeftAction.action.performed -= OnToggleInventoryLeft;
        }
    }

    private void Start()
    {
        Inventory.SetActive(false);
        UIActive = false;
        activeAnchor = null;
    }

    private void OnToggleInventoryLeft(InputAction.CallbackContext context)
    {
        ToggleInventory(LeftAnchor);
    }

    private void ToggleInventory(GameObject anchor)
    {
        UIActive = !UIActive;
        Inventory.SetActive(UIActive);

        if (UIActive)
        {
            inventoryRayInteractor.enabled = false;
            activeAnchor = anchor;
        }
        else
        {
            inventoryRayInteractor.enabled = true;
            activeAnchor = null;
        }
    }

    private void Update()
    {
        if (UIActive && activeAnchor != null)
        {
            Inventory.transform.position = activeAnchor.transform.position;
            Inventory.transform.rotation = activeAnchor.transform.rotation;
        }
    }
}
