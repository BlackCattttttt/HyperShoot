using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperShoot.Manager;

public class PopupMisson : UIPanel
{
    [SerializeField] private TMP_Text missonText;
    private GamePlayController gamePlayController;
    public override UI_PANEL GetID()
    {
        return UI_PANEL.PopupMisson;
    }
    public static PopupMisson Instance;
    public static void Show()
    {
        PopupMisson newInstance = (PopupMisson)GUIManager.Instance.NewPanel(UI_PANEL.PopupMisson);
        Instance = newInstance;
        newInstance.OnAppear();
    }
    public void OnAppear()
    {
        if (isInited)
            return;

        base.OnAppear();
        gamePlayController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GamePlayController>();
        if (gamePlayController.CurrenMisson != null)
        {
            missonText.text = gamePlayController.CurrenMisson.missonDes;
        }
        else
        {
            missonText.text = "Move on to the next mission";
        }
    }
    public void ClosePopup()
    {
        fp_Utility.LockCursor = true;
        Time.timeScale = 1;
        Close();
    }
}
