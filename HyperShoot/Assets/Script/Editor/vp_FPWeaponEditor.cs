/////////////////////////////////////////////////////////////////////////////////
//
//	fp_FPWeaponEditor.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	custom inspector for the fp_FPSWeapon class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using HyperShoot.Weapon;

[CustomEditor(typeof(FPWeapon))]

public class fp_FPWeaponEditor : Editor
{

	// target component
	public FPWeapon m_Component = null;

	// weapon foldouts
	// NOTE: these are static so they remain open when toggling
	// between different components. this simplifies copying
	// content (prefabs / sounds) between components
	public static bool m_WeaponRenderingFoldout;
	public static bool m_WeaponPositionFoldout;
	public static bool m_WeaponRotationFoldout;
	public static bool m_WeaponRetractionFoldout;
	public static bool m_WeaponShakeFoldout;
	public static bool m_WeaponBobFoldout;
	public static bool m_WeaponStepFoldout;
	public static bool m_WeaponIdleFoldout;
	public static bool m_WeaponSoundFoldout;
	public static bool m_WeaponAnimationFoldout;
	public static bool m_LookDownFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private bool m_WeaponPivotVisible = false;
	private static fp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the FPSCamera object to the inspector target
	/// </summary>
	public void OnEnable()
	{

		m_Component = (FPWeapon)target;

		if (m_Persister == null)
			m_Persister = new fp_ComponentPersister();
		m_Persister.Component = m_Component;
		m_Persister.IsActive = true;

		if (m_Component.DefaultState == null)
			m_Component.RefreshDefaultState();
		
	}


