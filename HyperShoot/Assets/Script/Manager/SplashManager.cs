using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public static SplashManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        GUIManager.Instance.Init();
        //StartCoroutine(ToMainScreen());
        // Load();
    }
    public void Load()
    {
        LoadingManager.Instance.LoadScene(SCENE_INDEX.Gameplay, () => MainScreen.Show());
    }
}
