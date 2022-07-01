﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public GameData Data;

    // Start is called before the first frame update
    void Start()
    {
        Data = Database.LoadData();
        if (Data == null)
        {
            Data = new GameData();

            Data.Vibrate = true;
            Data.Sound = true;
            Data.NoAds = false;
            Data.playerName = "Player";
            Database.SaveData();
        }
        SplashManager.Instance.Load();
        // Application.targetFrameRate = 60;
    }
}