	/// <summary>
	/// disables the persister and removes its reference
	/// </summary>
	void OnDestroy()
	{

		if (m_Persister != null)
			m_Persister.IsActive = false;

	}

	
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			DoRenderingFoldout();
			DoPositionFoldout();
			DoRotationFoldout();
			DoSoundFoldout();
			DoAnimationFoldout();

		}
		else
			fp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// state foldout
		m_StateFoldout = fp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// preset foldout
		m_PresetFoldout = fp_PresetEditorGUIUtility.PresetFoldout(m_PresetFoldout, m_Component);

		// update default state and persist in order not to loose inspector tweaks
		// due to state switches during runtime - UNLESS a runtime state button has
		// been pressed (in which case user wants to toggle states as opposed to
		// reset / alter them)
		if (GUI.changed &&
			(!fp_PresetEditorGUIUtility.RunTimeStateButtonTarget == m_Component))
		{

			EditorUtility.SetDirty(target);

			if (Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();
	
			m_Component.Refresh();

		}
		
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRenderingFoldout()
	{

		m_WeaponRenderingFoldout = EditorGUILayout.Foldout(m_WeaponRenderingFoldout, "Rendering");
		if (m_WeaponRenderingFoldout)
		{

			// weapon model
			GameObject model = m_Component.WeaponPrefab;
			m_Component.WeaponPrefab = (GameObject)EditorGUILayout.ObjectField("1st Person Weapon (Prefab)", m_Component.WeaponPrefab, typeof(GameObject), false);
			if (Application.isPlaying && model != m_Component.WeaponPrefab)
			{
				m_Component.InstantiateWeaponModel();
			}
			
			if (Application.isPlaying && ((m_Component.WeaponCamera == null) || (m_Component.WeaponCamera != null && !fp_Utility.IsActive(m_Component.WeaponCamera.gameObject))))
				GUI.enabled = false;
			// weapon fov
			Vector2 fovDirty = new Vector2(0.0f, m_Component.RenderingFieldOfView);
			m_Component.RenderingFieldOfView = EditorGUILayout.Slider("Field of View", m_Component.RenderingFieldOfView, 1, 179);
			if (fovDirty != new Vector2(0.0f, m_Component.RenderingFieldOfView))
				m_Component.Zoom();
			m_Component.RenderingZoomDamping = EditorGUILayout.Slider("Zoom Damping", m_Component.RenderingZoomDamping, 0.1f, 5.0f);

			// weapon clipping planes
			m_Component.RenderingClippingPlanes = EditorGUILayout.Vector2Field("Clipping Planes (Near:Far)", m_Component.RenderingClippingPlanes);

			if (GUI.enabled == false)
			{
				GUI.enabled = false;
				GUILayout.Label("The above parameters require an active weapon camera.\nSee the manual for more info.", fp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			
			GUI.enabled = true;

			float zScale = m_Component.RenderingZScale;
			m_Component.RenderingZScale = EditorGUILayout.Slider("Z Scale", m_Component.RenderingZScale, 0.0f, 1.0f);
			if ((m_Component.RenderingZScale != zScale) && Application.isPlaying && m_Component.WeaponModel != null)
				m_Component.WeaponModel.transform.localScale = new Vector3(1, 1, m_Component.RenderingZScale);
			
			GUI.enabled = false;
			GUILayout.Label("Z Scale can be used for tweaking the appearance of the\nweapon when running the system without a weapon camera.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			
			fp_EditorGUIUtility.Separator();

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoPositionFoldout()
	{

		m_WeaponPositionFoldout = EditorGUILayout.Foldout(m_WeaponPositionFoldout, "Position Springs");
		if (m_WeaponPositionFoldout)
		{

			m_Component.PositionOffset = EditorGUILayout.Vector3Field("Offset", m_Component.PositionOffset);
			m_Component.PositionExitOffset = EditorGUILayout.Vector3Field("Exit Offset", m_Component.PositionExitOffset);
			Vector3 currentPivot = m_Component.PositionPivot;
			m_Component.PositionPivot = EditorGUILayout.Vector3Field("Pivot", m_Component.PositionPivot);
			m_Component.PositionPivotSpringStiffness = EditorGUILayout.Slider("Pivot Stiffness", m_Component.PositionPivotSpringStiffness, 0, 1);
			m_Component.PositionPivotSpringDamping = EditorGUILayout.Slider("Pivot Damping", m_Component.PositionPivotSpringDamping, 0, 1);

			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentPivotVisible = m_WeaponPivotVisible;
			m_WeaponPivotVisible = EditorGUILayout.Toggle("Show Pivot", m_WeaponPivotVisible);
			if (Application.isPlaying)
			{
				if (m_Component.PositionPivot != currentPivot)
				{
					m_Component.SnapPivot();
					m_WeaponPivotVisible = true;
				}
				if (currentPivotVisible != m_WeaponPivotVisible)
					m_Component.SetPivotVisible(m_WeaponPivotVisible);
				GUI.enabled = false;
				GUILayout.Label("Set Pivot Z to about -0.5 to bring it into view.", fp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Pivot can be shown when the game is playing.", fp_EditorGUIUtility.NoteStyle);

			GUI.enabled = true;

			m_Component.PositionSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.PositionSpringStiffness, 0, 1);
			m_Component.PositionSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.PositionSpringDamping, 0, 1);
			m_Component.PositionSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.PositionSpring2Stiffness, 0, 1);
			m_Component.PositionSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.PositionSpring2Damping, 0, 1);

			GUI.enabled = false;
			GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.PositionKneeling = EditorGUILayout.Slider("Kneeling", m_Component.PositionKneeling, 0, 1);
			m_Component.PositionKneelingSoftness = EditorGUILayout.IntSlider("Kneeling Softness", m_Component.PositionKneelingSoftness, 1, 30);
			GUI.enabled = false;
			GUILayout.Label("Kneeling is positional down force upon fall impact. Softness is \nthe number of frames over which to even out each fall impact.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.PositionWalkSlide = EditorGUILayout.Vector3Field("Walk Sliding", m_Component.PositionWalkSlide);
			m_Component.PositionMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.PositionMaxInputVelocity);

			fp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRotationFoldout()
	{

		m_WeaponRotationFoldout = EditorGUILayout.Foldout(m_WeaponRotationFoldout, "Rotation Springs");
		if (m_WeaponRotationFoldout)
		{
			m_Component.RotationOffset = EditorGUILayout.Vector3Field("Offset", m_Component.RotationOffset);
			m_Component.RotationExitOffset = EditorGUILayout.Vector3Field("Exit Offset", m_Component.RotationExitOffset);
			Vector3 currentPivot = m_Component.PositionPivot;
			m_Component.RotationPivot = EditorGUILayout.Vector3Field("Pivot", m_Component.RotationPivot);
			m_Component.RotationPivotSpringStiffness = EditorGUILayout.Slider("Pivot Stiffness", m_Component.RotationPivotSpringStiffness, 0, 1);
			m_Component.RotationPivotSpringDamping = EditorGUILayout.Slider("Pivot Damping", m_Component.RotationPivotSpringDamping, 0, 1);

			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentPivotVisible = m_WeaponPivotVisible;
			m_WeaponPivotVisible = EditorGUILayout.Toggle("Show Pivot", m_WeaponPivotVisible);
			if (Application.isPlaying)
			{
				if (m_Component.PositionPivot != currentPivot)
				{
					m_Component.SnapPivot();
					m_WeaponPivotVisible = true;
				}
				if (currentPivotVisible != m_WeaponPivotVisible)
					m_Component.SetPivotVisible(m_WeaponPivotVisible);
			}
			else
				GUILayout.Label("Pivot can be shown when the game is playing.", fp_EditorGUIUtility.NoteStyle);

			GUI.enabled = true;
			m_Component.RotationSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.RotationSpringStiffness, 0, 1);
			m_Component.RotationSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.RotationSpringDamping, 0, 1);
			m_Component.RotationSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.RotationSpring2Stiffness, 0, 1);
			m_Component.RotationSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.RotationSpring2Damping, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.RotationKneeling = EditorGUILayout.Slider("Kneeling", m_Component.RotationKneeling, 0, 100);
			m_Component.RotationKneelingSoftness = EditorGUILayout.IntSlider("Kneeling Softness", m_Component.RotationKneelingSoftness, 1, 30);
			GUI.enabled = false;
			GUILayout.Label("Kneeling is downward pitch upon fall impact. Softness is the\nnumber of frames over which to even out each fall impact.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.RotationLookSway = EditorGUILayout.Vector3Field("Look Sway", m_Component.RotationLookSway);
			m_Component.RotationStrafeSway = EditorGUILayout.Vector3Field("Strafe Sway", m_Component.RotationStrafeSway);
			m_Component.RotationFallSway = EditorGUILayout.Vector3Field("Fall Sway", m_Component.RotationFallSway);
			m_Component.RotationSlopeSway = EditorGUILayout.Slider("Slope Sway", m_Component.RotationSlopeSway, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("SlopeSway multiplies FallSway when grounded\nand will take effect on slopes.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.RotationMaxInputVelocity = EditorGUILayout.FloatField("Max Input Rot.", m_Component.RotationMaxInputVelocity);

			fp_EditorGUIUtility.Separator();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public virtual void DoSoundFoldout()
	{

		m_WeaponSoundFoldout = EditorGUILayout.Foldout(m_WeaponSoundFoldout, "Sound");
		if (m_WeaponSoundFoldout)
		{
			m_Component.SoundWield = (AudioClip)EditorGUILayout.ObjectField("Wield", m_Component.SoundWield, typeof(AudioClip), false);
			m_Component.SoundUnWield = (AudioClip)EditorGUILayout.ObjectField("Unwield", m_Component.SoundUnWield, typeof(AudioClip), false);

			fp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoAnimationFoldout()
	{

		m_WeaponAnimationFoldout = EditorGUILayout.Foldout(m_WeaponAnimationFoldout, "Animation");
		if (m_WeaponAnimationFoldout)
		{


			m_Component.AnimationWield = (AnimationClip)EditorGUILayout.ObjectField("Wield", m_Component.AnimationWield, typeof(AnimationClip), false);
			m_Component.AnimationUnWield = (AnimationClip)EditorGUILayout.ObjectField("Unwield", m_Component.AnimationUnWield, typeof(AnimationClip), false);

			m_Component.AnimationGrip = (int)((BaseWeapon.Grip)EditorGUILayout.EnumPopup("Grip", (BaseWeapon.Grip)m_Component.AnimationGrip));
			m_Component.AnimationType = (int)((BaseWeapon.Type)EditorGUILayout.EnumPopup("Type", (BaseWeapon.Type)m_Component.AnimationType));

			GUI.enabled = false;
			GUILayout.Label("Should the character use one-handed or two-handed firearm\nor melee animations for this weapon in 3rd person?", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;

			fp_EditorGUIUtility.Separator();

		}

	}


}

