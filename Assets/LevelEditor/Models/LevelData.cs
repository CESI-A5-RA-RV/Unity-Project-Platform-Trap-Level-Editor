using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MultiLevelData
{
    public List<LevelData> levels;
}


[System.Serializable]
public class LevelData
{
    public int id;
    public string levelName;
    public List<ElementData> elements;
}

[System.Serializable]
public class ElementData
{
    public int id;
    public string elementType;
    public Vector3Data position;
    public Vector3Data size;
    public Vector3Data rotation;
    public List<Parameter> parameters; // For element-specific data
}

[System.Serializable]
public class Vector3Data
{
    public float x;
    public float y;
    public float z;

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[System.Serializable]
public class Parameter
{
    public string key;
    public float value;
}