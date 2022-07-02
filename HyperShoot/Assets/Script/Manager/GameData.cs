using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    public int Level = 0;
    public int CurrentMissonIndex = 2;
    public bool Sound;
}
public class Database
{
    public static void SaveData()
    {
        string dataString = JsonUtility.ToJson(GameManager.Instance.Data);
        PlayerPrefs.SetString("GameData", dataString);
        PlayerPrefs.Save();
    }

    public static GameData LoadData()
    {
        if (!PlayerPrefs.HasKey("GameData"))
            return null;

        return JsonUtility.FromJson<GameData>(PlayerPrefs.GetString("GameData"));
    }
    public static void ClearData()
    {
        PlayerPrefs.DeleteKey("GameData");
    }
}
