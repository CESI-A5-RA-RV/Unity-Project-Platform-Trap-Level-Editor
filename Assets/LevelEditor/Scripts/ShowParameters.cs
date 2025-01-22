using UnityEngine;

public class ShowMovingPlatformParameters : MonoBehaviour
{
    public LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponentInChildren<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        else
        {
            Debug.LogWarning($"No LineRenderer found on {gameObject.name}");
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = isSelected;
        }
    }
}
