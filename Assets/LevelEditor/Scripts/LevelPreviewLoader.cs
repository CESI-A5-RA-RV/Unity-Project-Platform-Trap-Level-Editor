using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LevelPreviewLoader : MonoBehaviour
{
    public Camera previewCamera;
    public Transform levelLoadingZone;
    public Transform buildingZone;

    void Start()
    {
        if (previewCamera == null || levelLoadingZone == null)
        {
            Debug.LogError("Please assign all necessary components (previewCamera, levelLoadingZone).");
            return;
        }

        previewCamera.transform.position = levelLoadingZone.position;
        previewCamera.transform.rotation = levelLoadingZone.rotation;
    }

    private List<ElementData> GetElementsFromBuildingZone()
    {
        List<ElementData> elements = new List<ElementData>();

        foreach (Transform child in buildingZone)
        {
            if (child.CompareTag("Platform") || child.CompareTag("Trap"))
            {
                string cleanName = Regex.Replace(child.name.Replace("(Clone)", "").Trim(), @"\s\(\d+\)$", "");

                ElementData elementData = new ElementData
                {
                    id = elements.Count,
                    elementType = cleanName,
                    position = new Vector3Data
                    {
                        x = child.localPosition.x,
                        y = child.localPosition.y,
                        z = child.localPosition.z
                    },
                    size = new Vector3Data
                    {
                        x = child.localScale.x,
                        y = child.localScale.y,
                        z = child.localScale.z
                    },
                    rotation = new Vector3Data
                    {
                        x = child.localRotation.eulerAngles.x,
                        y = child.localRotation.eulerAngles.y,
                        z = child.localRotation.eulerAngles.z
                    },
                    parameters = new List<Parameter>()
                };

                elements.Add(elementData);
            }
        }

        return elements;
    }

    public void LoadLevel()
    {
        ClearExistingElements();

        List<Transform> levelTransforms = new List<Transform>();

        foreach (var element in GetElementsFromBuildingZone())
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Traps/{element.elementType}") ??
                                Resources.Load<GameObject>($"Prefabs/Platforms/{element.elementType}");

            if (prefab)
            {
                GameObject instance = Instantiate(prefab, levelLoadingZone);
                instance.transform.localPosition = new Vector3(element.position.x, element.position.y, element.position.z);
                instance.transform.localScale = new Vector3(element.size.x, element.size.y, element.size.z);
                instance.transform.localRotation = Quaternion.Euler(new Vector3(element.rotation.x, element.rotation.y, element.rotation.z));

                levelTransforms.Add(instance.transform);
            }
            else
            {
                Debug.LogWarning($"Prefab not found for {element.elementType}");
            }
        }

        FitCameraToLevel(levelTransforms);
    }

    private void FitCameraToLevel(List<Transform> levelTransforms)
    {
        if (levelTransforms.Count == 0)
            return;

        // Calculate the bounding box manually using the positions of the transforms
        Vector3 minBounds = levelTransforms[0].position;
        Vector3 maxBounds = levelTransforms[0].position;

        foreach (Transform t in levelTransforms)
        {
            minBounds = Vector3.Min(minBounds, t.position);
            maxBounds = Vector3.Max(maxBounds, t.position);
        }

        Vector3 center = (minBounds + maxBounds) / 2;
        Vector3 size = maxBounds - minBounds;

        // Calculate Camera position
        Vector3 cameraPosition = center - previewCamera.transform.forward * Mathf.Max(size.x, size.y, size.z) / 2
                                 / Mathf.Tan(Mathf.Deg2Rad * previewCamera.fieldOfView / 2);

        // Add height adjustment
        cameraPosition.y = center.y + size.y / 2 + 2;

        float distanceFactor = 1.2f;  // Adjust this to control how far the camera should be
        cameraPosition -= previewCamera.transform.forward * (Mathf.Max(size.x, size.y, size.z) * distanceFactor / 2);

        previewCamera.transform.position = cameraPosition;
        previewCamera.transform.LookAt(center);
    }


    private void ClearExistingElements()
    {
        foreach (Transform child in levelLoadingZone)
        {
            if (child.CompareTag("Platform") || child.CompareTag("Trap"))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
