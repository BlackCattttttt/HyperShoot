using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HyperShoot.Combat;
using UniRx;

public class SecurityCamTurret : BaseTurret
{
    fp_AngleBob m_AngleBob = null;

    [SerializeField] private GameObject Swivel = null;
    Vector3 SwivelRotation = Vector3.zero;

    [SerializeField] private float SwivelAmp = 100;
    [SerializeField] private float SwivelRate = 0.5f;
    [SerializeField] private float SwivelOffset = 0.0f;
    [SerializeField] private EnemyDamageHandler enemyDamageHandler;

    fp_Timer.Handle fp_ResumeSwivelTimer = new fp_Timer.Handle();

    protected readonly CompositeDisposable _healthDisposables = new CompositeDisposable();
    protected override void Start()
    {
        base.Start();

        m_Transform = transform;
        m_AngleBob = gameObject.AddComponent<fp_AngleBob>();
        m_AngleBob.BobAmp.y = SwivelAmp;
        m_AngleBob.BobRate.y = SwivelRate;
        m_AngleBob.YOffset = SwivelOffset;
        m_AngleBob.FadeToTarget = true;

        SwivelRotation = Swivel.transform.eulerAngles;
        enemyDamageHandler.HealthObservable
             .Where(hp => hp <= 0f)
             .Subscribe(_ => Die())
             .AddTo(_healthDisposables);
    }
    public virtual void Die()
    {
        if (!isDead)
        {
            isDead = true;
            Destroy(gameObject, 2f);
        }
    }
    protected override void Update()
    {
        base.Update();

        if (!isDead)
        {
            // if have a target and swiveling is enabled
            if ((m_Target != null) && m_AngleBob.enabled)
            {
                m_AngleBob.enabled = false;
                fp_ResumeSwivelTimer.Cancel();
            }

            // if we have no target and swiveling is not enabled
            if ((m_Target == null) && !m_AngleBob.enabled && !fp_ResumeSwivelTimer.Active)
            {
                fp_Timer.In(WakeInterval * 2.0f, delegate ()
                {
                    m_AngleBob.enabled = true;
                }, fp_ResumeSwivelTimer);
            }

#if UNITY_EDITOR
            m_AngleBob.BobAmp.y = SwivelAmp;
            m_AngleBob.BobRate.y = SwivelRate;
            m_AngleBob.YOffset = SwivelOffset;
#endif

            SwivelRotation.y = m_Transform.eulerAngles.y;
            Swivel.transform.eulerAngles = SwivelRotation;
        }
    }
}