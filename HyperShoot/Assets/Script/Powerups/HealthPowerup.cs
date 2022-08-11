using UnityEngine;
using System.Collections;
using HyperShoot.Player;

public class HealthPowerup : Powerup
{
    public float Health = 1.0f;

    protected override bool TryGive(CharacterEventHandler player)
    {
        if (player.Health.Get() < 0.0f)
            return false;

        if (player.Health.Get() >= player.MaxHealth.Get())
            return false;

        // if this is singleplayer or we are a multiplayer master, update health
        player.Health.Set(Mathf.Min(player.MaxHealth.Get(), (player.Health.Get() + Health)));

        return true;

    }

}
