using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData 
{
    public int Level = 0;
    public int Gold = 0;

    public string playerName;
    public bool Vibrate;
    public bool Sound;

    public bool NoAds;
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
}
