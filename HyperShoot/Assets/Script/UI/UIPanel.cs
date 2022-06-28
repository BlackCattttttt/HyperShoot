using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using DG.Tweening;
//using RocketTeam.Sdk.Services.Ads;

public class UIPanel : MonoBehaviour
{
    public virtual UI_PANEL GetID()
    {
        return UI_PANEL.NONE;
    }
    protected GUIManager Gui;
    private void Awake()
    {
        Gui = GUIManager.Instance;
    }
    public bool unscaleTime = false;
    public ANIM_IN animIn = ANIM_IN.NONE;

    [HideIf("animIn", ANIM_IN.NONE)]
    public float animInTime = .5f;
    [HideIf("animIn", ANIM_IN.NONE)]
    public float delayAnimIn = 0f;


    [HideIf("animIn", ANIM_IN.NONE)]
    public UnityEvent animInCompletedEvent;

    public ANIM_OUT animOut = ANIM_OUT.NONE;

    [HideIf("animOut", ANIM_OUT.NONE)]
    public float animOutTime = .5f;

    [HideIf("animOut", ANIM_OUT.NONE)]
    public UnityEvent animOutCompletedEvent;

    public enum ANIM_IN
    {
        NONE,
        FROM_TOP,
        FROM_LEFT,
        FROM_RIGHT,
        FROM_BOT,
        FADE_ALPHA,
        FROM_SCALE
    }

    public enum ANIM_OUT
    {
        NONE,
        TO_TOP,
        TO_LEFT,
        TO_RIGHT,
        TO_BOT,
        FADE_ALPHA,
        TO_SCALE
    }

    public Canvas Root;
    public CanvasGroup RootGraphic;
    protected bool isInited = false;

