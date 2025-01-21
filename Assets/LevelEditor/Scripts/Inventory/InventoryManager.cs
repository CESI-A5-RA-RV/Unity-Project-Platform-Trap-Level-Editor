using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

public class InventoryManager : MonoBehaviour
{
    public GameObject content; // Content GameObject in the ScrollView
    public Button platformsButton;
    public Button trapsButton;
    public GameObject inventorySlotPrefab; // Assign your InventorySlot prefab in the Inspector
    public XRRayInteractor rayInteractor; // The ray interactor from the controller
    public XRRayInteractor inventoryRayInteractor;
    public string iconSavePath = "Assets/Icons"; // Path to save thumbnails

    void Start()
    {
        platformsButton.onClick.AddListener(() => LoadElementsByFolder("Platforms"));
        trapsButton.onClick.AddListener(() => LoadElementsByFolder("Traps"));

        LoadElementsByFolder("Platforms"); // Default
    }

    public void LoadElementsByFolder(string folder)
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        // Load prefabs from the specified folder
        string path = $"Prefabs/{folder}";
        GameObject[] prefabs = Resources.LoadAll<GameObject>(path);

        foreach (GameObject prefab in prefabs)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, content.transform);

            DragAndDropHandler dragAndDropHandler = slot.GetComponent<DragAndDropHandler>();
            if (dragAndDropHandler != null)
            {
                dragAndDropHandler.prefab = prefab;
            }

            TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = prefab.name;
            }

            Image image = slot.transform.Find("Thumbnail")?.GetComponentInChildren<Image>();
            if (image != null)
            {
                // Check if a thumbnail already exists
                string iconPath = Path.Combine(iconSavePath, $"{prefab.name}.png");
                if (!File.Exists(iconPath))
                {
                    GenerateAndSaveThumbnail(prefab, iconPath);
                }

                if (File.Exists(iconPath))
                {
                    byte[] fileData = File.ReadAllBytes(iconPath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    image.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Thumbnail not found for {prefab.name}");
                }
            }
        }
    }

    private void GenerateAndSaveThumbnail(GameObject prefab, string savePath)
    {
#if UNITY_EDITOR
        Texture2D icon = AssetPreview.GetAssetPreview(prefab);

        if (icon == null)
        {
            int retryCount = 3;
            while (icon == null && retryCount > 0)
            {
                --retryCount;
                icon = (Texture2D)AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(prefab)));
                System.Threading.Thread.Sleep(300);
            }
        }

        if (icon != null)
        {
            if (!Directory.Exists(iconSavePath))
            {
                Directory.CreateDirectory(iconSavePath);
            }

            byte[] pngData = icon.EncodeToPNG();
            File.WriteAllBytes(savePath, pngData);
            Debug.Log($"Thumbnail saved: {savePath}");
        }
        else
        {
            Debug.LogWarning($"Could not generate thumbnail for {prefab.name}");
        }
#endif
    }
}
