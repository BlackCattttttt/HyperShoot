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
        Database.SaveData();
        LoadingManager.Instance.LoadScene(SCENE_INDEX.Gameplay, () => PlayScreen.Show());
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }
    public void ButtonContinue()
    {
        //  PlayScreen.Show(false);
        LoadingManager.Instance.LoadScene(SCENE_INDEX.Gameplay, () => PlayScreen.Show());
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }
    public void ButtonSetting()
    {
        //PopupSetting.Show();
    }
    public void QuitButton()
    {
        Application.Quit();
    }
}
