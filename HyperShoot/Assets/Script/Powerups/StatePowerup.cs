using UnityEngine;
using System.Collections;
using HyperShoot.Player;

public class StatePowerup : Powerup
{
	public string State = "MegaSpeed";
	public float Duration = 0.0f;

	// timer handle to manage multiple timers
	protected fp_Timer.Handle m_Timer = new fp_Timer.Handle();

	protected override void Update()
	{
		// remove powerup if depleted and silent
		if (m_Depleted)
		{
			if (!m_Audio.isPlaying)
				Remove();
		}	
	}

	protected override bool TryGive(CharacterEventHandler player)
	{
		if (m_Timer.Active)
			return false;

		if (string.IsNullOrEmpty(State))
			return false;

		player.SetState(State);

		fp_Timer.In(((Duration <= 0.0f) ? RespawnDuration : Duration), delegate()
		{
			player.SetState(State, false);
		}, m_Timer);

		return true;
	}
}
