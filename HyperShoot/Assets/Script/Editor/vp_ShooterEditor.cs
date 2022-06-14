/////////////////////////////////////////////////////////////////////////////////
//
//	fp_ShooterEditor.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	custom inspector for the fp_Shooter class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using HyperShoot.Weapon;

[CustomEditor(typeof(BaseShooter))]

public class fp_ShooterEditor : Editor
{

	// target component
	public BaseShooter m_Component = null;

	// foldouts
	public static bool m_ProjectileFoldout;
	public static bool m_MuzzleFlashFoldout;
	public static bool m_ShellFoldout;
	public static bool m_AmmoFoldout;
	public static bool m_SoundFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private bool m_MuzzleFlashVisible = false;		// display the muzzle flash in the editor?
	private static fp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (BaseShooter)target;

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

		string objectInfo = m_Component.gameObject.name;

		if (fp_Utility.IsActive(m_Component.gameObject))
			GUI.enabled = true;
		else
		{
			GUI.enabled = false;
			objectInfo += " (INACTIVE)";
		}

		if (!fp_Utility.IsActive(m_Component.gameObject))
		{
			GUI.enabled = true;
			return;
		}

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			DoProjectileFoldout();
			DoMuzzleFlashFoldout();

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
	/// 
	/// </summary>
	public virtual void DoProjectileFoldout()
	{

		m_ProjectileFoldout = EditorGUILayout.Foldout(m_ProjectileFoldout, "Projectile");
		if (m_ProjectileFoldout)
		{

			m_Component.ProjectileFiringRate = Mathf.Max(0.0f, EditorGUILayout.FloatField("Firing Rate", m_Component.ProjectileFiringRate));
			if (m_Component.ProjectileFiringRate == 0.0f)
			{
				GUI.enabled = false;
			}
			GUI.enabled = true;
			m_Component.ProjectilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ProjectilePrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a gameobject with a projectile\nlogic script added to it (such as fp_HitscanBullet).", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.ProjectileScale = EditorGUILayout.Slider("Scale", m_Component.ProjectileScale, 0, 2);
			m_Component.ProjectileCount = EditorGUILayout.IntField("Count", m_Component.ProjectileCount);
			m_Component.ProjectileSpread = EditorGUILayout.Slider("Spread", m_Component.ProjectileSpread, 0, 360);
			m_Component.ProjectileSpawnDelay = Mathf.Abs(EditorGUILayout.FloatField("Spawn Delay", m_Component.ProjectileSpawnDelay));
			m_Component.ProjectileSourceIsRoot = EditorGUILayout.Toggle("Root Obj. is Source", m_Component.ProjectileSourceIsRoot);
			GUI.enabled = true;

			fp_EditorGUIUtility.Separator();

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoMuzzleFlashFoldout()
	{

		m_MuzzleFlashFoldout = EditorGUILayout.Foldout(m_MuzzleFlashFoldout, "Muzzle Flash");
		if (m_MuzzleFlashFoldout)
		{

			m_Component.MuzzleFlashPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.MuzzleFlashPrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a mesh with a Particles/Additive\nshader and a fp_MuzzleFlash script added to it.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			Vector3 currentPosition = m_Component.MuzzleFlashPosition;
			m_Component.MuzzleFlashPosition = EditorGUILayout.Vector3Field("Position", m_Component.MuzzleFlashPosition);
			Vector3 currentScale = m_Component.MuzzleFlashScale;
			m_Component.MuzzleFlashScale = EditorGUILayout.Vector3Field("Scale", m_Component.MuzzleFlashScale);
			m_Component.MuzzleFlashFadeSpeed = EditorGUILayout.Slider("Fade Speed", m_Component.MuzzleFlashFadeSpeed, 0.001f, 0.2f);
			m_Component.MuzzleFlashDelay = Mathf.Abs(EditorGUILayout.FloatField("MuzzleFlash Delay", m_Component.MuzzleFlashDelay));
			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentMuzzleFlashVisible = m_MuzzleFlashVisible;
			m_MuzzleFlashVisible = EditorGUILayout.Toggle("Show Muzzle Fl.", m_MuzzleFlashVisible);
			if (Application.isPlaying)
			{
				if (m_Component.MuzzleFlashPosition != currentPosition ||
					m_Component.MuzzleFlashScale != currentScale)
					m_MuzzleFlashVisible = true;

				MuzzleFlash mf = (MuzzleFlash)m_Component.MuzzleFlash.GetComponent("fp_MuzzleFlash");
				if (mf != null)
					mf.ForceShow = currentMuzzleFlashVisible;

				GUI.enabled = false;
				GUILayout.Label("Set Muzzle Flash Z to about 0.5 to bring it into view.", fp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Muzzle Flash can be shown when the game is playing.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;

			fp_EditorGUIUtility.Separator();
		}
	}
}

