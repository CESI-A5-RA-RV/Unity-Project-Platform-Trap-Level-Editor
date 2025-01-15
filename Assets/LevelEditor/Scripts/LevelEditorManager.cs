using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class LevelEditorManager : MonoBehaviour
{
    public Transform buildingZone; // The designated Building Zone in the scene
    private string savePath; // Save path for the JSON file
    public MultiLevelData multiLevelData = new MultiLevelData();

    private int currentLevelId = 0; // Auto-increment ID for new levels
    private LevelData currentEditingLevel = null;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "levels.json");

        string directoryPath = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        multiLevelData.levels = new List<LevelData>();
        HideBuildingZone();
        LoadLevelsFromJson();
    }

    private void ShowBuildingZone()
    {
        buildingZone.gameObject.SetActive(true);
    }

    private void HideBuildingZone()
    {
        buildingZone.gameObject.SetActive(false);
    }

    public void CreateNewLevel(string newLevelName = null)
    {
        ShowBuildingZone();
        if (newLevelName == null || newLevelName == "")
        {
            newLevelName = $"Level ({currentLevelId + 1})";
        }

        // Check if level name already exists and update it
        newLevelName = EnsureUniqueLevelName(newLevelName);

        currentEditingLevel = new LevelData
        {
            id = currentLevelId,
            levelName = newLevelName,
            elements = new List<ElementData>()
        };
        Debug.Log("Building zone activated for new level.");
    }

    public void EditExistingLevel(int levelId)
    {
        ShowBuildingZone();
        ClearBuildingZone();
        currentEditingLevel = multiLevelData.levels.Find(level => level.id == levelId);

        if (currentEditingLevel == null)
        {
            Debug.LogError("Level not found!");
            return;
        }

        foreach (var element in currentEditingLevel.elements)
        {
            // Try loading the prefab from both "Traps" and "Platforms" folders
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Traps/{element.elementType}") ??
                                Resources.Load<GameObject>($"Prefabs/Platforms/{element.elementType}");

            if (prefab)
            {
                Debug.Log($"Prefab found for {element.elementType}");
                GameObject instance = Instantiate(prefab, buildingZone);
                instance.transform.localPosition = new Vector3(element.position.x, element.position.y, element.position.z);
                instance.transform.localScale = new Vector3(element.size.x, element.size.y, element.size.z);
                instance.transform.localRotation = Quaternion.Euler(new Vector3(element.rotation.x, element.rotation.y, element.rotation.z));
            }
            else
            {
                Debug.LogWarning($"Prefab not found for {element.elementType}");
            }
        }

        Debug.Log($"Building zone activated for editing level: {currentEditingLevel.levelName}");
    }

    public void SaveCurrentLevel()
    {
        currentEditingLevel.elements = GetElementsFromBuildingZone();

        if (currentEditingLevel == null)
        {
            Debug.LogWarning("No level currently being edited!");
            return;
        }
        if (currentEditingLevel.elements.Count == 0 || !HasRequiredElements(currentEditingLevel.elements))
        {
            Debug.LogWarning("Level must have 'Platform Start' and 'Platform End' elements, and each must have at most one copy.");
            return;
        }

        if (!multiLevelData.levels.Contains(currentEditingLevel))
        {
            multiLevelData.levels.Add(currentEditingLevel);
            currentLevelId++;
        }

        SaveToJson();
        Debug.Log($"Level saved: {currentEditingLevel.levelName}");
        HideBuildingZone();
    }

    private void ClearBuildingZone()
    {
        foreach (Transform child in buildingZone)
        {
            if (child.CompareTag("Platform") || child.CompareTag("Trap"))
            {
                Destroy(child.gameObject);
            }
        }
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

    private void SaveToJson()
    {
        string json = JsonUtility.ToJson(multiLevelData, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Levels saved to {savePath}");
    }

    private void LoadLevelsFromJson()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            multiLevelData = JsonUtility.FromJson<MultiLevelData>(json);
            currentLevelId = multiLevelData.levels.Count;
        }
        else
        {
            Debug.Log($"No levels found at {savePath}. Starting fresh.");
        }
    }

    private string EnsureUniqueLevelName(string baseName)
    {
        int count = 0;
        string newName = baseName;

        while (multiLevelData.levels.Exists(level => level.levelName == newName))
        {
            count++;
            newName = $"{baseName} ({count})";
        }

        return newName;
    }

    private bool HasRequiredElements(List<ElementData> elements)
    {
        bool hasStartPlatform = false;
        bool hasEndPlatform = false;

        foreach (var element in elements)
        {
            Debug.Log($"Checking element type: {element.elementType}");
            if (Regex.IsMatch(element.elementType, @"^Platform Start(?: \(\d+\))?$"))
            {
                if (hasStartPlatform)
                {
                    Debug.LogWarning("Duplicate 'Platform Start' detected!");
                    return false;
                }
                hasStartPlatform = true;
            }
            else if (Regex.IsMatch(element.elementType, @"^Platform End(?: \(\d+\))?$"))
            {
                if (hasEndPlatform)
                {
                    Debug.LogWarning("Duplicate 'Platform End' detected!");
                    return false;
                }
                hasEndPlatform = true;
            }
        }

        if (!hasStartPlatform) Debug.LogWarning("'Platform Start' not found!");
        if (!hasEndPlatform) Debug.LogWarning("'Platform End' not found!");

        return hasStartPlatform && hasEndPlatform;
    }

}
