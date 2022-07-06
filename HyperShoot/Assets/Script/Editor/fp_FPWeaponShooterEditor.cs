/////////////////////////////////////////////////////////////////////////////////
//
//	fp_FPWeaponShooterEditor.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	custom inspector for the fp_FPWeaponShooter class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using HyperShoot.Weapon;

[CustomEditor(typeof(FPWeaponShooter))]

public class fp_FPWeaponShooterEditor : Editor
{

	// target component
	public FPWeaponShooter m_Component = null;

	// foldouts
	public static bool m_ProjectileFoldout;
	public static bool m_MotionFoldout;
	public static bool m_MuzzleFlashFoldout;
	public static bool m_ShellFoldout;
	public static bool m_AmmoFoldout;
	public static bool m_SoundFoldout;
	public static bool m_AnimationFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private bool m_MuzzleFlashVisible = false;		// display the muzzle flash in the editor?
	private static fp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (FPWeaponShooter)target;

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

			DoProjectileFoldout();
			DoMotionFoldout();
			DoMuzzleFlashFoldout();
			DoSoundFoldout();
			DoAnimationFoldout();

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
			m_Component.ProjectileTapFiringRate = Mathf.Min(Mathf.Max(0.0f, EditorGUILayout.FloatField("Tap Firing Rate", m_Component.ProjectileTapFiringRate)), m_Component.ProjectileFiringRate);
			GUI.enabled = true;
			GUI.enabled = false;
			GUILayout.Label("TIP: Set Firing Rate to zero if you want to use the length of the\nFire animation as Firing Rate (this will disable Tap Firing).", fp_EditorGUIUtility.NoteStyle);
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

			GUI.enabled = false;
			if (m_Component.m_ProjectileSpawnPoint != null)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Spawn Point");
				EditorGUILayout.LabelField(m_Component.m_ProjectileSpawnPoint.ToString());
				EditorGUILayout.EndHorizontal();
			}
			GUI.enabled = true;

			fp_EditorGUIUtility.Separator();

		}

	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoMotionFoldout()
	{

		m_MotionFoldout = EditorGUILayout.Foldout(m_MotionFoldout, "Motion");
		if (m_MotionFoldout)
		{

			m_Component.MotionPositionRecoil = EditorGUILayout.Vector3Field("Position Recoil", m_Component.MotionPositionRecoil);
			m_Component.MotionRotationRecoil = EditorGUILayout.Vector3Field("Rotation Recoil", m_Component.MotionRotationRecoil);
			m_Component.MotionRotationRecoilDeadZone = EditorGUILayout.Slider("Rot. Recoil Dead Zone", m_Component.MotionRotationRecoilDeadZone, 0.0f, 1.0f);
			GUI.enabled = false;
			GUILayout.Label("Recoil forces are added to the secondary position and\nrotation springs of the weapon. Dead Zone limits the minimum\nZ rotation. TIP: A high Dead Zone gives sharper Z twist.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.MotionPositionReset = EditorGUILayout.Slider("Position Reset", m_Component.MotionPositionReset, 0, 1);
			m_Component.MotionRotationReset = EditorGUILayout.Slider("Rotation Reset", m_Component.MotionRotationReset, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Upon firing, primary position and rotation springs\nwill snap back to their rest state by this factor.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.MotionPositionPause = EditorGUILayout.Slider("Position Pause", m_Component.MotionPositionPause, 0, 5);
			m_Component.MotionRotationPause = EditorGUILayout.Slider("Rotation Pause", m_Component.MotionRotationPause, 0, 5);
			GUI.enabled = false;
			GUILayout.Label("Upon firing, primary spring forces will pause and\nease back in over this time interval in seconds.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			m_Component.MotionDryFireRecoil = EditorGUILayout.Slider("Dry Fire Recoil", m_Component.MotionDryFireRecoil, -1, 1);
			m_Component.MotionRecoilDelay = Mathf.Abs(EditorGUILayout.FloatField("Recoil Delay", m_Component.MotionRecoilDelay));

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
			m_Component.MuzzleFlashDelay = Mathf.Abs(EditorGUILayout.FloatField("Muzzle Flash Delay", m_Component.MuzzleFlashDelay));
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
                if (mf != null) {
                    mf.transform.localPosition = currentPosition;
                    mf.transform.localScale = currentScale;
                    mf.ForceShow = currentMuzzleFlashVisible;
                }

                GUI.enabled = false;
				GUILayout.Label("Set Muzzle Flash Z to about 0.5 to bring it into view.", fp_EditorGUIUtility.NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Muzzle Flash can be shown when the game is playing.", fp_EditorGUIUtility.NoteStyle);
			GUI.enabled = true;
			//m_Component.MuzzleFlashFirstShotMaxDeviation = EditorGUILayout.Slider("1st Shot Max Deviation", m_Component.MuzzleFlashFirstShotMaxDeviation, 0, 180);	// NOTE: currently broken

			fp_EditorGUIUtility.Separator();
		}

	}



    /// <summary>
    /// 
    /// </summary>
    public virtual void DoSoundFoldout()
    {

        m_SoundFoldout = EditorGUILayout.Foldout(m_SoundFoldout, "Sound");
        if (m_SoundFoldout)
        {
            m_Component.SoundFire = (AudioClip)EditorGUILayout.ObjectField("Fire", m_Component.SoundFire, typeof(AudioClip), false);
            m_Component.SoundDryFire = (AudioClip)EditorGUILayout.ObjectField("Dry Fire", m_Component.SoundDryFire, typeof(AudioClip), false);
            //m_Component.SoundReload = (AudioClip)EditorGUILayout.ObjectField("Reload", m_Component.SoundReload, typeof(AudioClip), false);
            m_Component.SoundFirePitch = EditorGUILayout.Vector2Field("Fire Pitch (Min:Max)", m_Component.SoundFirePitch);
            EditorGUILayout.MinMaxSlider(ref m_Component.SoundFirePitch.x, ref m_Component.SoundFirePitch.y, 0.5f, 1.5f);
            m_Component.SoundFireDelay = Mathf.Abs(EditorGUILayout.FloatField("Fire Sound Delay", m_Component.SoundFireDelay));
            fp_EditorGUIUtility.Separator();
        }

    }


    /// <summary>
    /// 
    /// </summary>
    public virtual void DoAnimationFoldout()
	{

		m_AnimationFoldout = EditorGUILayout.Foldout(m_AnimationFoldout, "Animation");
		if (m_AnimationFoldout)
		{
			m_Component.AnimationFire = (AnimationClip)EditorGUILayout.ObjectField("Fire", m_Component.AnimationFire, typeof(AnimationClip), false);
			m_Component.AnimationOutOfAmmo = (AnimationClip)EditorGUILayout.ObjectField("OutOfAmmo", m_Component.AnimationOutOfAmmo, typeof(AnimationClip), false);
			fp_EditorGUIUtility.Separator();
		}

	}

		
}

