using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraController : MonoBehaviour
{
    public Camera targetCamera;
    public Transform quad; // Quad the user interacts with
    public Transform cameraRig; // Parent object of the camera for movement
    public InputActionReference rotateCameraAction;
    public InputActionReference moveCameraAction;
    public InputActionReference grabButtonAction;
    public XRRayInteractor leftRayInteractor; // Left hand ray
    public XRRayInteractor rightRayInteractor; // Right hand ray

    private bool isDragging = false;
    private Vector3 dragStartPosition;

    void OnEnable()
    {
        rotateCameraAction.action.Enable();
        moveCameraAction.action.Enable();
        grabButtonAction.action.Enable();

        grabButtonAction.action.performed += StartDrag;
        grabButtonAction.action.canceled += EndDrag;
    }

    void OnDisable()
    {
        rotateCameraAction.action.Disable();
        moveCameraAction.action.Disable();
        grabButtonAction.action.Disable();

        grabButtonAction.action.performed -= StartDrag;
        grabButtonAction.action.canceled -= EndDrag;
    }

    void Update()
    {
        if (IsRayHoveringOverQuad())
        {
            // Rotate Camera
            Vector2 rotationInput = rotateCameraAction.action.ReadValue<Vector2>();
            RotateCamera(rotationInput);

            // Drag Camera Movement
            if (isDragging)
            {
                Vector3 currentPosition = moveCameraAction.action.ReadValue<Vector3>();
                DragCamera(currentPosition);
            }
        }
    }

    private bool IsRayHoveringOverQuad()
    {
        // Check if the ray from either hand is hitting the quad
        return IsInteractorHoveringOver(leftRayInteractor) || IsInteractorHoveringOver(rightRayInteractor);
    }

    private bool IsInteractorHoveringOver(XRRayInteractor interactor)
    {
        if (interactor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            return hit.transform == quad;
        }
        return false;
    }

    private void RotateCamera(Vector2 input)
    {
        if (input != Vector2.zero && targetCamera != null)
        {
            Debug.Log("In rotate");
            float rotationSpeed = 50f;
            targetCamera.transform.Rotate(Vector3.up, input.x * rotationSpeed * Time.deltaTime, Space.World);
            targetCamera.transform.Rotate(Vector3.right, -input.y * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void StartDrag(InputAction.CallbackContext context)
    {
        if (IsRayHoveringOverQuad())
        {
            isDragging = true;
            dragStartPosition = moveCameraAction.action.ReadValue<Vector3>();
        }
    }

    private void EndDrag(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    private void DragCamera(Vector3 currentPosition)
    {
        if (targetCamera != null && cameraRig != null)
        {
            Debug.Log(currentPosition);

            Vector3 dragDelta = currentPosition - dragStartPosition;
            cameraRig.position += dragDelta * 0.1f; // Adjust movement sensitivity
            dragStartPosition = currentPosition;
        }
    }
}
