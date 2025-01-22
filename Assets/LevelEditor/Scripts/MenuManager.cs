using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject createLevelPanel;
    public GameObject editLevelPanel;
    public GameObject previewLevelPanel;

    public Button createLevelButton;
    public Button editLevelButton;
    public Button previewLevelButton;

    public GameObject validationPanel;
    public TMP_Text validationMessage;
    public Button validationConfirmButton;
    public Button validationCancelButton;

    private GameObject currentPanel;
    private GameObject targetPanel;
    private Button targetButton;
    private string currentMode;

    private Dictionary<GameObject, Button> panelButtonMap;

    public delegate void OnPanelSwitch(string mode);
    public event OnPanelSwitch PanelSwitchConfirmed;

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
        DeactivateAllPanels();
    }

    private void DeactivateAllPanels()
    {
        foreach (var panel in panelButtonMap.Keys)
        {
            panel.SetActive(false);
        }
    }

    private void OnPanelSwitchRequested(GameObject panel, Button button, string mode)
    {
        if (currentMode == mode)
        {
            ActivatePanel(panel, button);
            return;
        }

        validationPanel.SetActive(true);
        validationMessage.text = $"You are currently {currentMode} a level. Unsaved changes will be lost. Proceed?";
        targetPanel = panel;
        targetButton = button;
    }

    private void ConfirmPanelSwitch()
    {
        validationPanel.SetActive(false);
        ActivatePanel(targetPanel, targetButton);
        currentMode = targetPanel == createLevelPanel ? "creating" : "editing";
        targetPanel = null;
        targetButton = null;

        PanelSwitchConfirmed?.Invoke(currentMode);
    }

    private void CancelPanelSwitch()
    {
        validationPanel.SetActive(false);
        targetPanel = null;
        targetButton = null;
    }

    private void ActivatePanel(GameObject panel, Button associatedButton)
    {
        DeactivateAllPanels();
        panel.SetActive(true);

        foreach (var button in panelButtonMap.Values)
        {
            button.interactable = true;
        }
        associatedButton.interactable = false;
        currentPanel = panel;
    }
}
