using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HyperShoot.Player
{
    public class FPCamera : fp_Component
	{
		// character controller of the parent gameobject
		[HideInInspector] public FPCharacterController FPController = null;
		// camera position
		public Vector3 PositionOffset = new Vector3(0.0f, 1.75f, 0.1f);
		// camera rotation
		public Vector2 RotationPitchLimit = new Vector2(90.0f, -90.0f);
		public Vector2 RotationYawLimit = new Vector2(-360.0f, 360.0f);
		protected float m_Pitch = 0.0f;
		protected float m_Yaw = 0.0f;

		private FPCharacterEventHandler m_Player = null;
		public FPCharacterEventHandler Player
		{
			get
			{
				if (m_Player == null)
				{
					if (EventHandler != null)
						m_Player = (FPCharacterEventHandler)EventHandler;
				}
				return m_Player;
			}
		}
		// angle properties

		public Vector2 Angle
		{
			get { return new Vector2(m_Pitch, m_Yaw); }
			set
			{
				Pitch = value.x;
				Yaw = value.y;
			}
		}

		public float Pitch
		{
			// pitch is rotation around the x-vector
			get { return m_Pitch; }
			set
			{
				if (value > 90)
					value -= 360;
				m_Pitch = value;
			}
		}

		public float Yaw
		{
			// yaw is rotation around the y-vector
			get { return m_Yaw; }
			set
			{
				m_Yaw = value;
			}
		}
		protected override void Awake()
		{

			base.Awake();

			FPController = Root.GetComponent<FPCharacterController>();

			// run 'SetRotation' with the initial rotation of the camera. this is important
			// when not using the spawnpoint system (or player rotation will snap to zero yaw)
			SetRotation(new Vector2(Transform.eulerAngles.x, Transform.eulerAngles.y));

			// set parent gameobject layer to 'LocalPlayer', so camera can exclude it
			// this also prevents shell casings from colliding with the charactercollider
			Parent.gameObject.layer = fp_Layer.LocalPlayer;

			// main camera initialization
			// render everything except body and weapon
			Camera.cullingMask &= ~((1 << fp_Layer.LocalPlayer) | (1 << fp_Layer.Weapon));
			Camera.depth = 0;

			// weapon camera initialization
			// find a regular Unity Camera component existing in a child
			// gameobject to the FPSCamera's gameobject. if we don't find
			// a weapon cam, that's OK (some games don't have weapons).
			// NOTE: we don't use GetComponentInChildren here because that
			// would return the MainCamera (on this transform)
			Camera weaponCam = null;
			foreach (Transform t in Transform)
			{
				weaponCam = (Camera)t.GetComponent(typeof(Camera));
				if (weaponCam != null)
				{
					weaponCam.transform.localPosition = Vector3.zero;
					weaponCam.transform.localEulerAngles = Vector3.zero;
					weaponCam.clearFlags = CameraClearFlags.Depth;
					weaponCam.cullingMask = (1 << fp_Layer.Weapon); // only render the weapon
					weaponCam.depth = 1;
					weaponCam.farClipPlane = 100;
					weaponCam.nearClipPlane = 0.01f;
					weaponCam.fieldOfView = 60;
					break;
				}
			}

			// create springs for camera motion

			// --- primary position spring ---
			// this is used for all sorts of positional force acting on the camera
			//m_PositionSpring = new vp_Spring(Transform, vp_Spring.UpdateMode.Position, false);
			//m_PositionSpring.MinVelocity = 0.0f;
			//m_PositionSpring.RestState = PositionOffset;

			// --- secondary position spring ---
			// this is mainly intended for positional force from recoil, stomping and explosions
		//	m_PositionSpring2 = new vp_Spring(Transform, vp_Spring.UpdateMode.PositionAdditiveLocal, false);
			//m_PositionSpring2.MinVelocity = 0.0f;

			// --- rotation spring ---
			// this is used for all sorts of angular force acting on the camera
		//	m_RotationSpring = new vp_Spring(Transform, vp_Spring.UpdateMode.RotationAdditiveLocal, false);
		//	m_RotationSpring.MinVelocity = 0.0f;
		}
		protected override void OnEnable()
		{
			base.OnEnable();
		//	vp_TargetEvent<float>.Register(m_Root, "CameraBombShake", OnMessage_CameraBombShake);
		//	vp_TargetEvent<float>.Register(m_Root, "CameraGroundStomp", OnMessage_CameraGroundStomp);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		//	vp_TargetEvent<float>.Unregister(m_Root, "CameraBombShake", OnMessage_CameraBombShake);
		//	vp_TargetEvent<float>.Unregister(m_Root, "CameraGroundStomp", OnMessage_CameraGroundStomp);
		}
		protected override void Start()
		{

			base.Start();

			Refresh();

			// snap the camera to its start values when first activated
			//SnapSprings();
			//SnapZoom();

		}
		protected override void Update()
		{

			base.Update();

			if (Time.timeScale == 0.0f)
				return;

			UpdateInput();

		}


		/// <summary>
		/// 
		/// </summary>
		protected override void FixedUpdate()
		{

			base.FixedUpdate();

			if (Time.timeScale == 0.0f)
				return;

			//UpdateZoom();

			//UpdateSwaying();

			//UpdateBob();

			//UpdateEarthQuake();

			//UpdateShakes();

			//UpdateSprings();

		}


		/// <summary>
		/// actual rotation of the player model and camera is performed in
		/// LateUpdate, since by then all game logic should be finished
		/// </summary>
		protected override void LateUpdate()
		{

			base.LateUpdate();

			if (Time.timeScale == 0.0f)
				return;

			// fetch the FPSController's SmoothPosition. this reduces jitter
			// by moving the camera at arbitrary update intervals while
			// controller and springs move at the fixed update interval
			m_Transform.position = FPController.SmoothPosition;

			// apply current spring offsets
			//if (Player.IsFirstPerson.Get())
			//	m_Transform.localPosition += (m_PositionSpring.State + m_PositionSpring2.State);
			//else
			//	m_Transform.localPosition += (m_PositionSpring.State +
			//									(Vector3.Scale(m_PositionSpring2.State, Vector3.up)));  // don't shake camera sideways in third person

			// prevent camera from intersecting objects
			//TryCameraCollision();

			// rotate the parent gameobject (i.e. player model)
			// NOTE: this rotation does not pitch the player model, it only applies yaw
			Quaternion xQuaternion = Quaternion.AngleAxis(m_Yaw, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis(0, Vector3.left);
			Parent.rotation =
				fp_MathUtility.NaNSafeQuaternion((xQuaternion * yQuaternion), Parent.rotation);

			// pitch and yaw the camera
			yQuaternion = Quaternion.AngleAxis(-m_Pitch, Vector3.left);
			Transform.rotation =
				fp_MathUtility.NaNSafeQuaternion((xQuaternion * yQuaternion), Transform.rotation);

			// roll the camera
			//Transform.localEulerAngles +=
			//	fp_MathUtility.NaNSafeVector3(Vector3.forward * m_RotationSpring.State.z);

		}
		protected virtual void UpdateInput()
		{

			if (Player.Dead.Active)
				return;

			if (Player.InputSmoothLook.Get() == Vector2.zero)
				return;

			// modify pitch and yaw with mouselook
			m_Yaw += Player.InputSmoothLook.Get().x;
			m_Pitch += Player.InputSmoothLook.Get().y;

			// clamp angles
			m_Yaw = m_Yaw < -360.0f ? m_Yaw += 360.0f : m_Yaw;
			m_Yaw = m_Yaw > 360.0f ? m_Yaw -= 360.0f : m_Yaw;
			m_Yaw = Mathf.Clamp(m_Yaw, RotationYawLimit.x, RotationYawLimit.y);
			m_Pitch = m_Pitch < -360.0f ? m_Pitch += 360.0f : m_Pitch;
			m_Pitch = m_Pitch > 360.0f ? m_Pitch -= 360.0f : m_Pitch;
			m_Pitch = Mathf.Clamp(m_Pitch, -RotationPitchLimit.x, -RotationPitchLimit.y);

		}
		/// <summary>
		/// sets camera rotation and snaps springs and zoom to a halt
		/// </summary>
		public virtual void SetRotation(Vector2 eulerAngles)
		{

			Angle = eulerAngles;
			Stop();

		}
		/// <summary>
		/// stops the springs and zoom
		/// </summary>
		public virtual void Stop()
		{
			//SnapSprings();
			//SnapZoom();
			Refresh();
		}
	}
}
