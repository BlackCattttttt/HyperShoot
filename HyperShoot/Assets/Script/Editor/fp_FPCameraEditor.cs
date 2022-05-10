/////////////////////////////////////////////////////////////////////////////////
//
//	fp_FPCameraEditor.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	custom inspector for the fp_FPCamera class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using HyperShoot.Player;

[CustomEditor(typeof(FPCamera))]

public class fp_FPCameraEditor : Editor
{

	// target component
	private FPCamera m_Component = null;

	// camera foldouts
	public static bool m_CameraMouseFoldout;
	public static bool m_CameraRenderingFoldout;
	public static bool m_CameraRotationFoldout;
	public static bool m_CameraPositionFoldout;
	public static bool m_CameraShakeFoldout;
	public static bool m_CameraBobFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private static fp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the FPSCamera object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (FPCamera)target;

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
	public virtual void OnDestroy()
	{

		if (m_Persister != null)
			m_Persister.IsActive = false;

	}
	

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			DoMouseFoldout();
			DoRenderingFoldout();
			DoPositionFoldout();
			DoRotationFoldout();

		}
		else
			fp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// state
		m_StateFoldout = fp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// preset
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
	/// TODO: remove this soon
	/// </summary>
	public virtual void DoMouseFoldout()
	{

		m_CameraMouseFoldout = EditorGUILayout.Foldout(m_CameraMouseFoldout, "Mouse");

		if (m_CameraMouseFoldout)
			EditorGUILayout.HelpBox("Mouse look settings have been moved to the 'fp_FPInput' component. For more information, please see the release notes of UFPS v1.4.8.", MessageType.Info);

		fp_EditorGUIUtility.Separator();

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRenderingFoldout()
	{

		m_CameraRenderingFoldout = EditorGUILayout.Foldout(m_CameraRenderingFoldout, "Rendering");
		if (m_CameraRenderingFoldout)
		{
			Vector2 fovDirty = new Vector2(m_Component.RenderingFieldOfView, 0.0f);
			m_Component.RenderingFieldOfView = EditorGUILayout.Slider("Field of View", m_Component.RenderingFieldOfView, 1, 179);
			if (fovDirty != new Vector2(m_Component.RenderingFieldOfView, 0.0f))
				m_Component.Zoom();
			m_Component.RenderingZoomDamping = EditorGUILayout.Slider("Zoom Damping", m_Component.RenderingZoomDamping, 0.0f, 5.0f);
			//m_Component.DisableVRModeOnStartup = EditorGUILayout.Toggle("Disable VR mode on startup", m_Component.DisableVRModeOnStartup);

			fp_EditorGUIUtility.Separator();
		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoPositionFoldout()
	{

		m_CameraPositionFoldout = EditorGUILayout.Foldout(m_CameraPositionFoldout, "Position Spring");
		if (m_CameraPositionFoldout)
		{

		//	m_Component.DrawCameraCollisionDebugLine = true;

			m_Component.PositionOffset = EditorGUILayout.Vector3Field("Offset", m_Component.PositionOffset);
			m_Component.PositionOffset.y = Mathf.Max(m_Component.PositionGroundLimit, m_Component.PositionOffset.y);
			m_Component.PositionGroundLimit = EditorGUILayout.Slider("Ground Limit", m_Component.PositionGroundLimit, -5, 5);
			m_Component.PositionSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.PositionSpringStiffness, 0, 1);
			m_Component.PositionSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.PositionSpringDamping, 0, 1);
			//m_Component.PositionSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.PositionSpring2Stiffness, 0, 1);
			//m_Component.PositionSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.PositionSpring2Damping, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is a scripting feature. See the docs for usage.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			//m_Component.PositionKneeling = EditorGUILayout.Slider("Kneeling", m_Component.PositionKneeling, 0, 0.5f);
			//m_Component.PositionKneelingSoftness = EditorGUILayout.IntSlider("Kneeling Softness", m_Component.PositionKneelingSoftness, 1, 30);
			GUI.enabled = false;
			GUILayout.Label("Kneeling is down force upon fall impact. Softness is the\nnumber of frames over which to even out each fall impact.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			

			fp_EditorGUIUtility.Separator();
		}
		//else
			//m_Component.DrawCameraCollisionDebugLine = false;

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoRotationFoldout()
	{

		m_CameraRotationFoldout = EditorGUILayout.Foldout(m_CameraRotationFoldout, "Rotation Spring");
		if (m_CameraRotationFoldout)
		{
			m_Component.RotationPitchLimit = EditorGUILayout.Vector2Field("Pitch Limit (Min:Max)", m_Component.RotationPitchLimit);
			EditorGUILayout.MinMaxSlider(ref m_Component.RotationPitchLimit.y, ref m_Component.RotationPitchLimit.x, -90.0f, 90.0f);
			m_Component.RotationYawLimit = EditorGUILayout.Vector2Field("Yaw Limit (Min:Max)", m_Component.RotationYawLimit);
			EditorGUILayout.MinMaxSlider(ref m_Component.RotationYawLimit.x, ref m_Component.RotationYawLimit.y, -360.0f, 360.0f);
			//m_Component.RotationKneeling = EditorGUILayout.Slider("Kneeling", m_Component.RotationKneeling, 0, 0.5f);
			//m_Component.RotationKneelingSoftness = EditorGUILayout.IntSlider("Kneeling Softness", m_Component.RotationKneelingSoftness, 1, 30);
			m_Component.RotationSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.RotationSpringStiffness, 0, 1);
			m_Component.RotationSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.RotationSpringDamping, 0, 1);

			fp_EditorGUIUtility.Separator();
		}

	}
}

