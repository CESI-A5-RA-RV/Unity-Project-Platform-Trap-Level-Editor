using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class LevelEditorManager : MonoBehaviour
{
    public Transform buildingZone;
    public GameObject uiPanel;
    public TMP_InputField levelNameInputField;
    public TMP_Dropdown levelDropdown;
    public Button exitEditModeButton;
    public RectTransform movingElement;
    public TMP_Text messageDisplay;

    private string savePath;
    public MultiLevelData multiLevelData = new MultiLevelData();

    private int currentLevelId = 0;
    private LevelData currentEditingLevel = null;

    private Vector2 movingElementOriginalPosition;

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

        movingElementOriginalPosition = movingElement.anchoredPosition;

        exitEditModeButton.onClick.AddListener(ExitEditMode);
        HideUI();
    }

    private void ShowBuildingZone()
    {
        buildingZone.gameObject.SetActive(true);
    }

    private void HideBuildingZone()
    {
        ClearBuildingZone();
        buildingZone.gameObject.SetActive(false);
    }

    private void ShowUI(string defaultName)
    {
        uiPanel.SetActive(true);

        levelNameInputField.text = "";
        levelNameInputField.placeholder.GetComponent<TMP_Text>().text = defaultName;

        movingElement.anchoredPosition = movingElementOriginalPosition + new Vector2(0, 127);

        // Clear the message display
        messageDisplay.text = "";
    }

    private void HideUI()
    {
        uiPanel.SetActive(false);
        NonNativeKeyboard.Instance.Close();
        movingElement.anchoredPosition = movingElementOriginalPosition;
    }

    public void CreateNewLevel()
    {
        levelNameInputField.gameObject.SetActive(true);
        levelDropdown.gameObject.SetActive(false);

        string defaultName = $"Level ({currentLevelId + 1})";
        ShowUI(defaultName);

        // Update exit button text
        exitEditModeButton.GetComponentInChildren<TMP_Text>().text = "Exit Creation Mode";

        ShowBuildingZone();

        currentEditingLevel = new LevelData
        {
            id = currentLevelId,
            levelName = defaultName,
            elements = new List<ElementData>()
        };

        Debug.Log("Building zone activated for new level.");
    }

    public void EditExistingLevelMode()
    {
        levelNameInputField.gameObject.SetActive(false);
        levelDropdown.gameObject.SetActive(true); // Show dropdown

        PopulateDropdownWithLevels(); // Populate dropdown with level names

        ShowUI("Select a level to edit");
        exitEditModeButton.GetComponentInChildren<TMP_Text>().text = "Exit Edit Mode";

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
        Debug.Log($"EditExistingLevel: {levelId}");
        ClearBuildingZone();

        currentEditingLevel = multiLevelData.levels.Find(level => level.id == levelId);

        if (currentEditingLevel == null)
        {
            Debug.LogError("Level not found!");
            return;
        }

        ShowUI(currentEditingLevel.levelName);

        foreach (var element in currentEditingLevel.elements)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Traps/{element.elementType}") ??
                                Resources.Load<GameObject>($"Prefabs/Platforms/{element.elementType}");

            if (prefab)
            {
                GameObject instance = Instantiate(prefab, buildingZone);
                instance.transform.localPosition = new Vector3(element.position.x, element.position.y, element.position.z);
                instance.transform.localScale = new Vector3(element.size.x, element.size.y, element.size.z);
                instance.transform.localRotation = Quaternion.Euler(new Vector3(element.rotation.x, element.rotation.y, element.rotation.z));
            }
            else
            {
                DisplayMessage($"Prefab not found for {element.elementType}", true);
            }
        }

        Debug.Log($"Building zone activated for editing level: {currentEditingLevel.levelName}");
    }

    public void SaveCurrentLevel()
    {
        if (levelNameInputField != null && !string.IsNullOrWhiteSpace(levelNameInputField.text))
        {
            currentEditingLevel.levelName = levelNameInputField.text.Trim();
        }

        currentEditingLevel.elements = GetElementsFromBuildingZone();

        if (currentEditingLevel == null)
        {
            DisplayMessage("No level currently being edited!", true);
            return;
        }

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

        HideUI();
        HideBuildingZone();
    }

    private void ExitEditMode()
    {
        Debug.Log("Exiting edit mode...");
        HideBuildingZone();
        HideUI();
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

    private void DisplayMessage(string message, bool isError)
    {
        messageDisplay.color = isError ? Color.red : Color.green;
        messageDisplay.text = message;
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
}
