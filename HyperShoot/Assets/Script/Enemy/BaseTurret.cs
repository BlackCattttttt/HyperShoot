using UnityEngine;
using System.Collections;
using HyperShoot.Weapon;

public class BaseTurret : MonoBehaviour
{ 
    [SerializeField] protected float ViewRange = 10.0f;
	[SerializeField] protected float AimSpeed = 50.0f;
	[SerializeField] protected float WakeInterval = 2.0f;
	[SerializeField] protected float FireAngle = 10.0f;

	protected BaseShooter m_Shooter = null;
	protected Transform m_Transform = null;
	protected Transform m_Target = null;
	protected Collider m_TargetCollider = null;
	protected fp_Timer.Handle m_Timer = new fp_Timer.Handle();

	protected virtual void Start()
	{
		m_Shooter = GetComponent<BaseShooter>();
		m_Transform = transform;
	}

	protected virtual void Update()
	{
		if (!m_Timer.Active)
		{
			fp_Timer.In(WakeInterval, delegate()
			{
				if (m_Target == null)
					m_Target = ScanForLocalPlayer();
				else
				{
					m_Target = null;
					m_TargetCollider = null;
				}
			}, m_Timer);
		}

		if (m_Target != null)
			AttackTarget();
	}

	protected virtual Transform ScanForLocalPlayer()
	{
		Collider[] colliders = Physics.OverlapSphere(m_Transform.position, ViewRange, (1 << fp_Layer.LocalPlayer));
		foreach (Collider hit in colliders)
		{
			RaycastHit blocker;
			Physics.Linecast(m_Transform.position, hit.transform.position + Vector3.up, out blocker, fp_Layer.Mask.BulletBlockers);

			// skip if raycast hit an object that wasn't the intended target
			if (blocker.collider != null && blocker.collider != hit)
				continue;

			// we have line of sight to the local player! return its transform
			return hit.transform;

		}
		return null;
	}

	protected virtual void AttackTarget()
	{
		// smoothly aim at target
		if (m_TargetCollider == null)
			m_TargetCollider = m_Target.GetComponent<Collider>();
		Vector3 dir;
		if (m_TargetCollider != null)
			dir = (m_TargetCollider.bounds.center - m_Transform.position);
		else
			dir = (m_Target.transform.position - m_Transform.position);
		m_Transform.rotation = Quaternion.RotateTowards(m_Transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * AimSpeed);

		// fire the shooter
		if(Mathf.Abs(fp_3DUtility.LookAtAngleHorizontal(m_Transform.position, m_Transform.forward, m_Target.position)) < FireAngle)
			m_Shooter.TryFire();
	}
}
