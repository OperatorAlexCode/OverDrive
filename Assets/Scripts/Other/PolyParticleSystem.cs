using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolyParticleSystem : MonoBehaviour
{
    [SerializeField] ParticleSystem[] ParticleSystems;

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
            system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public bool IsEmitting()
    {
        return ParticleSystems.Any(s => s.isEmitting);
    }
}
