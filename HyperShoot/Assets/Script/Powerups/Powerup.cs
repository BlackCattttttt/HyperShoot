using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HyperShoot.Player;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]

public abstract class Powerup : MonoBehaviour
{
	protected Transform m_Transform = null;
	protected AudioSource m_Audio = null;
	protected Renderer m_Renderer = null;

	public List<string> RecipientTags = new List<string>();

	Collider m_LastCollider = null;
	CharacterEventHandler m_Recipient = null;

	public string GiveMessage = "Got a powerup!";
	public string FailMessage = "You currently can't get this powerup!";

	// position
	protected Vector3 m_SpawnPosition = Vector3.zero;
	protected Vector3 m_SpawnScale = Vector3.zero;

	// appearance
	public bool Billboard = false;
	// after triggered, the powerup will respawn in this many seconds
	public float RespawnDuration = 10.0f;
	public float RespawnScaleUpDuration = 0.0f;
	public float RemoveDuration = 0.0f;

	// sounds
	public AudioClip PickupSound = null;		// player triggers the powerup
	public AudioClip PickupFailSound = null;	// player failed to pick up the powerup (i.e. ammo full)
	public AudioClip RespawnSound = null;		// powerup respawns
	public bool PickupSoundSlomo = true;
	public bool FailSoundSlomo = true;
	public bool RespawnSoundSlomo = true;

	protected bool m_Depleted = false;

	protected bool m_AlreadyFailed = false;

	protected fp_Timer.Handle m_RespawnTimer = new fp_Timer.Handle();

	protected virtual void Start()
	{
		m_Transform = transform;
		m_Audio = GetComponent<AudioSource>();
		m_Renderer = GetComponent<Renderer>();

		// some default audio settings
		m_Audio.clip = PickupSound;
		m_Audio.playOnAwake = false;
		m_Audio.minDistance = 3;
		m_Audio.maxDistance = 150;
		m_Audio.rolloffMode = AudioRolloffMode.Linear;
		m_Audio.dopplerLevel = 0.0f;

		// store the initial position
		m_SpawnPosition = m_Transform.position;
		m_SpawnScale = m_Transform.localScale;

		if (RecipientTags.Count == 0)
			RecipientTags.Add("Player");

		if (RemoveDuration != 0.0f)
			fp_Timer.In(RemoveDuration, Remove);
	}

	protected virtual void Update()
	{
		if (m_Depleted && !m_Audio.isPlaying)
			Remove();
	}

	protected virtual void OnTriggerEnter(Collider col)
	{
		// only do something if the trigger is still active
		if (m_Depleted)
			return;

		// see if the colliding object was a valid recipient
		foreach(string s in RecipientTags)
		{
			if(col.gameObject.tag == s)
				goto isRecipient;
		}
		return;
		isRecipient:

		if (col != m_LastCollider)
			m_Recipient = col.gameObject.GetComponentInParent<CharacterEventHandler>();

		if (m_Recipient == null)
			return;

		if (TryGive(m_Recipient))
		{
			m_Audio.pitch = PickupSoundSlomo ? Time.timeScale : 1.0f;
			m_Audio.Play();
			m_Renderer.enabled = false;
			m_Depleted = true;
			//if(m_Recipient is vp_FPPlayerEventHandler)
			//	(m_Recipient as vp_FPPlayerEventHandler).HUDText.Send(GiveMessage);
		}
		else if (!m_AlreadyFailed)
		{
			//if (!vp_Gameplay.IsMultiplayer || (vp_Gameplay.IsMultiplayer && (m_Recipient is vp_FPPlayerEventHandler)))
			//{
			//	m_Audio.pitch = FailSoundSlomo ? Time.timeScale : 1.0f;
			//	m_Audio.PlayOneShot(PickupFailSound);
		//	}
			m_AlreadyFailed = true;
			//if (m_Recipient is vp_FPPlayerEventHandler)
			//	(m_Recipient as vp_FPPlayerEventHandler).HUDText.Send(FailMessage);
		}

	}

	protected virtual void OnTriggerExit(Collider col)
	{
		// reset fail status
		m_AlreadyFailed = false;
	}

	protected virtual bool TryGive(CharacterEventHandler player)
	{
		return true;
	}

	protected virtual void Remove()
	{
		if (this == null)
			return;

		if (RespawnDuration == 0.0f)
			fp_Utility.Destroy(gameObject);
		else
		{
			if (!m_RespawnTimer.Active)
			{
				fp_Utility.Activate(gameObject, false);
				fp_Timer.In(RespawnDuration, Respawn, m_RespawnTimer);
			}
		}
	}

	protected virtual void Respawn()
	{
		if (m_Transform == null)
			return;

		m_RespawnTimer.Cancel();	// cancel timer in case we didn't get here via timer

		m_Transform.position = m_SpawnPosition;

		if (RespawnScaleUpDuration > 0.0f)
			m_Transform.localScale = Vector3.zero;

		m_Renderer.enabled = true;
		fp_Utility.Activate(gameObject);
		m_Audio.pitch = (RespawnSoundSlomo ? Time.timeScale : 1.0f);
		m_Audio.PlayOneShot(RespawnSound);
		m_Depleted = false;
	}
}
