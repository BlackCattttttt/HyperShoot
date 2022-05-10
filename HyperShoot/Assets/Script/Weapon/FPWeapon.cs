using HyperShoot.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HyperShoot.Weapon
{
    public class FPWeapon : BaseWeapon
    {
		public GameObject WeaponPrefab = null;      // NOTE: this is always a PREFAB from the PROJECT VIEW and only used in 1st person
		protected Animation m_WeaponModelAnimation = null;

		protected CharacterController Controller = null;

		// weapon rendering
		public float RenderingZoomDamping = 0.5f;
		protected float m_FinalZoomTime = 0.0f; 
		public float RenderingFieldOfView = 35.0f;
		public Vector2 RenderingClippingPlanes = new Vector2(0.01f, 10.0f);

		// weapon position spring
		public float PositionSpringStiffness = 0.01f;
		public float PositionSpringDamping = 0.25f;
		public float PositionKneeling = 0.06f;
		public int PositionKneelingSoftness = 1;
		protected fp_Spring m_PositionSpring = null;

		// weapon rotation spring
		public float RotationSpringStiffness = 0.01f;
		public float RotationSpringDamping = 0.25f;
		public float RotationKneeling = 0;
		public int RotationKneelingSoftness = 1;
		protected fp_Spring m_RotationSpring = null;

		// weapon switching
		public Vector3 PositionExitOffset = new Vector3(0.0f, -1.0f, 0.0f);     // used by the camera when switching the weapon out of view
		public Vector3 RotationExitOffset = new Vector3(40.0f, 0.0f, 0.0f);
		public Vector3 DefaultPosition { get { return (Vector3)DefaultState.Preset.GetFieldValue("PositionOffset"); } }
		public Vector3 DefaultRotation { get { return (Vector3)DefaultState.Preset.GetFieldValue("RotationOffset"); } }

		// animation
		public AnimationClip AnimationWield = null;
		public AnimationClip AnimationUnWield = null;

		//sway
		public float PositionMaxInputVelocity = 25;
		public Vector3 PositionWalkSlide = new Vector3(0.5f, 0.75f, 0.5f);
		public Vector3 RotationLookSway = new Vector3(1.0f, 0.7f, 0.0f);
		public Vector3 RotationStrafeSway = new Vector3(0.3f, 1.0f, 1.5f);
		public Vector3 RotationFallSway = new Vector3(1.0f, -0.5f, -3.0f);
		public float RotationSlopeSway = 0.5f;
		public float RotationMaxInputVelocity = 15;
		protected Vector2 m_LookInput = Vector2.zero;
		protected Vector3 m_SwayVel = Vector3.zero;
		protected Vector3 m_FallSway = Vector3.zero;

		protected Camera m_WeaponCamera = null;
		protected GameObject m_WeaponGroup = null;
		protected Transform m_WeaponGroupTransform = null;

		FPCharacterEventHandler m_FPPlayer = null;
		FPCharacterEventHandler FPPlayer
		{
			get
			{
				if (m_FPPlayer == null)
				{
					if (EventHandler != null)
						m_FPPlayer = EventHandler as FPCharacterEventHandler;
				}
				return m_FPPlayer;
			}
		}

		protected override void Awake()
		{

			base.Awake();

			// store a reference to the Unity CharacterController
			Controller = Transform.root.GetComponent<CharacterController>();

			if (Controller == null)
			{
				fp_Utility.Activate(gameObject, false);
				return;
			}

			// always start with zero angle
			Transform.eulerAngles = Vector3.zero;

			// hook up the weapon camera - find a regular Unity Camera component
			// existing in a child gameobject to our parent gameobject
			Camera weaponCam = null;
			foreach (Transform t in Transform.parent)
			{
				weaponCam = t.GetComponent<Camera>();
				if (weaponCam != null)
				{
					m_WeaponCamera = weaponCam;
					break;
				}
			}

			// disallow colliders on the weapon or we may get issues with
			// player collision
			if (Collider != null)
				Collider.enabled = false;

		}
		protected override void Start()
		{

			// attempt to spawn a weapon model, if available
			InstantiateWeaponModel();

			base.Start();   // TODO: maybe to this first?

			// set up the weapon group, to which the weapon and pivot object
			// will be childed at runtime (the main purpose of this is to
			// allow runtime pivot manipulation)
			m_WeaponGroup = new GameObject(name + "Transform");
			m_WeaponGroupTransform = m_WeaponGroup.transform;
			m_WeaponGroupTransform.parent = Transform.parent;
			m_WeaponGroupTransform.localPosition = PositionOffset;
			fp_Layer.Set(m_WeaponGroup, fp_Layer.Weapon);

			// reposition weapon under weapon group gameobject
			Transform.parent = m_WeaponGroupTransform;
			Transform.localPosition = Vector3.zero;
			m_WeaponGroupTransform.localEulerAngles = RotationOffset;

			// put this gameobject and all its descendants in the 'WeaponLayer'
			// so the weapon camera can render them separately from the scene
			if (m_WeaponCamera != null && fp_Utility.IsActive(m_WeaponCamera.gameObject))
				fp_Layer.Set(gameObject, fp_Layer.Weapon, true);

			// setup the weapon springs
			m_PositionSpring = new fp_Spring(m_WeaponGroup.gameObject.transform, fp_Spring.UpdateMode.Position);
			m_PositionSpring.RestState = PositionOffset;

			m_PositionSpring2 = new fp_Spring(Transform, fp_Spring.UpdateMode.PositionAdditiveLocal);
			m_PositionSpring2.MinVelocity = 0.00001f;

			m_RotationSpring = new fp_Spring(m_WeaponGroup.gameObject.transform, fp_Spring.UpdateMode.Rotation);
			m_RotationSpring.RestState = RotationOffset;

			m_RotationSpring2 = new fp_Spring(m_WeaponGroup.gameObject.transform, fp_Spring.UpdateMode.RotationAdditiveLocal);
			m_RotationSpring2.MinVelocity = 0.00001f;

			// snap the springs so they always start out rested & in the right place
			SnapSprings();
			Refresh();

		}
		public virtual void InstantiateWeaponModel()
		{

			if (WeaponPrefab != null)
			{
				if (m_WeaponModel != null && m_WeaponModel != this.gameObject)
					Destroy(m_WeaponModel);
				m_WeaponModel = (GameObject)Object.Instantiate(WeaponPrefab);
				m_WeaponModel.transform.parent = transform;
				m_WeaponModel.transform.localPosition = Vector3.zero;
				m_WeaponModel.transform.localScale = new Vector3(1, 1, 1);
				m_WeaponModel.transform.localEulerAngles = Vector3.zero;
				m_WeaponModelAnimation = m_WeaponModel.GetComponent<Animation>();

				// set layer here too in case the method is called at runtime from the editor
				if (m_WeaponCamera != null && fp_Utility.IsActive(m_WeaponCamera.gameObject))
					fp_Layer.Set(m_WeaponModel, fp_Layer.Weapon, true);
			}
			else
				m_WeaponModel = this.gameObject;

			CacheRenderers();

		}
		protected override void Update()
		{

			base.Update();

			if (Time.timeScale != 0.0f)
				UpdateInput();

		}
		protected override void FixedUpdate()
		{

			if (Time.timeScale != 0.0f)
			{

				UpdateZoom();

				UpdateSwaying();

				//UpdateBob();

				//UpdateEarthQuake();

				//UpdateStep();

				//UpdateShakes();

				//UpdateRetraction();

				UpdateSprings();

				//UpdateLookDown();
			}

		}
		public virtual void AddForce(Vector3 force)
		{
			m_PositionSpring.AddForce(force);
		}
		public virtual void AddForce(Vector3 positional, Vector3 angular)
		{
			m_PositionSpring.AddForce(positional);
			m_RotationSpring.AddForce(angular);

		}
		protected virtual void UpdateInput()
		{
			if (Player.Dead.Active)
				return;

			// get input for this frame. unlike camera input, this is non-smoothed,
			// divided by delta for rotation spring framerate independence and
			// multiplied by timescale _twice_  for proper slow motion behavior
			m_LookInput = FPPlayer.InputRawLook.Get() / Delta * Time.timeScale * Time.timeScale;

			// limit rotation velocity to protect against extreme input sensitivity
			m_LookInput = Vector3.Min(m_LookInput, Vector3.one * RotationMaxInputVelocity);
			m_LookInput = Vector3.Max(m_LookInput, Vector3.one * -RotationMaxInputVelocity);

		}
		protected virtual void UpdateSwaying()
		{
			// limit position velocity to protect against extreme speeds
			m_SwayVel = Controller.velocity * 3;
			m_SwayVel = Vector3.Min(m_SwayVel, Vector3.one * PositionMaxInputVelocity);
			m_SwayVel = Vector3.Max(m_SwayVel, Vector3.one * -PositionMaxInputVelocity);

			m_SwayVel *= Time.timeScale;

			// calculate local velocity
			Vector3 localVelocity = Transform.InverseTransformDirection(m_SwayVel / 60);

			// --- pitch & yaw rotational sway ---
			// sway the weapon transform using input and weapon 'weight'
			m_RotationSpring.AddForce(new Vector3(
				(m_LookInput.y * (RotationLookSway.x * 0.025f)),
				(m_LookInput.x * (RotationLookSway.y * -0.025f)),
				m_LookInput.x * (RotationLookSway.z * -0.025f)));

			// --- falling ---

			// rotate weapon while falling. this will take effect in reverse when being elevated,
			// for example walking up a ramp. however, the weapon will only rotate around the z
			// vector while going down
			m_FallSway = (RotationFallSway * (m_SwayVel.y * 0.005f));
			// if grounded, optionally reduce fallsway
			if (Controller.isGrounded)
				m_FallSway *= RotationSlopeSway;
			m_FallSway.z = Mathf.Max(0.0f, m_FallSway.z);
			m_RotationSpring.AddForce(m_FallSway);

			// drag weapon towards ourselves
			m_PositionSpring.AddForce(Vector3.forward * -Mathf.Abs((m_SwayVel.y) * 0.000025f));

			// --- weapon strafe & walk slide ---
			// PositionWalkSlide x will slide sideways when strafing
			// PositionWalkSlide y will slide down when strafing (it can't push up)
			// PositionWalkSlide z will slide forward or backward when walking
			m_PositionSpring.AddForce(new Vector3(
				(localVelocity.x * (PositionWalkSlide.x * 0.0016f)),
				-(Mathf.Abs(localVelocity.x * (PositionWalkSlide.y * 0.0016f))),
				(-localVelocity.z * (PositionWalkSlide.z * 0.0016f))));

			// --- weapon strafe rotate ---
			// RotationStrafeSway x will rotate up when strafing (it can't push down)
			// RotationStrafeSway y will rotate sideways when strafing
			// RotationStrafeSway z will twist weapon around the forward vector when strafing
			m_RotationSpring.AddForce(new Vector3(
				-Mathf.Abs(localVelocity.x * (RotationStrafeSway.x * 0.16f)),
				-(localVelocity.x * (RotationStrafeSway.y * 0.16f)),
				localVelocity.x * (RotationStrafeSway.z * 0.16f)));
		}
		protected virtual void UpdateZoom()
		{
			if (m_FinalZoomTime <= Time.time)
				return;

			if (!m_Wielded)
				return;

			RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
			float zoom = 1.0f - ((m_FinalZoomTime - Time.time) / RenderingZoomDamping);

			if (m_WeaponCamera != null && fp_Utility.IsActive(m_WeaponCamera.gameObject))
				m_WeaponCamera.fieldOfView = Mathf.SmoothStep(m_WeaponCamera.fieldOfView,
																		RenderingFieldOfView, zoom);

		}
		public virtual void Zoom()
		{
			m_FinalZoomTime = Time.time + RenderingZoomDamping;
		}
		public virtual void SnapZoom()
		{
			if (m_WeaponCamera != null && fp_Utility.IsActive(m_WeaponCamera.gameObject))
				m_WeaponCamera.fieldOfView = RenderingFieldOfView;

		}
		protected override void UpdateSprings()
		{
			m_PositionSpring.FixedUpdate();
			m_RotationSpring.FixedUpdate();
			m_PositionSpring2.FixedUpdate();
			m_RotationSpring2.FixedUpdate();
		}
		public virtual void ResetSprings(float positionReset, float rotationReset, float positionPauseTime = 0.0f, float rotationPauseTime = 0.0f)
		{
			m_PositionSpring.State = Vector3.Lerp(m_PositionSpring.State, m_PositionSpring.RestState, positionReset);
			m_RotationSpring.State = Vector3.Lerp(m_RotationSpring.State, m_RotationSpring.RestState, rotationReset);

			if (positionPauseTime != 0.0f)
				m_PositionSpring.ForceVelocityFadeIn(positionPauseTime);

			if (rotationPauseTime != 0.0f)
				m_RotationSpring.ForceVelocityFadeIn(rotationPauseTime);
		}
		public override void SnapSprings()
		{

			base.SnapSprings();

			if (m_PositionSpring != null)
			{
				m_PositionSpring.RestState = PositionOffset;
				m_PositionSpring.State = PositionOffset;
				m_PositionSpring.Stop(true);
			}
			if (m_WeaponGroup != null)
				m_WeaponGroupTransform.localPosition = PositionOffset;

			if (m_RotationSpring != null)
			{
				m_RotationSpring.RestState = RotationOffset;
				m_RotationSpring.State = RotationOffset;
				m_RotationSpring.Stop(true);
			}
		}
		public override void StopSprings()
		{
			if (m_PositionSpring != null)
				m_PositionSpring.Stop(true);

			if (m_RotationSpring != null)
				m_RotationSpring.Stop(true);
		}
		public override void Refresh()
		{
			base.Refresh();

			if (m_PositionSpring != null)
			{
				m_PositionSpring.Stiffness =
					new Vector3(PositionSpringStiffness, PositionSpringStiffness, PositionSpringStiffness);
				m_PositionSpring.Damping = Vector3.one -
					new Vector3(PositionSpringDamping, PositionSpringDamping, PositionSpringDamping);
				m_PositionSpring.RestState = PositionOffset;
			}
			if (m_RotationSpring != null)
			{
				m_RotationSpring.Stiffness =
					new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
				m_RotationSpring.Damping = Vector3.one -
					new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);
				m_RotationSpring.RestState = RotationOffset;

			}
			if (Rendering)
			{
				if ((m_WeaponCamera != null) && fp_Utility.IsActive(m_WeaponCamera.gameObject))
				{
					m_WeaponCamera.nearClipPlane = RenderingClippingPlanes.x;
					m_WeaponCamera.farClipPlane = RenderingClippingPlanes.y;
				}

				Zoom();
			}
		}
		public override void Activate()
		{
			base.Activate();

			SnapZoom();

			if (m_WeaponGroup != null)
			{
				if (!fp_Utility.IsActive(m_WeaponGroup))
					fp_Utility.Activate(m_WeaponGroup);
			}
		}
		public override void Deactivate()
		{
			m_Wielded = false;
			if (m_WeaponGroup != null)
			{
				if (fp_Utility.IsActive(m_WeaponGroup))
					fp_Utility.Activate(m_WeaponGroup, false);
			}
		}
		public virtual void SnapToExit()
		{
			RotationOffset = RotationExitOffset;
			PositionOffset = PositionExitOffset;
			SnapSprings();
		}

		public override void Wield(bool isWielding = true)
		{

			if (isWielding)
				SnapToExit();   // wielding previously unwielded weapon: start at exit offset

			// smoothly rotate and move the weapon into / out of view
			PositionOffset = (isWielding ? DefaultPosition : PositionExitOffset);
			RotationOffset = (isWielding ? DefaultRotation : RotationExitOffset);

			m_Wielded = isWielding;

			Refresh();
			StateManager.CombineStates();

			// play sound
			//if (Audio != null)
			//{

			//	if ((isWielding ? SoundWield : SoundUnWield) != null)
			//	{
			//		if (fp_Utility.IsActive(gameObject))
			//		{
			//			Audio.pitch = Time.timeScale;
			//			Audio.PlayOneShot((isWielding ? SoundWield : SoundUnWield));
			//		}
			//	}
			//}

			// play animation
			if ((isWielding ? AnimationWield : AnimationUnWield) != null)
			{
				if (fp_Utility.IsActive(gameObject) && (m_WeaponModelAnimation != null))
				{
					if (isWielding)
						m_WeaponModelAnimation.CrossFade(AnimationWield.name);
					else
						m_WeaponModelAnimation.CrossFade(AnimationUnWield.name);
				}
			}
		}
		protected virtual void OnMessage_FallImpact(float impact)
		{
			if (m_PositionSpring != null)
				m_PositionSpring.AddSoftForce(Vector3.down * impact * PositionKneeling, PositionKneelingSoftness);

			if (m_RotationSpring != null)
				m_RotationSpring.AddSoftForce(Vector3.right * impact * RotationKneeling, RotationKneelingSoftness);
		}
	}
}
