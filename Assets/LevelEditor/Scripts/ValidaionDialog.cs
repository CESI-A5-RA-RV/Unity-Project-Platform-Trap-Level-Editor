using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ValidationDialog : MonoBehaviour
{
    public TextMeshProUGUI messageText; // Reference to the UI text displaying the message
    public Button yesButton; // YES button
    public Button cancelButton; // CANCEL button

    private Camera playerCamera; // Reference to the player's camera
    private System.Action onYesCallback; // Callback for YES button
    private System.Action onCancelCallback; // Callback for CANCEL button

    void Start()
    {
        playerCamera = Camera.main; // Get the player's main camera

        // Ensure buttons are properly wired
        yesButton.onClick.AddListener(OnYesClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        gameObject.SetActive(false); // Hide by default
    }

    /// <summary>
    /// Show the validation dialog with a custom message and callbacks.
    /// </summary>
    public void Show(string message, System.Action onYes, System.Action onCancel = null)
    {
        messageText.text = message;
        onYesCallback = onYes;
        onCancelCallback = onCancel;
        gameObject.SetActive(true);

        // Position the dialog in front of the player
        PositionDialogInFrontOfPlayer();
    }

    /// <summary>
    /// Positions the dialog in front of the player.
    /// </summary>
    private void PositionDialogInFrontOfPlayer()
    {
        if (playerCamera != null)
        {
            Vector3 cameraPosition = playerCamera.transform.position;
            Vector3 cameraForward = playerCamera.transform.forward;
            float dialogDistance = 2.0f; // You can adjust this distance

            transform.position = cameraPosition + cameraForward * dialogDistance;
            transform.forward = cameraForward; // Make sure the dialog faces the camera
        }
    }

    /// <summary>
    /// Called when the YES button is clicked.
    /// </summary>
    private void OnYesClicked()
    {
        onYesCallback?.Invoke();
        Close();
    }

    /// <summary>
    /// Called when the CANCEL button is clicked.
    /// </summary>
    private void OnCancelClicked()
    {
        onCancelCallback?.Invoke();
        Close();
    }

    /// <summary>
    /// Hides the dialog.
    /// </summary>
    private void Close()
    {
        gameObject.SetActive(false);
    }
}
