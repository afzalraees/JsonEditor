using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableTemplate
{
    public string name;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public string color; // Hexadecimal color
    public bool isEditing = true; // Flag to track editing mode
}

[System.Serializable]
public class SerializableTemplateList
{
    public SerializableTemplate[] list;
}