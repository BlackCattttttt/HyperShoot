using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreen : UIPanel
{
    public override UI_PANEL GetID()
    {
        return UI_PANEL.WinScreen;
    }

    public static WinScreen Instance;

    public static void Show()
    {
        WinScreen newInstance = (WinScreen)GUIManager.Instance.NewPanel(UI_PANEL.WinScreen);
        Instance = newInstance;
        newInstance.OnAppear();
    }
    public void OnAppear()
    {
        if (isInited)
            return;

        base.OnAppear();
    }
    public void ButtonRePlay()
    {
        //  PlayScreen.Show(false);
        //LoadingManager.Instance.LoadScene(SCENE_INDEX.Gameplay, () => PlayScreen.Show());
        //  EvenGlobalManager.Instance.OnLoadLevel.Dispatch();
    }
    public void ButtonMain()
    {
        //  PlayScreen.Show(false);
        LoadingManager.Instance.LoadScene(SCENE_INDEX.Main, () => MainScreen.Show());
        //  EvenGlobalManager.Instance.OnLoadLevel.Dispatch();
    }
}
