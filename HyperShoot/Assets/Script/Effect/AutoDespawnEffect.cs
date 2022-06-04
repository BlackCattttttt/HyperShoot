using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDespawnEffect : MonoBehaviour
{
    public ParticleSystem particle;
    public bool getTimeLifeFromParticle = true;
    public bool getDurationFromParticle = false;
    public bool autoBeChildOfEffectContainer = false;

    public float lifeTime = 1f;

    private float currentLifeTime;

    void Awake()
    {
        if (particle == null)
            particle = GetComponent<ParticleSystem>();
        if (particle != null)
        {
            if (getTimeLifeFromParticle)
                lifeTime = (particle.startLifetime + particle.startDelay);
            if (getDurationFromParticle)
                lifeTime = particle.duration;
        }
    }

    void OnEnable()
    {
        if (particle != null)
            particle.Play();

        currentLifeTime = lifeTime;
    }

    private void Update()
    {
        if (currentLifeTime > 0)
        {
            currentLifeTime -= Time.deltaTime;

            if (currentLifeTime <= 0)
            {
                if (autoBeChildOfEffectContainer)
                {
                    SimplePool.Despawn(gameObject, EffectContainer.Instance.transform);

                }
                else
                    SimplePool.Despawn(gameObject);
            }
        }
    }

    public void SetLifeTime(float time)
    {
        lifeTime = time;

        if (particle != null)
        {
            foreach (ParticleSystem item in particle.GetComponentsInChildren<ParticleSystem>())
            {
                item.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = item.main;
                main.duration = lifeTime;
            }
        }
        if (!particle.isEmitting)
            particle.Play();
        currentLifeTime = lifeTime;

    }
}
