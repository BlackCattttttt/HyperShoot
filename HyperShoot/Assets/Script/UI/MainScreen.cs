using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperShoot.Manager;

public class MainScreen : UIPanel
{
    public override UI_PANEL GetID()
    {
        return UI_PANEL.MainScreen;
    }

    public static MainScreen Instance;

    public static void Show()
    {
        MainScreen newInstance = (MainScreen)GUIManager.Instance.NewPanel(UI_PANEL.MainScreen);
        Instance = newInstance;
        newInstance.OnAppear();
    }
    public void OnAppear()
    {
        if (isInited)
            return;

        base.OnAppear();
    }
    public void ButtonPlay()
    {
        //  PlayScreen.Show(false);
        GameManager.Instance.Data.Level = 1;
        GameManager.Instance.Data.CurrentMissonIndex = 0;
        GameManager.Instance.Data.PlayerPosition = new Vector3(457.64f, 28.25f, 123.2f);
        GameManager.Instance.Data.isLoadPosition = false;
        Database.SaveData();
        if (GameManager.Instance.Data.Level == 1)
            LoadingManager.Instance.LoadScene(SCENE_INDEX.Level1, () => PlayScreen.Show());
        else if (GameManager.Instance.Data.Level == 2)
            LoadingManager.Instance.LoadScene(SCENE_INDEX.Level2, () => PlayScreen.Show());
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }
    public void ButtonContinue()
    {
        //  PlayScreen.Show(false);
        if (GameManager.Instance.Data.Level == 1)
            LoadingManager.Instance.LoadScene(SCENE_INDEX.Level1, () => PlayScreen.Show());
        else if (GameManager.Instance.Data.Level == 2)
            LoadingManager.Instance.LoadScene(SCENE_INDEX.Level2, () => PlayScreen.Show());
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }
    public void ButtonSetting()
    {
        PopupSetting.Show();
    }
    public void QuitButton()
    {
        Application.Quit();
    }
}
