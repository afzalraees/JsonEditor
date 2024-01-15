using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.UI;
using Codice.Utils;

public class JSONEditorWindow : EditorWindow
{
    private SerializableTemplateList templateList = new SerializableTemplateList();
    private string jsonFilePath = "";
    private Vector2 scrollPosition;

    [MenuItem("Window/JSON Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<JSONEditorWindow>("JSON Template Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Manage JSON Templates", EditorStyles.boldLabel);

        if (GUILayout.Button("Add New Template"))
        {
            AddNewTemplate();
        }

        // Scroll view for templates
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (templateList.list != null)
        {
            for (int i = 0; i < templateList.list.Length; i++)
            {
                var template = templateList.list[i];
                if (template.isEditing)
                {
                    DrawTemplateUI(template, i);
                }
                else
                {
                    DrawTemplateView(template, i);
                }
            }
        }

        GUILayout.Space(10);    

        if (GUILayout.Button("Load JSON"))
        {
            LoadJSON();
        }

        if (GUILayout.Button("Save JSON"))
        {
            SaveJSON();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawTemplateUI(SerializableTemplate template, int index)
    {
        GUILayout.BeginHorizontal();
        template.name = EditorGUILayout.TextField("Name", template.name);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        template.position = EditorGUILayout.Vector3Field("Position", template.position);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        template.rotation = EditorGUILayout.Vector3Field("Rotation", template.rotation);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        template.scale = EditorGUILayout.Vector3Field("Scale", template.scale);
        GUILayout.EndHorizontal();
    
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Color:");
        Color currentColor = HexToColor(template.color);
        Color newColor = EditorGUILayout.ColorField(currentColor);
        if (newColor != currentColor)
        {
            template.color = ColorToHex(newColor);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Remove"))
        {
            RemoveTemplateAtIndex(index);
        }

        if (GUILayout.Button("Save and View"))
        {
            template.isEditing = false;
            SaveJSON(); // Save changes to the JSON file
        }
    }

    private void DrawTemplateView(SerializableTemplate template, int index)
    {
        GUILayout.Label("Template: " + template.name);

        if (GUILayout.Button("Instantiate"))
        {
            InstantiateUIElement(template);
        }

        if (GUILayout.Button("Remove"))
        {
            RemoveTemplateAtIndex(index);
        }

        if (GUILayout.Button("Edit"))
        {
            template.isEditing = true;
        }
    }

    private void RemoveTemplateAtIndex(int index)
    {
        if (index >= 0 && index < templateList.list.Length)
        {
            List<SerializableTemplate> list = new List<SerializableTemplate>(templateList.list);
            list.RemoveAt(index);
            templateList.list = list.ToArray();
            SaveJSON(); // Save the updated list to the JSON
        }
    }

    private void LoadJSON()
    {
        string path = EditorUtility.OpenFilePanel("Load JSON File", "", "json");
        if (path.Length != 0)
        {
            jsonFilePath = path;
            string jsonData = File.ReadAllText(path);
            templateList = JsonUtility.FromJson<SerializableTemplateList>(jsonData);
            for (int i = 0; i < templateList.list.Length; i++)
            {
                DrawTemplateView(templateList.list[i], i);
                Debug.Log(templateList.list[i].name);
            }
        }
    }

    private void SaveJSON()
    {
        if (jsonFilePath == "")
        {
            jsonFilePath = EditorUtility.SaveFilePanel("Save JSON File", "", "TemplateList.json", "json");
            Debug.Log("There is no json file so a new file is created at: " + jsonFilePath);
        }

        if (jsonFilePath.Length != 0)
        {
            string jsonData = JsonUtility.ToJson(templateList, true);
            File.WriteAllText(jsonFilePath, jsonData);
            AssetDatabase.Refresh();
        }
    }

    private void AddNewTemplate()
    {
        List<SerializableTemplate> list = new List<SerializableTemplate>(templateList.list ?? new SerializableTemplate[0]);
        list.Add(new SerializableTemplate());
        templateList.list = list.ToArray();
    }

    private void InstantiateUIElement(SerializableTemplate template)
    {
        GameObject newUIElement = new GameObject(template.name);

        // Assuming you're using Unity's UI system, attach RectTransform and set it as a child of Canvas.
        // If you don't have a Canvas in your scene, you'll need to create one or handle this differently.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        newUIElement.transform.SetParent(canvas.transform, false);
        RectTransform rectTransform = newUIElement.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = template.position;
        rectTransform.localRotation = Quaternion.Euler(template.rotation);
        rectTransform.localScale = template.scale;

        // Add Image component or other UI components as needed.
        Image image = newUIElement.AddComponent<Image>();
        if (ColorUtility.TryParseHtmlString(template.color, out Color color))
        {
            image.color = color;
        }

        // Set other properties as needed.
    }

    private string ColorToHex(Color color)
    {
        Color32 color32 = color;
        return "#" + color32.r.ToString("X2") + color32.g.ToString("X2") + color32.b.ToString("X2");
    }

    private Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }
        return Color.white; // Default color if parsing fails
    }
}