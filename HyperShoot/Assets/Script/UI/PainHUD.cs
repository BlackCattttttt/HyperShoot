using UnityEngine;
using System.Collections.Generic;
using HyperShoot.Combat;
using HyperShoot.Player;

public class PainHUD : MonoBehaviour
{
    protected class Inflictor
    {
        public Transform Transform = null;
        public float DamageTime = 0.0f;
        public Inflictor(Transform transform, float damageTime)
        {
            Transform = transform;
            DamageTime = damageTime;
        }
    }

    // list of current inflictors
    protected List<Inflictor> m_Inflictors = new List<Inflictor>();

    public Texture PainTexture = null;
    public Texture DeathTexture = null;
    public Texture ArrowTexture = null;

    public float PainIntensity = 0.2f;

    [Range(0.01f, 0.5f)]
    public float ArrowScale = 0.083f;
    public float ArrowAngleOffset = -135;
    public float ArrowVisibleDuration = 1.5f;
    public float ArrowShakeDuration = 0.125f;

    protected float m_LastInflictorTime = 0.0f;
    protected DamageType m_LatestIncomingDamageType = DamageType.Unknown;
    protected Color m_PainColor = new Color(0.8f, 0, 0, 0);
    protected Color m_ArrowColor = new Color(0.8f, 0, 0, 0);
    protected Color m_FlashInvisibleColor = new Color(1, 0, 0, 0);
    protected Color m_SplatColor = new Color(1, 1, 1, 0);
    protected Rect m_SplatRect;

    private FPCharacterEventHandler m_Player = null;

    protected bool m_RenderGUI = true;
    public bool UseOnGUI
    {
        get
        {
            return m_RenderGUI;
        }
        set
        {
            m_RenderGUI = value;
        }
    }

    public Color PainColor
    {
        get
        {
            return m_PainColor;
        }
    }
    protected virtual void Awake()
    {
        m_Player = transform.GetComponent<FPCharacterEventHandler>();
    }

    protected virtual void OnEnable()
    {
        if (m_Player != null)
            m_Player.Register(this);
    }

    protected virtual void OnDisable()
    {
        if (m_Player != null)
            m_Player.Unregister(this);
    }

    protected virtual void OnGUI()
    {
        UpdatePainFlash();

        UpdateInflictorArrows();

        UpdateDeathTexture();
    }

    protected virtual void UpdatePainFlash()
    {
        if (m_PainColor.a < 0.01f)
        {
            m_PainColor.a = 0.0f;
            return;
        }

        m_PainColor = Color.Lerp(m_PainColor, m_FlashInvisibleColor, Time.deltaTime * 0.4f);
        if (UseOnGUI)
        {
            GUI.color = m_PainColor;

            if (PainTexture != null)
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), PainTexture);

            GUI.color = Color.white;

        }
    }

    protected virtual void UpdateInflictorArrows()
    {
        if (ArrowTexture == null)
            return;

        for (int v = m_Inflictors.Count - 1; v > -1; v--)
        {
            if ((m_Inflictors[v] == null)
                || (m_Inflictors[v].Transform == null)
                || (!fp_Utility.IsActive(m_Inflictors[v].Transform.gameObject)))
            {
                m_Inflictors.Remove(m_Inflictors[v]);
                continue;
            }

            // fade out arrow
            m_ArrowColor.a = (ArrowVisibleDuration - (Time.time - m_Inflictors[v].DamageTime)) / ArrowVisibleDuration;

            // skip any invisible arrows
            if (m_ArrowColor.a < 0.0f)
                continue;

            // get horizontal direction of damage inflictor
            Vector2 pos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            float rot = fp_3DUtility.LookAtAngleHorizontal(
                transform.position,
                transform.forward,
                m_Inflictors[v].Transform.position)
                + ArrowAngleOffset
                + ((m_Inflictors[v].Transform != transform) ? 0 : 90);  // if damaging self, point straight down
            float scale = (Screen.width * ArrowScale);

            float push = (ArrowShakeDuration - (Time.time - m_LastInflictorTime)) / ArrowShakeDuration;
            push = Mathf.Lerp(0, 1, push);
            scale += ((Screen.width / 100) * push);

            if (UseOnGUI)
            {
                // rotate and draw arrow
                Matrix4x4 matrixBackup = GUI.matrix;
                GUIUtility.RotateAroundPivot(rot, pos);
                GUI.color = m_ArrowColor;
                GUI.DrawTexture(new Rect(pos.x, pos.y, scale, scale), ArrowTexture);
                GUI.matrix = matrixBackup;
            }
        }
    }

    protected virtual void UpdateDeathTexture()
    {
        if (DeathTexture == null)
            return;

        if (!m_Player.Dead.Active)
            return;

        if (m_SplatColor.a == 0.0f)
            return;

        if (UseOnGUI)
        {
            GUI.color = m_SplatColor;
            GUI.DrawTexture(m_SplatRect, DeathTexture);
        }
    }

    protected virtual void OnMessage_HUDDamageFlash(DamageData damageInfo)
    {
        if (damageInfo.Damage == 0.0f)
        {
            m_PainColor.a = 0.0f;
            m_SplatColor.a = 0.0f;
            return;
        }

        m_LatestIncomingDamageType = damageInfo.Type;

        m_PainColor.a += (damageInfo.Damage * PainIntensity);
    }

    protected virtual void OnStart_Dead()
    {
        // don't show blood spatter for falling damage
        if (m_LatestIncomingDamageType == DamageType.Fall)
        {
            m_SplatColor.a = 0.0f;
            return;
        }

        float col = (Random.value * 0.6f) + 0.4f;
        m_SplatColor = new Color(col, col, col, 1);

        // decide how big the droplets should appear
        float zoom =
            (Random.value < 0.5f) ?
            (Screen.width / Random.Range(5, 10)) :
             (Screen.width / Random.Range(4, 7));

        // set up screen rect
        m_SplatRect = new Rect(
            Random.Range(-zoom, 0),
            Random.Range(-zoom, 0),
            Screen.width + zoom,
            Screen.height + zoom);

        if (Random.value < 0.5f)
        {
            m_SplatRect.x = Screen.width - m_SplatRect.x;
            m_SplatRect.width = -m_SplatRect.width;
        }

        if (Random.value < 0.125f)
            col *= 0.5f;
        m_SplatColor = new Color(col, col, col, 1);
        m_SplatRect.y = Screen.height - m_SplatRect.y;
        m_SplatRect.height = -m_SplatRect.height;
    }
    protected virtual void OnStop_Dead()
    {
        m_PainColor.a = 0.0f;

        for (int v = m_Inflictors.Count - 1; v > -1; v--)
        {
            m_Inflictors[v].DamageTime = 0.0f;
        }

    }
}




