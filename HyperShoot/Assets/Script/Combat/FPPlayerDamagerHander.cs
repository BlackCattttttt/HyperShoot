using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;
using UniRx;
using System;

namespace HyperShoot.Combat
{
    public class FPPlayerDamagerHander : PlayerDamageHandler
    {
		public float CameraShakeFactor = 0.02f;
		protected float m_DamageAngle = 0.0f;
		protected float m_DamageAngleFactor = 1.0f;
		private readonly CompositeDisposable _disposables = new CompositeDisposable();
		private bool _isDead = false;

		protected FPCharacterEventHandler m_FPPlayer = null;    // should never be referenced directly
		protected FPCharacterEventHandler FPPlayer  // lazy initialization of the event handler field
		{
			get
			{
				if (m_FPPlayer == null)
					m_FPPlayer = transform.GetComponent<FPCharacterEventHandler>();
				return m_FPPlayer;
			}
		}

		protected FPCamera m_FPCamera = null;    // should never be referenced directly
		protected FPCamera FPCamera  // lazy initialization of the fp camera field
		{
			get
			{
				if (m_FPCamera == null)
					m_FPCamera = transform.GetComponentInChildren<FPCamera>();
				return m_FPCamera;
			}
		}
		protected CharacterController m_CharacterController = null; // should never be referenced directly
		protected CharacterController CharacterController   // lazy initialization of the event handler field
		{
			get
			{
				if (m_CharacterController == null)
					m_CharacterController = transform.root.GetComponentInChildren<CharacterController>();
				return m_CharacterController;
			}
		}
		protected override void OnEnable()
		{
			if (FPPlayer != null)
				FPPlayer.Register(this);

			RefreshColliders();
		}
		protected override void OnDisable()
		{
			if (FPPlayer != null)
				FPPlayer.Unregister(this);
		}
		public override void TakeDamage(DamageData damageInfo)
		{

			if (!enabled)
				return;

			if (!fp_Utility.IsActive(gameObject))
				return;

			base.TakeDamage(damageInfo);

			FPPlayer.HUDDamageFlash.Send(damageInfo);

			// shake camera to left or right depending on direction of damage
			if (damageInfo.ImpactObject != null)
			{
				m_DamageAngle = fp_3DUtility.LookAtAngleHorizontal(
					FPCamera.Transform.position,
					FPCamera.Transform.forward,
					damageInfo.ImpactObject.transform.position);

				m_DamageAngleFactor = ((Mathf.Abs(m_DamageAngle) > 30.0f) ? 1 : (Mathf.Lerp(0, 1, (Mathf.Abs(m_DamageAngle) * 0.033f))));
			}
		}
		public override void Die()
		{
       		base.Die();

			if (!enabled || !fp_Utility.IsActive(gameObject))
				return;

			FPPlayer.InputAllowGameplay.Set(false);

			if (!_isDead)
			{
				Observable.Timer(TimeSpan.FromSeconds(2))
						 .Subscribe(_ =>
						 {
							 fp_Utility.LockCursor = false;
							 LoadingManager.Instance.LoadScene(SCENE_INDEX.Lose, () => LoseScreen.Show());
						 })
						 .AddTo(_disposables);
			}
			_isDead = true;
		}
		public virtual void RefreshColliders()
		{
			if ((CharacterController != null) && CharacterController.enabled)
			{
				foreach (Collider c in Colliders)
				{
					if (c.enabled)
						Physics.IgnoreCollision(CharacterController, c, true);
				}
			}
		}
		protected override void Reset()
		{
			base.Reset();

			if (!Application.isPlaying)
				return;

			FPPlayer.InputAllowGameplay.Set(true);
			//FPPlayer.HUDDamageFlash.Send(null);

     		RefreshColliders();
		}
		void OnStart_Crouch()
		{
			RefreshColliders();
		}
		void OnStop_Crouch()
		{
			RefreshColliders();
		}
	}
}
