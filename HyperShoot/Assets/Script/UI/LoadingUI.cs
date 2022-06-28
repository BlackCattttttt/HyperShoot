using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public TMP_Text loadingText;

    private int x = 0;

    private void OnEnable()
    {
        x = 0;
        InvokeRepeating(nameof(ChangeText), 0.5f, 0.5f);
    }
    public void ChangeText()
    {
        x++;
        if (x >= 3) x = 0;
        switch (x)
        {
            case 0:
                loadingText.text = "Loading.";
                break;
            case 1:
                loadingText.text = "Loading..";
                break;
            case 2:
                loadingText.text = "Loading...";
                break;
        }
    }
    private void OnDisable()
    {
        CancelInvoke();
    }
}
