using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    public int Level = 0;
    public int CurrentMissonIndex = 2;
    public Vector3 PlayerPosition;
    public bool isLoadPosition;
    public bool Sound;
    public Dictionary<string, KeyCode> Buttons = new Dictionary<string, KeyCode>();
}
public class Database
{
    public static void SaveData()
    {
        string dataString = JsonConvert.SerializeObject(GameManager.Instance.Data);
        PlayerPrefs.SetString("GameData", dataString);
        PlayerPrefs.Save();
    }

    public static GameData LoadData()
    {
        if (!PlayerPrefs.HasKey("GameData"))
            return null;

        return JsonConvert.DeserializeObject<GameData>(PlayerPrefs.GetString("GameData"));
    }
    public static void ClearData()
    {
        PlayerPrefs.DeleteKey("GameData");
    }
}
