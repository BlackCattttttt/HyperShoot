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

#if UNITY_EDITOR
		protected bool m_AllowEditing = false;
		public bool AllowEditTransform
		{
			get
			{
				return m_AllowEditing;
			}
			set
			{
				m_AllowEditing = value;
			}
		}
#endif

		// event handler property cast as a playereventhandler
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
		}

		protected override void Awake()
       	{
			base.Awake();

			RotationOffset = transform.localEulerAngles;
			PositionOffset = transform.position;

			Transform.localEulerAngles = RotationOffset;

			if (transform.parent == null) // TODO: or parent contains a fp_FPCamera
			{
				fp_Utility.Activate(gameObject, false);
				return;
			}

			// disallow colliders on the weapon or we may get issues with
			// player collision
			if (GetComponent<Collider>() != null)
				GetComponent<Collider>().enabled = false;

#if UNITY_EDITOR
			m_AllowEditing = false;
#endif
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

		public virtual void AddForce2(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
		{
			AddForce2(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
		}

		protected virtual void UpdateSprings()
		{
			Transform.localPosition = Vector3.up;           // middle of player
			Transform.localRotation = Quaternion.identity;  // aiming head-on

			// update recoil springs for additive position and rotation forces
			m_PositionSpring2.FixedUpdate();    // TODO: only in 1st person
			m_RotationSpring2.FixedUpdate();

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
				m_RotationSpring2.RestState = m_RotationSpring2DefaultRotation;
			}
		}

		public override void Activate()
		{
			base.Activate();

			m_Wielded = true;
			Rendering = true;
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
				m_RotationSpring2.RestState = m_RotationSpring2DefaultRotation;
				m_RotationSpring2.State = m_RotationSpring2DefaultRotation;
				m_RotationSpring2.Stop(true);
			}
		}

		public virtual void StopSprings()
		{
			if (m_PositionSpring2 != null)
				m_PositionSpring2.Stop(true);

			if (m_RotationSpring2 != null)
				m_RotationSpring2.Stop(true);
		}

		public virtual void Wield(bool isWielding = true)
		{
			m_Wielded = isWielding;

			Refresh();
			StateManager.CombineStates();
		}

		protected virtual void OnStart_Dead()
		{
			if (Player.IsFirstPerson.Get())
				return;

			Rendering = false;
		}

		protected virtual void OnStop_Dead()
		{
			if (Player.IsFirstPerson.Get())
				return;

			Rendering = true;
		}

		protected virtual bool CanStart_Zoom()
		{
		//	if (Player.CurrentWeaponType.Get() == (int)fp_Weapon.Type.Melee)
		//		return false;

			return true;
		}
	}
}
