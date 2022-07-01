using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIManager : Singleton<GUIManager>
{
    [SerializeField] private Canvas Root;
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Canvas LayerScreen;
    [SerializeField] private Canvas LayerPopup;
    [SerializeField] private Canvas LayerNotify;
    [SerializeField] private PanelInstance Prefabs;

    private Dictionary<UI_PANEL, UIPanel> initiedPanels = new Dictionary<UI_PANEL, UIPanel>();

    private List<UIPanel> showingPopups = new List<UIPanel>();
    private List<UIPanel> showingNotifications = new List<UIPanel>();

    private Queue<Action> QueuePopup = new Queue<Action>();
    private Stack<UIPanel> ScreenStack = new Stack<UIPanel>();

    protected override void Awake()
    {
        DestroyChildren(LayerScreen.transform);
        DestroyChildren(LayerPopup.transform);
        DestroyChildren(LayerNotify.transform);

        //if (Context.isInit)
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        base.Awake();
    }

    public void ReloadCamera()
    {
        if (MainCamera == null)
            MainCamera = Camera.main;
        Root.worldCamera = MainCamera;
    }
    public void Init()
    {
#if UNITY_EDITOR
        Application.runInBackground = true;
#endif

        DestroyChildren(LayerScreen.transform);
        DestroyChildren(LayerPopup.transform);
        DestroyChildren(LayerNotify.transform);
        ReloadCamera();

        EvenGlobalManager.Instance.OnStartLoadScene.AddListener(StartLoading);
        EvenGlobalManager.Instance.OnFinishLoadScene.AddListener(ReloadCamera);
    }
    public UIPanel NewPanel(UI_PANEL id)
    {
        PANEL_TYPE type = id.ToString().GetPanelType();

        UIPanel newPanel = null;
        if (initiedPanels.ContainsKey(id))
            newPanel = initiedPanels[id];
        else
        {
            newPanel = Instantiate(GetPrefab(id), GetRootByType(type).transform);
            initiedPanels.Add(id, newPanel);
        }

        if (type == PANEL_TYPE.POPUP)
        {
            if (showingPopups.Contains(newPanel))
                showingPopups.Remove(newPanel);
            if (type == PANEL_TYPE.POPUP)
                showingPopups.Add(newPanel);
        }
        else if (type == PANEL_TYPE.NOTIFICATION)
        {
            if (!showingNotifications.Contains(newPanel))
                showingNotifications.Add(newPanel);
        }
        else
        {
            if (GetCurrentScreen() != null && GetCurrentScreen().gameObject.activeSelf)
                GetCurrentScreen().Close(false);

            if (ScreenStack.Contains(newPanel))
                ScreenStack = MakeElementToTopStack(newPanel, ScreenStack);
            else
                ScreenStack.Push(newPanel);
        }

        newPanel.transform.SetAsLastSibling();
        newPanel.gameObject.SetActive(true);

        return newPanel;
    }
    public UIPanel GetCurrentScreen()
    {
        if (ScreenStack.Count == 0)
            return null;

        return ScreenStack.Peek();
    }
    public void Dismiss(UIPanel panel)
    {
        showingPopups.Remove(panel);
        showingNotifications.Remove(panel);
    }
    public void CheckPopupQueue()
    {
        if (showingPopups.Count == 0 && QueuePopup.Count > 0)
        {
            if (QueuePopup.Peek() != null)
                QueuePopup.Dequeue().Invoke();
        }
    }
    #region Utilities

    public UIPanel GetTopPopup()
    {
        if (showingPopups.Count == 0)
            return null;

        return showingPopups.GetLast();
    }

    UIPanel GetPrefab(UI_PANEL id)
    {
        if (Prefabs == null)
            return null;

        return Prefabs.Instances.FindLast(e => e.GetID().Equals(id));
    }

    Canvas GetRootByType(PANEL_TYPE type)
    {
        switch (type)
        {
            case PANEL_TYPE.SCREEN:
                return LayerScreen;
            case PANEL_TYPE.POPUP:
                return LayerPopup;
            case PANEL_TYPE.NOTIFICATION:
                return LayerNotify;
            //case PANEL_TYPE.LOADING:
            //    return LayerLoading;
        }

        return null;
    }
    public void DestroyChildren(Transform transform)
    {
        int totalChild = transform.childCount;

        if (totalChild == 0)
            return;

        for (int i = totalChild - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public UIPanel GetPanel(UI_PANEL type)
    {
        if (initiedPanels.ContainsKey(type))
        {
            return initiedPanels[type];
        }

        return null;
    }

    public Stack<UIPanel> MakeElementToTopStack(UIPanel objectTop, Stack<UIPanel> stack)
    {
        UIPanel[] extraPanel = stack.ToArray();
        for (int i = 0; i < extraPanel.Length; i++)
        {
            if (extraPanel[i] == objectTop)
            {
                for (int ii = i; ii > 0; ii--)
                {
                    extraPanel[ii] = extraPanel[ii - 1];
                }

                extraPanel[0] = objectTop;
            }
        }

        Array.Reverse(extraPanel);
        return new Stack<UIPanel>(extraPanel);
    }
    #endregion
    #region Loading
    public LoadingUI LoadingUI;

    public void StartLoading()
    {
        LoadingUI.gameObject.SetActive(true);
    }

    public void EndLoading()
    {
        LoadingUI.gameObject.SetActive(false);
    }
    #endregion
}
