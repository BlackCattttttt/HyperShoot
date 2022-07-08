using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSetting : UIPanel
{
    public RectTransform rectTransform;
    public Image soundImg;
    public Sprite soundOnImg;
    public Sprite soundOffImg;
    public override UI_PANEL GetID()
    {
        return UI_PANEL.PopupSetting;
    }

    public static void Show()
    {
        PopupSetting newInstance = (PopupSetting)GUIManager.Instance.NewPanel(UI_PANEL.PopupSetting);
        newInstance.OnAppear();
    }
    public void OnAppear()
    {
        if (isInited)
            return;

        base.OnAppear();
        if (GameManager.Instance.Data.Sound)
        {
            soundImg.sprite = soundOnImg;
        }
        else
        {
            soundImg.sprite = soundOffImg;
        }
    }
    private void Update()
    {
        //if (Input.GetMouseButton(0) && gameObject.activeSelf &&
        //     !RectTransformUtility.RectangleContainsScreenPoint(
        //         rectTransform,
        //         Input.mousePosition,
        //         Camera.main))
        //{
        //    Close();
        //}
    }
    public void ButtonSound()
    {
        if (GameManager.Instance.Data.Sound)
        {
            soundImg.sprite = soundOffImg;
            GameManager.Instance.Data.Sound = false;
            Database.SaveData();
        }
        else
        {
            soundImg.sprite = soundOnImg;
            GameManager.Instance.Data.Sound = true;
            Database.SaveData();
        }
    }
}
