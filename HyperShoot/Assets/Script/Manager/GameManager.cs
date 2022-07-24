using System.Collections;
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

            Data.Level = 1;
            Data.CurrentMissonIndex = 0;
            Data.PlayerPosition = new Vector3(457.64f, 28.25f, 123.2f);
            Data.isLoadPosition = false;
            Data.Sound = true;

            Data.Buttons = new Dictionary<string, KeyCode>();

            Data.Buttons.Add("Forward", KeyCode.W);
            Data.Buttons.Add("Back", KeyCode.S);
            Data.Buttons.Add("Left", KeyCode.A);
            Data.Buttons.Add("Right", KeyCode.D);
            Data.Buttons.Add("Attack", KeyCode.Mouse0);
            Data.Buttons.Add("Zoom", KeyCode.Mouse1);
            Data.Buttons.Add("Run", KeyCode.LeftShift);
            Data.Buttons.Add("Crouch", KeyCode.C);
            Data.Buttons.Add("Jump", KeyCode.Space);
            Data.Buttons.Add("Reload", KeyCode.R);
            Data.Buttons.Add("Misson", KeyCode.M);
            Data.Buttons.Add("Pause", KeyCode.P);

            Database.SaveData();
        }
        SplashManager.Instance.Load();
        // Application.targetFrameRate = 60;
    }
}
