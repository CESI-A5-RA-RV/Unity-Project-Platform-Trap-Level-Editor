using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    // Method to add an outline to a GameObject
    public static void AddOutline(GameObject obj, Color outlineColor = default, float outlineWidth = 5.0f)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }

        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = outlineColor == default ? Color.white : outlineColor;
        outline.OutlineWidth = outlineWidth;
    }

    // Method to remove the outline from a GameObject
    public static void RemoveOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }
}
