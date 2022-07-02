using HyperShoot.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using static HyperShoot.Manager.MissonData;

public class PlayScreen : UIPanel
{
    [SerializeField] private MissonData missonData;
    [SerializeField] private GameObject missonBackground;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text count;

    private GamePlayController gamePlayController;
    private MissonAtribute currenMisson;

    public override UI_PANEL GetID()
    {
        return UI_PANEL.PlayScreen;
    }

    public static PlayScreen Instance;

    public static void Show()
    {
        PlayScreen newInstance = (PlayScreen)GUIManager.Instance.NewPanel(UI_PANEL.PlayScreen);
        Instance = newInstance;
        newInstance.OnAppear();
    }
    public void OnAppear()
    {
        if (isInited)
            return;

        base.OnAppear();

        gamePlayController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GamePlayController>();
        missonBackground.SetActive(false);
    }
    public void SetMisson(MissonAtribute misson)
    {
        missonBackground.SetActive(true);
        iconImage.sprite = misson.icon;
        currenMisson = misson;
        if (misson.skillType == MissonAtribute.MissonType.KILL_ENEMY)
        {
            var m = misson.misson as KillEnemyMisson;
            count.text = m.CurrentSkill.ToString() + "/" + m.NumberOfSkill.ToString();
        }
        else if (misson.skillType == MissonAtribute.MissonType.COLLECT)
        {
            var m = misson.misson as CollectMisson;
            count.text = m.CurrentCollect.ToString() + "/" + m.NumberOfCollect.ToString();
        }
        else if (misson.skillType == MissonAtribute.MissonType.FIND)
        {
            var m = misson.misson as FindMisson;
            count.text = "0/1";
        }
    }
    public void Updatecount(int current, int max)
    {
        count.text = current.ToString() + "/" + max.ToString();
    }
}
