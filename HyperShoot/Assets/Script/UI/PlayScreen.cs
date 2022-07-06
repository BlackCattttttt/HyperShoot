using HyperShoot.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using DG.Tweening;
using static HyperShoot.Manager.MissonData;

public class PlayScreen : UIPanel
{
    [SerializeField] private MissonData missonData;
    [SerializeField] private GameObject missonBackground;
    [SerializeField] private GameObject noMissonBackground;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text count, pickUp;

    private MissonAtribute currenMisson;
    private float timeRemaining = 10;
    private bool timerIsRunning;

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

        missonBackground.SetActive(false);
        noMissonBackground.SetActive(true);
    }
    private void Update()
    {

        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                StopCountDown(true);
            }
        }
    }
    public void SetMisson(MissonAtribute misson)
    {
        noMissonBackground.SetActive(false);
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
        else if (misson.skillType == MissonAtribute.MissonType.SURVIVAL)
        {
            var m = misson.misson as SurvivalMisson;
            StartCountDown(m.TimeSurvival);
        }
    }
    public void StartCountDown(float s)
    {
        timeRemaining = s;
        timerIsRunning = true;
    }
    public void StopCountDown(bool b)
    {
        timeRemaining = 0;
        timerIsRunning = false;
    }
    public void DisplayTime(float timeToDisplay)
    {
        count.text = string.Format("{0:00}", timeToDisplay);
    }
    public void NoMisson()
    {
        noMissonBackground.SetActive(true);
        missonBackground.SetActive(false);
    }
    public void Updatecount(int current, int max)
    {
        count.text = current.ToString() + "/" + max.ToString();
    }
    public void PickUpItem(string msg)
    {
        pickUp.text = msg;
        pickUp.color = new Color(1, 1, 1, 1);
        pickUp.gameObject.SetActive(true);
        pickUp.DOColor(new Color(1, 1, 1, 0), 1).SetEase(Ease.Linear).OnComplete(() =>
        {
            pickUp.gameObject.SetActive(false);
        });
    }
}
