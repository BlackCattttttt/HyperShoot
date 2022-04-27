using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HyperShoot.Player
{
	public class FPCharacterEventHandler : CharacterEventHandler
	{

		// these declarations determine which events are supported by the
		// player event handler. it is then up to external classes to fill
		// them up with delegates for communication.

		// TIPS:
		//  1) mouse-over on the event types (e.g. fp_Message) for usage info.
		//  2) to find the places where an event is SENT, you can do 'Find All
		// References' on the event in your IDE. if this is not available, you
		// can search the project for the event name preceded by '.' (.Reload)
		//  3) to find the methods that LISTEN to an event, search the project
		// for its name preceded by '_' (_Reload)


		// gui
		//public fp_Message<fp_DamageInfo> HUDDamageFlash;
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

		// old inventory system
		// TIP: these events can be removed along with the old inventory system
		public fp_Value<string> CurrentWeaponClipType;
		public fp_Attempt<object> AddAmmo;
		public fp_Attempt RemoveClip;


		/// <summary>
		/// on startup, cache the local player and all of its standard components
		/// for use by the globally accessible 'fp_LocalPlayer' wrapper
		/// </summary>
		protected override void Awake()
		{

			base.Awake();

			//	fp_LocalPlayer.Refresh();

		}


		/// <summary>
		/// 
		/// </summary>
		private void OnEnable()
		{

#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded += OnLevelLoad;
#endif

		}


		/// <summary>
		/// 
		/// </summary>
		private void OnDisable()
		{

#if UNITY_5_4_OR_NEWER
			SceneManager.sceneLoaded -= OnLevelLoad;
#endif

		}


		/// <summary>
		/// on level load, cache the local player and all of its standard components
		/// for use by the globally accessible 'fp_LocalPlayer' wrapper
		/// </summary>
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
