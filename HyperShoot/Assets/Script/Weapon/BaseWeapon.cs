using HyperShoot.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperShoot.Weapon
{
    public class BaseWeapon : fp_Component
    {
		protected GameObject m_WeaponModel = null;
		// recoil position spring
		public Vector3 PositionOffset = new Vector3(0.15f, -0.15f, -0.15f);
		public float PositionSpring2Stiffness = 0.95f;
		public float PositionSpring2Damping = 0.25f;
		protected fp_Spring m_PositionSpring2 = null;       // spring for secondary forces like recoil (typically with stiffer spring settings)

		// recoil rotation spring
		public Vector3 RotationOffset = Vector3.zero;
		public float RotationSpring2Stiffness = 0.95f;
		public float RotationSpring2Damping = 0.25f;
		protected fp_Spring m_RotationSpring2 = null;       // spring for secondary forces like recoil (typically with stiffer spring settings)
		protected Vector3 m_RotationSpringDefaultRotation = Vector3.zero;

		// weapon switching
		protected bool m_Wielded = true;
		public bool Wielded { get { return (m_Wielded && Rendering); } set { m_Wielded = value; } }
		// weapon info
		public int AnimationType = 1;
		public int AnimationGrip = 1;

		public new enum Type
		{
			Custom,
			Firearm,
			Melee,
			Thrown
		}

		public enum Grip
		{
			Custom,
			OneHanded,
			TwoHanded,
			TwoHandedHeavy
		}

		protected Vector3 m_RotationSpring2DefaultRotation = Vector3.zero;
		public Vector3 RotationSpring2DefaultRotation
		{
			get
			{
				return m_RotationSpring2DefaultRotation;
			}
			set
			{
				m_RotationSpring2DefaultRotation = value;
			}
		}                                             // event handler property cast as a playereventhandler
		protected CharacterEventHandler m_Player = null;
		protected CharacterEventHandler Player
		{
			get
			{
				if (m_Player == null)
				{
					if (EventHandler != null)
						m_Player = (CharacterEventHandler)EventHandler;
				}
				return m_Player;
			}
		}
		protected override void Awake()
		{

			base.Awake();

			RotationOffset = transform.localEulerAngles;
			PositionOffset = transform.position;

			Transform.localEulerAngles = RotationOffset;

			if (transform.parent == null) // TODO: or parent contains a vp_FPCamera
			{
				fp_Utility.Activate(gameObject, false);
				return;
			}

			// disallow colliders on the weapon or we may get issues with
			// player collision
			if (GetComponent<Collider>() != null)
				GetComponent<Collider>().enabled = false;
		}

		protected override void Start()
		{

			base.Start();

			// setup the weapon springs
			m_PositionSpring2 = new fp_Spring(transform, fp_Spring.UpdateMode.PositionAdditiveSelf, true);
			m_PositionSpring2.MinVelocity = 0.00001f;

			m_RotationSpring2 = new fp_Spring(transform, fp_Spring.UpdateMode.RotationAdditiveGlobal);
			m_RotationSpring2.MinVelocity = 0.00001f;

			// snap the springs so they always start out rested & in the right place
			SnapSprings();
			Refresh();

			CacheRenderers();

			//if (Player.IsLocal.Get())
			//	CacheMaterials();

		}
		public Vector3 Recoil
		{
			get
			{
				return m_RotationSpring2.State;
			}
		}
		protected override void FixedUpdate()
		{

			base.FixedUpdate();


			if (Time.timeScale == 0.0f)
				return;

			UpdateSprings();

		}
		public virtual void AddForce2(Vector3 positional, Vector3 angular)
		{
			if (m_PositionSpring2 != null)
				m_PositionSpring2.AddForce(positional);

			if (m_RotationSpring2 != null)
				m_RotationSpring2.AddForce(angular);
		}
		protected virtual void UpdateSprings()
		{
			Transform.localPosition = Vector3.up;           // middle of player
			Transform.localRotation = Quaternion.identity;  // aiming head-on

			// update recoil springs for additive position and rotation forces
			m_PositionSpring2.FixedUpdate();    // TODO: only in 1st person
			m_RotationSpring2.FixedUpdate();

		}
		public virtual void Wield(bool isWielding = true)
		{
			m_Wielded = isWielding;

			Refresh();
			StateManager.CombineStates();
		}
		public override void Activate()
		{
			base.Activate();

			m_Wielded = true;
			Rendering = true;
		}
		public override void Refresh()
		{

			if (!Application.isPlaying)
				return;

			if (m_PositionSpring2 != null)
			{
				m_PositionSpring2.Stiffness =
					new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
				m_PositionSpring2.Damping = Vector3.one -
					new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);
				m_PositionSpring2.RestState = Vector3.zero;
			}

			if (m_RotationSpring2 != null)
			{
				m_RotationSpring2.Stiffness =
					new Vector3(RotationSpring2Stiffness, RotationSpring2Stiffness, RotationSpring2Stiffness);
				m_RotationSpring2.Damping = Vector3.one -
					new Vector3(RotationSpring2Damping, RotationSpring2Damping, RotationSpring2Damping);
				m_RotationSpring2.RestState = m_RotationSpringDefaultRotation;
			}
		}
		public virtual void StopSprings()
		{
			if (m_PositionSpring2 != null)
				m_PositionSpring2.Stop(true);

			if (m_RotationSpring2 != null)
				m_RotationSpring2.Stop(true);

		}
		public virtual void SnapSprings()
		{
			if (m_PositionSpring2 != null)
			{
				m_PositionSpring2.RestState = Vector3.zero;
				m_PositionSpring2.State = Vector3.zero;
				m_PositionSpring2.Stop(true);
			}

			if (m_RotationSpring2 != null)
			{
				m_RotationSpring2.RestState = m_RotationSpringDefaultRotation;
				m_RotationSpring2.State = m_RotationSpringDefaultRotation;
				m_RotationSpring2.Stop(true);
			}
		}
		protected virtual bool CanStart_Zoom()
		{

			//if (Player.CurrentWeaponType.Get() == (int)vp_Weapon.Type.Melee)
			//	return false;

			return true;

		}
	}
}
