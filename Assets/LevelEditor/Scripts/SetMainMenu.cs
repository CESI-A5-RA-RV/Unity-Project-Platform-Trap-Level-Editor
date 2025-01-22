using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetMainMenu : MonoBehaviour
{
    public GameObject MainMenu; // The menu GameObject
    public InputActionReference toggleMenu; // Input action to toggle the menu
    public Transform playerAnchor; // Reference to the player's body (or camera rig anchor)

    public float menuDistance = 2f; // Distance of the menu from the player
    public float menuHeight = -0.1f; // Height of the menu relative to the anchor

    private bool UIActive;

    private void OnEnable()
    {
        if (toggleMenu != null)
        {
            toggleMenu.action.performed += OnToggleMenu;
        }
    }

    private void OnDisable()
    {
        if (toggleMenu != null)
        {
            toggleMenu.action.performed -= OnToggleMenu;
        }
    }

    private void Start()
    {
        MainMenu.SetActive(false);
        UIActive = false;
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        UIActive = !UIActive;
        MainMenu.SetActive(UIActive);

        if (UIActive)
        {
            PositionMenu();
        }
    }

    private void Update()
    {
        if (UIActive)
        {
            PositionMenu();
        }
    }

    private void PositionMenu()
    {
        if (playerAnchor == null) return;

        Vector3 forwardDirection = new Vector3(playerAnchor.forward.x, 0, playerAnchor.forward.z).normalized;
        Vector3 menuPosition = playerAnchor.position + forwardDirection * menuDistance;
        menuPosition.y = playerAnchor.position.y + menuHeight;
        MainMenu.transform.position = menuPosition;

        MainMenu.transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
    }
}
