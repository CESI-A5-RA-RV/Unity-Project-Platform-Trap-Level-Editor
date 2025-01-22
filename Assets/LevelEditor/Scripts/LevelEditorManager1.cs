using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class LevelEditorManager1 : MonoBehaviour
{
    public GameObject createLevelPanel;
    public GameObject editLevelPanel;
    public GameObject previewLevelPanel;

    public Button createLevelButton;
    public Button editLevelButton;
    public Button previewLevelButton;

    public TMP_Text messageDisplay;
    public GameObject validationPanel;
    public TMP_Text validationMessage;
    public Button validationConfirmButton;
    public Button validationCancelButton;

    private GameObject previousPanel;
    private GameObject currentPanel;
    private GameObject targetPanel;
    private Button targetButton;

    private string currentMode = "";

    public Transform buildingZone;
    private TMP_InputField levelNameInputField;
    private TMP_Dropdown levelDropdown;

    private string savePath;

    public MultiLevelData multiLevelData = new MultiLevelData();

    private int currentLevelId = 0;
    private LevelData currentEditingLevel = null;

    private Dictionary<GameObject, Button> panelButtonMap;

    void Start()
    {
        panelButtonMap = new Dictionary<GameObject, Button>
        {
            { createLevelPanel, createLevelButton },
            { editLevelPanel, editLevelButton },
            { previewLevelPanel, previewLevelButton }
        };

        createLevelButton.onClick.AddListener(() => OnPanelSwitchRequested(createLevelPanel, createLevelButton, "creating"));
        editLevelButton.onClick.AddListener(() => OnPanelSwitchRequested(editLevelPanel, editLevelButton, "editing"));
        previewLevelButton.onClick.AddListener(() => ActivatePanel(previewLevelPanel, previewLevelButton));

        validationConfirmButton.onClick.AddListener(ConfirmPanelSwitch);
        validationCancelButton.onClick.AddListener(CancelPanelSwitch);

        currentPanel = null;
        targetPanel = null;
        targetButton = null;

        DeactivateAllPanels();

        levelNameInputField = createLevelPanel.GetComponentInChildren<TMP_InputField>();
        levelDropdown = editLevelPanel.GetComponentInChildren<TMP_Dropdown>();

        savePath = Path.Combine(Application.persistentDataPath, "levels.json");

        string directoryPath = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        multiLevelData.levels = new List<LevelData>();
        ShowBuildingZone();
        LoadLevelsFromJson();

        ActivatePanel(createLevelPanel, createLevelButton);
        createLevelButton.onClick.Invoke();
    }

    public void CreateNewLevel()
    {
        currentPanel = createLevelPanel;
        string defaultName = $"Level ({currentLevelId + 1})";

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
        PopulateDropdownWithLevels();

        ShowBuildingZone();
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

        foreach (var element in currentEditingLevel.elements)
        {
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Traps/{element.elementType}") ??
                                Resources.Load<GameObject>($"Prefabs/Platforms/{element.elementType}");

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
            else
            {
                DisplayMessage($"Prefab not found for {element.elementType}", true);
            }
        }
        Debug.Log($"Building zone activated for editing level: {currentEditingLevel.levelName}");
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

        HideBuildingZone();
        DeactivateAllPanels();
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
                    parameters = GetParametersFromComponent(child.gameObject)
                };
                elements.Add(elementData);
            }
        }
        return elements;
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

    private List<Parameter> GetParametersFromComponent(GameObject gameObject)
    {
        var parameters = new List<Parameter>();
        var components = gameObject.GetComponents<MonoBehaviour>();

        foreach (var component in components)
        {
            if (component != null && component.GetType().IsSubclassOf(typeof(LevelElement)))
            {
                var fields = component.GetType().GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(component);
                    parameters.Add(new Parameter
                    {
                        key = field.Name,
                        value = value != null ? float.Parse(value.ToString()) : 0f
                });
                }
            }
        }

        return parameters;
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

    private void ShowBuildingZone()
    {
        buildingZone.gameObject.SetActive(true);
    }

    private void HideBuildingZone()
    {
        ClearBuildingZone();
    }

    private void PopulateDropdownWithLevels()
    {
        levelDropdown.ClearOptions();

        List<string> levelNames = new List<string> { "Choose your level" };

        foreach (var level in multiLevelData.levels)
        {
            levelNames.Add(level.levelName);
        }

        levelDropdown.AddOptions(levelNames);

        levelDropdown.value = 0;

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

    public void ExitEditMode()
    {
        Debug.Log("Exiting edit mode...");
        HideBuildingZone();
        ClearBuildingZone();
        DeactivateAllPanels();
    }

    public void DisplayMessage(string message, bool isError)
    {
        messageDisplay.color = isError ? Color.red : Color.green;
        messageDisplay.text = message;
    }

    private void OnPanelSwitchRequested(GameObject panel, Button button, string mode)
    {
        Debug.Log($"previous: {previousPanel}, current: {currentPanel}");
        if (currentMode == mode || previousPanel == null)
        {
            ActivatePanel(panel, button);
            return;
        }

        validationPanel.SetActive(true);
        validationMessage.text = $"You are currently {currentMode} a level. If you change without saving, you will lose your changes. Are you sure that you want to proceed?";

        targetPanel = panel;
        targetButton = button;
    }

    private void ConfirmPanelSwitch()
    {
        validationPanel.SetActive(false);

        if (targetPanel != null && targetButton != null)
        {
            ActivatePanel(targetPanel, targetButton);

            currentMode = (targetPanel == createLevelPanel) ? "creating" : "editing";

            targetPanel = null;
            targetButton = null;
        }
    }

    private void CancelPanelSwitch()
    {
        validationPanel.SetActive(false);
        DisplayMessage("", false);
        targetPanel = null;
        targetButton = null;
    }

    private void ActivatePanel(GameObject panel, Button button)
    {
        DisplayMessage($"", false);
        DeactivateAllPanels();
        previousPanel = currentPanel;
        currentPanel = panel;

        panel.SetActive(true);

        // Update the current mode based on the panel being activated
        if (panel == createLevelPanel)
        {
            currentMode = "creating";
        }
        else if (panel == editLevelPanel)
        {
            currentMode = "editing";
        }
        else if (panel == previewLevelPanel)
        {
            currentMode = "previewing";
            // Display the message for preview mode
            DisplayMessage($"You are currently in {currentMode} mode.", false);
        }

        button.interactable = false;
        panelButtonMap[panel].interactable = false;

        Debug.Log($"Panel {panel.name} activated.");
    }

    private void DeactivateAllPanels()
    {
        createLevelPanel.SetActive(false);
        editLevelPanel.SetActive(false);
        previewLevelPanel.SetActive(false);

        createLevelButton.interactable = true;
        editLevelButton.interactable = true;
        previewLevelButton.interactable = true;
    }

    public void GoBackToPreviousPanel()
    {
        if (previousPanel != null)
        {
            ActivatePanel(previousPanel, panelButtonMap[previousPanel]);
        }
        else
        {
            Debug.LogWarning("No previous panel to go back to.");
        }
    }
}
