using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecurityCamTurret : BaseTurret
{
	fp_AngleBob m_AngleBob = null;

	public GameObject Swivel = null;
	Vector3 SwivelRotation = Vector3.zero;

	public float SwivelAmp = 100;
	public float SwivelRate = 0.5f;
	public float SwivelOffset = 0.0f;

	fp_Timer.Handle fp_ResumeSwivelTimer = new fp_Timer.Handle();

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
	}

	protected override void Update()
	{
		base.Update();

		// if have a target and swiveling is enabled
		if ((m_Target != null) && m_AngleBob.enabled)
		{
			m_AngleBob.enabled = false;
			fp_ResumeSwivelTimer.Cancel();
		}

		// if we have no target and swiveling is not enabled
		if ((m_Target == null) && !m_AngleBob.enabled && !fp_ResumeSwivelTimer.Active)
		{
			fp_Timer.In(WakeInterval * 2.0f, delegate()
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