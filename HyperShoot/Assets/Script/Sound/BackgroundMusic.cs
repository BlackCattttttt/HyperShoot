using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [SerializeField] List<AudioSource> bg;

    private void Start()
    {
        if (GameManager.Instance.Data.Sound)
        {
            for (int i = 0; i < bg.Count; i++)
            {
                bg[i].mute = false;
            }
        }
        else
        {
            for (int i = 0; i < bg.Count; i++)
            {
                bg[i].mute = true;
            }
        }
    }
}
