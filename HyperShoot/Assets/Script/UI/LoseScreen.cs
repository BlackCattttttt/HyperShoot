using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseScreen : UIPanel
{
    public override UI_PANEL GetID()
    {
        return UI_PANEL.LoseScreen;
    }

    public static LoseScreen Instance;

    public static void Show()
    {
        LoseScreen newInstance = (LoseScreen)GUIManager.Instance.NewPanel(UI_PANEL.LoseScreen);
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
        if (GameManager.Instance.Data.Level == 1)
            LoadingManager.Instance.LoadScene(SCENE_INDEX.Level1, () => PlayScreen.Show());
        else if (GameManager.Instance.Data.Level == 2)
            LoadingManager.Instance.LoadScene(SCENE_INDEX.Level2, () => PlayScreen.Show());
        //  EvenGlobalManager.Instance.OnLoadLevel.Dispatch();
    }
    public void ButtonMain()
    {
        //  PlayScreen.Show(false);
        LoadingManager.Instance.LoadScene(SCENE_INDEX.Main, () => MainScreen.Show());
        //  EvenGlobalManager.Instance.OnLoadLevel.Dispatch();
    }
}