    public virtual void OnAppear(float dismissTimer = 0f)
    {
        if (RootGraphic)
        {
            RootGraphic.alpha = 1;
            RootGraphic.DOKill(false);
        }

        Root.transform.localScale = Vector3.one;
        Root.transform.DOKill(false);

        if (isInited)
            return;

        AnimIn();

        if (dismissTimer > 0f)
            ScheduleClose(dismissTimer);

        RegisterEvent();

        //AudioAssistant.Shot(typeOpenSound);
    }
    protected void AnimIn()
    {
        if (animIn != ANIM_IN.NONE)
            Root.DOComplete();
        switch (animIn)
        {
            case ANIM_IN.FROM_TOP:
                Root.transform.localPosition = Vector3.up * Screen.height;
                if (RootGraphic)
                {
                    RootGraphic.alpha = 0f;
                    RootGraphic.DOFade(1f, animInTime * 0.75f).SetUpdate(unscaleTime).SetDelay(delayAnimIn);
                }

                Root.transform.DOLocalMoveY(0f, animInTime).SetDelay(delayAnimIn).SetUpdate(unscaleTime).OnComplete(() => { OnAnimInComplete(); });
                break;
            case ANIM_IN.FROM_BOT:
                Root.transform.localPosition = Vector3.down * Screen.height;
                if (RootGraphic)
                {
                    RootGraphic.alpha = 0f;
                    RootGraphic.DOFade(1f, animInTime * 0.75f).SetDelay(delayAnimIn).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveY(0f, animInTime).SetDelay(delayAnimIn).SetUpdate(unscaleTime).OnComplete(() => { OnAnimInComplete(); });
                break;
            case ANIM_IN.FROM_LEFT:
                Root.transform.localPosition = Vector3.left * Screen.width;
                if (RootGraphic)
                {
                    RootGraphic.alpha = 0f;
                    RootGraphic.DOFade(1f, animInTime * 0.75f).SetDelay(delayAnimIn).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveX(0f, animInTime).SetDelay(delayAnimIn).SetUpdate(unscaleTime).OnComplete(() => { OnAnimInComplete(); });
                break;
            case ANIM_IN.FROM_RIGHT:
                Root.transform.localPosition = Vector3.right * Screen.width;
                if (RootGraphic)
                {
                    RootGraphic.alpha = 0f;
                    RootGraphic.DOFade(1f, animInTime * 0.75f).SetDelay(delayAnimIn).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveX(0f, animInTime).SetDelay(delayAnimIn).SetUpdate(unscaleTime).OnComplete(() => { OnAnimInComplete(); });
                break;
            case ANIM_IN.FADE_ALPHA:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 0f;
                    RootGraphic.DOFade(1f, animInTime).SetDelay(delayAnimIn).SetUpdate(unscaleTime).OnComplete(() => { OnAnimInComplete(); });
                }

                break;
            case ANIM_IN.FROM_SCALE:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 0f;
                    RootGraphic.DOFade(1f, animInTime * 0.75f).SetDelay(delayAnimIn).SetUpdate(unscaleTime);
                }

                Root.transform.localScale = Vector3.one * 0.94f;
                Root.transform.DOScale(Vector3.one, animInTime).SetDelay(delayAnimIn).SetUpdate(unscaleTime).OnComplete(() => { OnAnimInComplete(); });
                break;
        }
    }
    public virtual void OnAnimInComplete()
    {
        animInCompletedEvent.Invoke();
    }
    public virtual void OnDisappear()
    {
        UnregisterEvent();
        isInited = false;

    }
    public virtual void Close()
    {
        Close(true);
    }

    public void Close(bool dismiss = true)
    {
        OnDisappear();
        CloseImmediately();
        if (dismiss)
            Gui.Dismiss(this);

        // if (isUseCloseSound)
        //     AudioAssistant.Shot(TYPE_SOUND.CLOSE_POPUP);

        Gui.CheckPopupQueue();
    }
    protected virtual void ScheduleClose(float timer)
    {
        CancelInvoke("Close");
        Invoke("Close", timer);
    }

    public void CloseImmediately()
    {
        if (animOut == ANIM_OUT.NONE)
        {
            CancelInvoke();
            Hide();
        }
        else
            AnimOut();
    }

    void AnimOut()
    {
        if (animOut != ANIM_OUT.NONE)
            Root.DOComplete();
        switch (animOut)
        {
            case ANIM_OUT.TO_TOP:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 1f;
                    RootGraphic.DOFade(0f, animOutTime * 0.75f).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveY(Screen.height / 4f, animOutTime).SetUpdate(unscaleTime).OnComplete(() => { OnAnimOutComplete(); });
                break;
            case ANIM_OUT.TO_BOT:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 1f;
                    RootGraphic.DOFade(0f, animOutTime * 0.75f).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveY(-Screen.height / 4f, animOutTime).SetUpdate(unscaleTime).OnComplete(() => { OnAnimOutComplete(); });
                break;
            case ANIM_OUT.TO_LEFT:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 1f;
                    RootGraphic.DOFade(0f, animOutTime * 0.75f).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveX(-Screen.width / 4f, animOutTime).SetUpdate(unscaleTime).OnComplete(() => { OnAnimOutComplete(); });
                break;
            case ANIM_OUT.TO_RIGHT:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 1f;
                    RootGraphic.DOFade(0f, animOutTime * 0.75f).SetUpdate(unscaleTime);
                }

                Root.transform.DOLocalMoveX(Screen.width / 4f, animOutTime).SetUpdate(unscaleTime).OnComplete(() => { OnAnimOutComplete(); });
                break;
            case ANIM_OUT.FADE_ALPHA:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 1f;
                    RootGraphic.DOFade(0f, animOutTime).SetUpdate(unscaleTime).OnComplete(() => { OnAnimOutComplete(); });
                }

                break;
            case ANIM_OUT.TO_SCALE:
                if (RootGraphic)
                {
                    RootGraphic.alpha = 1f;
                    RootGraphic.DOFade(0f, animOutTime * 0.75f).SetUpdate(unscaleTime);
                }

                Root.transform.localScale = Vector3.one;
                Root.transform.DOScale(Vector3.one * 0.94f, animOutTime).OnComplete(() => { OnAnimOutComplete(); });
                break;
        }
    }

    public virtual void OnAnimOutComplete()
    {
        animOutCompletedEvent.Invoke();
        CancelInvoke();
        Hide();
    }

    protected virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    protected virtual void RegisterEvent()
    {
    }

    protected virtual void UnregisterEvent()
    {
    }
}
