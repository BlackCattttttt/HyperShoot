using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperShoot.Manager;

public class MainScreen : UIPanel
{
    public TMP_Text coinText;
    public GameObject adsBtn;
    public TMP_InputField playerName;

    private GamePlayController gamePlayController;

    private DateTime timeNoAds;
    private bool isFirst = true;

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
        gamePlayController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GamePlayController>();
        coinText.text = GameManager.Instance.Data.Gold.ToString();
    }
    public void ButtonPlay()
    {
     
      //  PlayScreen.Show(false);
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }

    public void ButtonPlayAds()
    {
       OnCompleteAdsHuggy(1);
    }
    public void OnCompleteAdsHuggy(int res)
    {
       // PlayScreen.Show(false);
        EvenGlobalManager.Instance.OnStartPlay.Dispatch();
    }

    public void ButtonSetting()
    {
        //PopupSetting.Show();
    }
}
