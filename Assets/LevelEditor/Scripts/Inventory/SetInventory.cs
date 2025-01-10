using UnityEngine;
using UnityEngine.InputSystem;

public class SetIn : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject Anchor;
    public InputActionReference toggleInventoryAction; // Assign this in the Inspector

    private bool UIActive;

    private void OnEnable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.performed += OnToggleInventory;
        }
    }

    private void OnDisable()
    {
        if (toggleInventoryAction != null)
        {
            toggleInventoryAction.action.performed -= OnToggleInventory;
        }
    }

    private void Start()
    {
        Inventory.SetActive(false);
        UIActive = false;
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        UIActive = !UIActive;
        Inventory.SetActive(UIActive);
    }

    private void Update()
    {
        if (UIActive)
        {
            Inventory.transform.position = Anchor.transform.position;
            Inventory.transform.eulerAngles = new Vector3(Anchor.transform.eulerAngles.x + 15, Anchor.transform.eulerAngles.y, 0);
        }
    }
}
