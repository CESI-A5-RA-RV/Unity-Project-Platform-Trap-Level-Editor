using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobilePlatformConfigurator : MonoBehaviour
{
    [Header("References")]
    public Transform platform;            // The moving platform
    public LineRenderer originalLineRenderer;     // LineRenderer for the path
    public Transform anchor;              // Anchor point for positioning the UI

    [Header("UI Positioning")]
    public float distance = 1f;           // Distance from the anchor
    public float menuHeight = -0.1f;      // Vertical offset for the UI
    public float offset = -0.5f;

    [Header("UI Components")]
    public Slider rangeSlider;            // Slider for range adjustment
    public Slider travelTimeSlider;       // Slider for travel time
    public Button playPauseButton;        // Button for previewing movement
    public TMP_Text travelTimeText;       // Display for travel time
    public TMP_Text rangeText;            // Display for range
    public GameObject settingsPanel;      // Reference to the settings panel UI
    public GameObject errorPanel;         // Reference to the error panel UI

    private Vector3 startMarker;          // Start position
    private Vector3 endMarker;            // End position
    private bool isPlaying = false;       // Preview state
    private float travelTime = 5.0f;      // Travel time (seconds)
    private float range = 5.0f;           // Movement range (units)
    private Coroutine movementCoroutine;  // Reference to running coroutine
    private MovingPlatform movingPlatform; // Reference to the MovingPlatform component
    private LineRenderer currentLineRenderer;
    private bool UIActive => gameObject.activeSelf; // Check if UI is active

    public void Initialize(GameObject targetPlatform)
    {
        gameObject.SetActive(true);
        platform = targetPlatform.transform;

        if (anchor != null)
        {
            PositionUI();
        }
        else
        {
            Debug.LogWarning("Anchor is null. Defaulting to platform position.");
            transform.position = platform.position;
            transform.rotation = Quaternion.identity;
        }

        // Get the MovingPlatform component from the platform
        movingPlatform = targetPlatform.GetComponentInChildren<MovingPlatform>();
        if (movingPlatform != null)
        {
            // Initialize sliders based on the MovingPlatform's properties
            range = movingPlatform.Range;
            travelTime = movingPlatform.Speed; // Assuming Speed corresponds to travel time
            rangeSlider.value = range;
            travelTimeSlider.value = travelTime;
        }
        else
        {
            // Hide settings panel and show error panel if MovingPlatform is missing
            settingsPanel.SetActive(false);
            errorPanel.SetActive(true);
            return; // Exit initialization early
        }

        startMarker = platform.position;
        endMarker = startMarker + platform.right * range;

        currentLineRenderer = platform.GetComponent<LineRenderer>();
        CreateLineRenderer();
        UpdateLineRenderer();
        UpdateUIText();

        Debug.Log($"UI Initialized at {transform.position} facing {transform.forward}");
    }

    private void Update()
    {
        if (UIActive)
        {
            PositionUI();
        }
    }

    private void PositionUI()
    {
        if (anchor == null) return;

        Vector3 forwardDirection = new Vector3(anchor.forward.x, 0, anchor.forward.z).normalized;
        Vector3 leftOffset = Vector3.Cross(Vector3.up, forwardDirection).normalized * offset;

        Vector3 menuPosition = anchor.position + forwardDirection * distance + leftOffset;
        menuPosition.y = anchor.position.y + menuHeight;

        transform.position = menuPosition;
        transform.rotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
    }

    void Start()
    {
        rangeSlider.onValueChanged.AddListener(OnRangeChanged);
        travelTimeSlider.onValueChanged.AddListener(OnTravelTimeChanged);
    }

    void OnRangeChanged(float value)
    {
        range = value;
        endMarker = startMarker + platform.right * range;
        UpdateLineRenderer();
        UpdateUIText();

        // Update the MovingPlatform component's Range
        if (movingPlatform != null)
        {
            movingPlatform.Range = range;
            Debug.Log($"Updated MovingPlatform Range to: {range}");
        }
    }

    void OnTravelTimeChanged(float value)
    {
        travelTime = value;
        UpdateUIText();

        // Update the MovingPlatform component's Speed
        if (movingPlatform != null)
        {
            movingPlatform.Speed = travelTime; // Assuming Speed corresponds to travel time
            Debug.Log($"Updated MovingPlatform Speed to: {travelTime}");
        }
    }

    public void Play()
    {
        isPlaying = true;
        movementCoroutine = StartCoroutine(PreviewMovement());
    }

    public void Pause()
    {
        isPlaying = false;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    IEnumerator PreviewMovement() // Should do the same thing as Pricilla MovingWall.cs
    {
        float elapsedTime = 0;
        Vector3 currentStart = startMarker;
        Vector3 currentEnd = endMarker;

        while (isPlaying)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.PingPong(elapsedTime / travelTime, 1.0f);
            platform.position = Vector3.Lerp(currentStart, currentEnd, t);

            yield return null;
        }
    }

    private void CreateLineRenderer()
    {
        if (currentLineRenderer == null && platform != null)
        {
            currentLineRenderer = platform.gameObject.AddComponent<LineRenderer>();
            currentLineRenderer.positionCount = 2;
            currentLineRenderer.startWidth = 0.05f;
            currentLineRenderer.endWidth = 0.05f;
        }
        currentLineRenderer.material = originalLineRenderer.material;
        currentLineRenderer.enabled = true;
    }


    void UpdateLineRenderer()
    {
        if (currentLineRenderer != null)
        {
            currentLineRenderer.positionCount = 2;
            currentLineRenderer.SetPosition(0, startMarker);
            currentLineRenderer.SetPosition(1, endMarker);
        }
    }

    void UpdateUIText()
    {
        if (rangeText != null)
        {
            rangeText.text = $"Range: {range} units";
        }
        if (travelTimeText != null)
        {
            travelTimeText.text = $"Travel Time: {travelTime} seconds";
        }
    }

    public void CloseUI()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        ResetPlatformPosition();

        if (currentLineRenderer != null)
        {
            currentLineRenderer.enabled = false;
        }

        isPlaying = false;
        gameObject.SetActive(false);
    }

    private void ResetPlatformPosition()
    {
        if (platform != null)
        {
            platform.position = startMarker;
        }
    }
}

