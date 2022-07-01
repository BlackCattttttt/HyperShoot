using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperShoot.Manager;

public class MainScreen : UIPanel
{
    private GamePlayController gamePlayController;

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
        LoadingManager.Instance.LoadScene(SCENE_INDEX.Gameplay, () => Close());
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }

    public void ButtonSetting()
    {
        //PopupSetting.Show();
    }
}
