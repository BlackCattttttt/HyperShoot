using HyperShoot.Combat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HyperShoot.Player
{
	public class FPCharacterEventHandler : CharacterEventHandler
	{
		// gui
		public fp_Message<DamageData> HUDDamageFlash;
		public fp_Message<string> HUDText;
		public fp_Value<Texture> Crosshair;
		public fp_Value<Texture2D> CurrentAmmoIcon;

		// input
		public fp_Value<Vector2> InputSmoothLook;
		public fp_Value<Vector2> InputRawLook;
		public fp_Message<string, bool> InputGetButton;
		public fp_Message<string, bool> InputGetButtonUp;
		public fp_Message<string, bool> InputGetButtonDown;
		public fp_Value<bool> InputAllowGameplay;
		public fp_Value<bool> Pause;

		// camera
		public fp_Value<Vector3> CameraLookDirection;   // returns camera forward vector. NOTE: this will be different from 'HeadLookDirection' in 3rd person
		public fp_Message CameraToggle3rdPerson;
		public fp_Message<float> CameraGroundStomp;
		public fp_Message<float> CameraBombShake;
		public fp_Value<Vector3> CameraEarthQuakeForce;
		public fp_Activity<Vector3> CameraEarthQuake;

		public fp_Value<string> CurrentWeaponClipType;
		public fp_Attempt<object> AddAmmo;
		public fp_Attempt RemoveClip;

		protected override void Awake()
		{

			base.Awake();

			//	fp_LocalPlayer.Refresh();

		}

		private void OnEnable()
		{

#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded += OnLevelLoad;
#endif

		}

		private void OnDisable()
		{

#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded -= OnLevelLoad;
#endif

		}

#if UNITY_5_4_OR_NEWER
		protected virtual void OnLevelLoad(Scene scene, LoadSceneMode mode)
#else
	protected virtual void OnLevelWasLoaded()
#endif
		{

			//fp_LocalPlayer.Refresh();

		}

	}
}
