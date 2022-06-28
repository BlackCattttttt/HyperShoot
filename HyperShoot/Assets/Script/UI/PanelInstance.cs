using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

[System.Serializable]
[CreateAssetMenu(fileName = "List UI Panel", menuName = "Panel Instances")]
public class PanelInstance : ScriptableObject
{
    public List<UIPanel> Instances;

#if UNITY_EDITOR
    [FolderPath]
    public string path;
    [Button]
    void GetUI()
    {
        Instances = new List<UIPanel>();
        string[] fileEntries = Directory.GetFiles(path);
        for (int i = 0; i < fileEntries.Length; i++)
        {
            if (fileEntries[i].EndsWith(".prefab"))
            {
                UIPanel ui = AssetDatabase.LoadAssetAtPath<UIPanel>(fileEntries[i].Replace("\\", "/"));
                if (ui != null)
                {
                    Instances.Add(ui);
                }
            }
        }
    }    
#endif
}