using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class LevelManager : MonoBehaviour
{
    public Transform buildingZone;
    public TMP_InputField levelNameInputField;
    public TMP_Dropdown levelDropdown;
    public TMP_Text messageDisplay;

    private string savePath;
    private int currentLevelId = 0;
    private LevelData currentEditingLevel = null;
    private MultiLevelData multiLevelData = new MultiLevelData();

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "levels.json");

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        }

        multiLevelData.levels = new List<LevelData>();
        LoadLevelsFromJson();
        ShowBuildingZone();
    }

    public void CreateNewLevel()
    {
        string defaultName = $"Level ({currentLevelId + 1})";
        currentEditingLevel = new LevelData
        {
            id = currentLevelId,
            levelName = defaultName,
            elements = new List<ElementData>()
        };
        ClearBuildingZone();
        
    }

    public void EditExistingLevelMode()
    {
        PopulateDropdownWithLevels();

        ShowBuildingZone();
    }

    private void PopulateDropdownWithLevels()
    {
        levelDropdown.ClearOptions();

        List<string> levelNames = new List<string>();
        foreach (var level in multiLevelData.levels)
        {
            levelNames.Add(level.levelName);
        }

        levelDropdown.AddOptions(levelNames);
        levelDropdown.onValueChanged.RemoveAllListeners();
        levelDropdown.onValueChanged.AddListener(OnDropdownLevelSelected);
    }

    private void OnDropdownLevelSelected(int index)
    {
        Debug.Log($"OnDropdownLevelSelected: {index}");
        if (index < 0 || index >= multiLevelData.levels.Count)
        {
            Debug.LogError("Invalid level selected.");
            return;
        }

        int selectedLevelId = multiLevelData.levels[index].id;
        EditExistingLevel(selectedLevelId);
    }

    public void EditExistingLevel(int levelId)
    {
        ClearBuildingZone();
        currentEditingLevel = multiLevelData.levels.Find(level => level.id == levelId);

        if (currentEditingLevel == null)
        {
            DisplayMessage("Level not found!", true);
            return;
        }

        foreach (var element in currentEditingLevel.elements)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{element.elementType}");
            if (prefab)
            {
                GameObject instance = Instantiate(prefab, buildingZone);
                instance.transform.localPosition = new Vector3(
                    element.position.x,
                    element.position.y,
                    element.position.z
                );
                instance.transform.localScale = new Vector3(
                    element.size.x,
                    element.size.y,
                    element.size.z
                );
                instance.transform.localRotation = Quaternion.Euler(new Vector3(
                    element.rotation.x,
                    element.rotation.y,
                    element.rotation.z
                ));
            }
        }
    }

    public void SaveCurrentLevel()
    {
        if (currentEditingLevel == null)
        {
            DisplayMessage("No level currently being edited!", true);
            return;
        }

        if (levelNameInputField != null && !string.IsNullOrWhiteSpace(levelNameInputField.text))
        {
            currentEditingLevel.levelName = levelNameInputField.text.Trim();
        }

        currentEditingLevel.elements = GetElementsFromBuildingZone();

        if (currentEditingLevel.elements.Count == 0 || !HasRequiredElements(currentEditingLevel.elements))
        {
            DisplayMessage("Level must have 'Platform Start' and 'Platform End' elements, each with at most one copy.", true);
            return;
        }

        if (!multiLevelData.levels.Contains(currentEditingLevel))
        {
            multiLevelData.levels.Add(currentEditingLevel);
            currentLevelId++;
        }

        SaveToJson();
        DisplayMessage($"Level saved: {currentEditingLevel.levelName}", false);

        ClearBuildingZone();
    }

    private bool HasRequiredElements(List<ElementData> elements)
    {
        bool hasStartPlatform = false;
        bool hasEndPlatform = false;

        foreach (var element in elements)
        {
            if (Regex.IsMatch(element.elementType, @"^Platform Start(?: \(\d+\))?$"))
            {
                if (hasStartPlatform)
                {
                    DisplayMessage("Duplicate 'Platform Start' detected!", true);
                    return false;
                }
                hasStartPlatform = true;
            }
            else if (Regex.IsMatch(element.elementType, @"^Platform End(?: \(\d+\))?$"))
            {
                if (hasEndPlatform)
                {
                    DisplayMessage("Duplicate 'Platform End' detected!", true);
                    return false;
                }
                hasEndPlatform = true;
            }
        }

        if (!hasStartPlatform) DisplayMessage("'Platform Start' not found!", true);
        if (!hasEndPlatform) DisplayMessage("'Platform End' not found!", true);

        return hasStartPlatform && hasEndPlatform;
    }

    public void ExitEditMode()
    {
        Debug.Log("Exiting edit mode...");
        ClearBuildingZone();
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
            string elementType = child.name.Replace("(Clone)", "").Trim();
            elements.Add(new ElementData
            {
                elementType = elementType,
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
                }
            });
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

    private void DisplayMessage(string message, bool isError)
    {
        messageDisplay.color = isError ? Color.red : Color.green;
        messageDisplay.text = message;
    }

    private void ShowBuildingZone()
    {
        buildingZone.gameObject.SetActive(true);
    }
}
