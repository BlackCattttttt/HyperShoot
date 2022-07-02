using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LoadingManager : Singleton<LoadingManager>
{
    private LoadingUI loadingUI;

    private Dictionary<SCENE_INDEX, Action> SceneLoadedSpecialAction = new Dictionary<SCENE_INDEX, Action>();

    public void SetSceneAction(SCENE_INDEX index, Action action)
    {
        if (SceneLoadedSpecialAction.ContainsKey(index))
        {
            SceneLoadedSpecialAction[index] = action;
        }
        else
        {
            SceneLoadedSpecialAction.Add(index, action);
        }
    }

    public void LoadScene(SCENE_INDEX type, Action action = null)
    {
        if (loadingUI == null)
        {
            loadingUI = GUIManager.Instance.LoadingUI;
        }
        if (action != null)
        {
            SetSceneAction(type, action);
        }
        EvenGlobalManager.Instance.OnStartLoadScene.Dispatch();
        StartCoroutine(LoadSceneAsync(type));
    }

    IEnumerator LoadSceneAsync(SCENE_INDEX index)
    {
        yield return Yielders.Get(0.25f);
        AsyncOperation async = SceneManager.LoadSceneAsync((int)index);
        async.allowSceneActivation = false;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        while (!async.isDone)
        {
            if (async.progress >= 0.9f)
            {
                async.allowSceneActivation = true;
            }
            yield return null;
        }

        System.GC.Collect(2, GCCollectionMode.Forced);
        Resources.UnloadUnusedAssets();

        if (index == SCENE_INDEX.Gameplay)
            EvenGlobalManager.Instance.OnFinishLoadScene.Dispatch();
        if (SceneLoadedSpecialAction.ContainsKey(index) && SceneLoadedSpecialAction[index] != null)
        {
            SceneLoadedSpecialAction[index].Invoke();
        }

        yield return Yielders.Get(0.5f);
        loadingUI.gameObject.SetActive(false);
    }
}
