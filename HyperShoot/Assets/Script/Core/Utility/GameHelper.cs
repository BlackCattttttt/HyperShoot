using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameHelper
{
    public static PANEL_TYPE GetPanelType(this string key)
    {
        if (key.Contains("Screen"))
            return PANEL_TYPE.SCREEN;
        else if (key.Contains("Popup"))
            return PANEL_TYPE.POPUP;
        else if (key.Contains("Noti"))
            return PANEL_TYPE.NOTIFICATION;

        return PANEL_TYPE.NONE;
    }

    public static void SetActive(this GameObject[] list, bool on)
    {
        if (list == null || list.Length == 0)
            return;

        foreach (var gameObject in list)
        {
            gameObject.SetActive(on);
        }
    }

    public static void SetActive(this List<GameObject> list, bool on)
    {
        if (list == null || list.Count == 0)
            return;

        foreach (var gameObject in list)
        {
            gameObject.SetActive(on);
        }
    }

    public static void SetChildActive(this Transform parent, bool on, int exception = -1)
    {
        if (parent == null || parent.childCount == 0)
            return;

        for (int i = 0; i < parent.childCount; i++)
        {
            if (i == exception)
                parent.GetChild(i).gameObject.SetActive(!on);
            else
                parent.GetChild(i).gameObject.SetActive(on);
        }
    }
}