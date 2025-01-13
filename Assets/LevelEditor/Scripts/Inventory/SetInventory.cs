using UnityEngine;
using UnityEngine.InputSystem;

public class SetIn : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject LeftAnchor;
    public GameObject RightAnchor;
    public InputActionReference toggleInventoryLeftAction;
    public InputActionReference toggleInventoryRightAction;

    private bool UIActive;
    private GameObject activeAnchor;

    private void OnEnable()
    {
        if (toggleInventoryLeftAction != null)
        {
            toggleInventoryLeftAction.action.performed += OnToggleInventoryLeft;
        }
        if (toggleInventoryRightAction != null)
        {
            toggleInventoryRightAction.action.performed += OnToggleInventoryRight;
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryLeftAction != null)
        {
            toggleInventoryLeftAction.action.performed -= OnToggleInventoryLeft;
        }
        if (toggleInventoryRightAction != null)
        {
            toggleInventoryRightAction.action.performed -= OnToggleInventoryRight;
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

    private void OnToggleInventoryRight(InputAction.CallbackContext context)
    {
        ToggleInventory(RightAnchor);
    }

    private void ToggleInventory(GameObject anchor)
    {
        UIActive = !UIActive;
        Inventory.SetActive(UIActive);

        if (UIActive)
        {
            activeAnchor = anchor;
        }
        else
        {
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
