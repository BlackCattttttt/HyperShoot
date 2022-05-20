using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperShoot.Player;

public class Crosshair : MonoBehaviour
{
	public Texture m_ImageCrosshair = null;

	public bool Hide = false;                  
	public bool HideOnFirstPersonZoom = true;
	public bool HideOnDeath = true;

	protected FPCharacterEventHandler m_Player = null;


	protected virtual void Awake()
	{
	m_Player = GameObject.FindObjectOfType(typeof(FPCharacterEventHandler)) as FPCharacterEventHandler; // cache the player event handler

	}

	protected virtual void OnEnable()
	{
		if (m_Player != null)
			m_Player.Register(this);
	}

	protected virtual void OnDisable()
	{
		if (m_Player != null)
			m_Player.Unregister(this);
	}

	void OnGUI()
	{
		if (m_ImageCrosshair == null)
			return;

		if (Hide)
			return;

		if (HideOnFirstPersonZoom && m_Player.Zoom.Active && m_Player.IsFirstPerson.Get())
			return;

		if (HideOnDeath && m_Player.Dead.Active)
			return;

		GUI.color = new Color(1, 1, 1, 0.8f);
		GUI.DrawTexture(new Rect((Screen.width * 0.5f) - (m_ImageCrosshair.width * 0.5f),
			(Screen.height * 0.5f) - (m_ImageCrosshair.height * 0.5f), m_ImageCrosshair.width,
			m_ImageCrosshair.height), m_ImageCrosshair);
		GUI.color = Color.white;

	}

	protected virtual Texture OnValue_Crosshair
	{
		get { return m_ImageCrosshair; }
		set { m_ImageCrosshair = value; }
	}
}
