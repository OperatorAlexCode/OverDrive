using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Projectile
{
    /// <summary>
    /// How many enemies the laser can penetrate
    /// </summary>
    [SerializeField]
    int Penetration;

    public override void ProjectileDeath(bool silentDestuction = false)
    {
        if (Penetration-- <= 0 || Age >= LifeTime)
        {
            if (GetComponent<ParticleSystem>().isPlaying)
                GetComponent<ParticleSystem>().Stop();

            var particleSystem = GetComponent<ParticleSystem>().main;
            particleSystem.stopAction = ParticleSystemStopAction.Destroy;
            base.ProjectileDeath(silentDestuction);
        }
        else
        {
            GetComponent<ParticleSystem>().Play();

            if (!HitSfx.isPlaying)
                HitSfx.Play();
        }
    }

    public void SetPenetration(int newPenetration)
    {
        Penetration = newPenetration;
    }

    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
        var main = GetComponent<ParticleSystem>().main;
        main.startColor = newColor;
    }
}
