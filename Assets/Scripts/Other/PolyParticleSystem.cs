using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class PolyParticleSystem : MonoBehaviour
{
    [SerializeField] ParticleSystem[] ParticleSystems;
    [SerializeField] bool Emitting;

    private void Update()
    {
        Emitting = IsEmitting();
    }

    public void Play()
    {
        foreach (ParticleSystem system in ParticleSystems)
            system.Play();
    }

    public void Pause()
    {
        foreach (ParticleSystem system in ParticleSystems)
            system.Pause();
    }

    public void Stop()
    {
        foreach (ParticleSystem system in ParticleSystems)
            system.Stop();
    }

    public void StopAndClear()
    {
        foreach (ParticleSystem system in ParticleSystems)
        {
            system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            //system.Stop();
            //system.Clear();
        }
    }

    public void ForceStop()
    {
        foreach (ParticleSystem system in ParticleSystems)
        {
            system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            var emission = system.emission;
            emission.enabled = false;
        }
    }

    public bool IsEmitting()
    {
        return ParticleSystems.Any(s => s.isEmitting);
    }
}
