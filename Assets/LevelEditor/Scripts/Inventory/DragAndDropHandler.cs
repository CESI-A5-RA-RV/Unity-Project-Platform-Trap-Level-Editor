using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour, IPointerClickHandler
{
    public GameObject prefab;  // Prefab associated with this slot
    private GameObject grabbedObject;
    private bool isSelected = false;

    void Update()
    {
        // Only handle dragging if this slot is selected
        if (isSelected)
        {
            if (Input.GetButtonDown("Fire1") && grabbedObject == null)
            {
                // Instantiate the prefab at controller tip
                grabbedObject = Instantiate(prefab, transform.position, transform.rotation);
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                grabbedObject.transform.SetParent(transform); // Attach to VR controller
            }

            if (Input.GetButtonUp("Fire1") && grabbedObject != null)
            {
                // Place the object in the scene
                grabbedObject.GetComponent<Rigidbody>().isKinematic = false;
                grabbedObject.transform.SetParent(null); // Detach from controller
                grabbedObject = null;
                isSelected = false; // Deselect after placing
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Selected = true");
        isSelected = true;  // Select this inventory slot when clicked
    }
}
