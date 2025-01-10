using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject prefab; // Assign the prefab linked to this thumbnail
    private GameObject draggedInstance;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (prefab != null)
        {
            draggedInstance = Instantiate(prefab);
            draggedInstance.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedInstance != null)
        {
            draggedInstance.SetActive(true);

            Vector3 screenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane);
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPoint);

            draggedInstance.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedInstance != null)
        {
            draggedInstance.transform.position = GetValidDropPosition();

            AddObjectManipulator(draggedInstance);

            draggedInstance = null;
        }
    }

    private Vector3 GetValidDropPosition()
    {
        // Example: Drop prefab on ground plane
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }

        // Default position if no valid drop target
        return mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
    }

    private void AddObjectManipulator(GameObject obj)
    {
        ObjectManipulator manipulator = obj.AddComponent<ObjectManipulator>();
        manipulator.EnableManipulation();
    }
}
