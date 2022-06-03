using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectContainer : Singleton<EffectContainer>
{
    [SerializeField] private GameObject explosionChassis;
    [SerializeField] private GameObject hit1Particle;

    public void SpawnExplosionEffect(Vector3 pos)
    {
        if (explosionChassis == null) return;
        var obj = Instantiate(explosionChassis, transform);
        obj.transform.position = pos;
    }

    public void SpawnHit1Effect(Vector3 pos)
    {
        if (hit1Particle == null) return;
        var obj = Instantiate(hit1Particle, transform);
        obj.transform.position = pos;
    }
}