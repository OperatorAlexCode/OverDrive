using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Behemoth : Destroyer
{
    // Float | Missile
    [SerializeField] float MissileTurnSpeed;
    [SerializeField] float MissileAcceleration;
    [SerializeField] float MissileLockOnDistance;
    [SerializeField] float MissileSeekingCone;
    // Float | Turrets
    [SerializeField] float TurretLaserSpeed;
    [SerializeField] float TurretCooldown;
    [SerializeField] float TurretRange;
    [SerializeField] float TurretRotationSpeed;
    [SerializeField] float TurretMinimumFiringAngle;

    // Other
    [SerializeField] int TurretDamage;
    [SerializeField] List<LaserTurret> Turrets;

    protected override void EnemyInitialization()
    {
        base.EnemyInitialization();

        foreach (LaserTurret turret in Turrets)
        {
            turret.Set(TurretDamage, TurretLaserSpeed, TurretCooldown, TurretRange, TurretRotationSpeed, TurretMinimumFiringAngle);
            turret.SetTags("EnemyProjectile", "Player");
            turret.SetColor(GetComponent<SpriteRenderer>().color);
            turret.SetProjectileColor(GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>().EnemyProjectilesColor);
            //turret.SetLayerMasks(ProjectileHitMask, LayerMask.NameToLayer("Player"));
            turret.Disable(false);
        }

        Missile warhead = new Missile();
        warhead.Set(Damage, "Player", MissileTurnSpeed, MissileAcceleration, MissileLockOnDistance, MissileSeekingCone);
        Warhead = warhead;

        if (LoadedTorpedo != null)
            LoadedTorpedo.GetComponent<Torpedo>().ProjectileDeath(true);

        CanFire = true;
    }

    public override void EnemyLogic()
    {
        SeekPlayer();

        if (Vector3.Distance(transform.position, PlayerPos) <= Range && !Player.GetComponent<PlayerController>().IsDead)
        {
            if (Rb.velocity.magnitude >= 0.5f * MaxVelocity)
                Rb.AddForce(-(Rb.velocity.normalized * Acceleration * Time.deltaTime), ForceMode2D.Force);

            if (CanFire && Vector2.Angle(transform.up, PlayerPos - transform.position) <= MinimumFiringAngle)
                StartCoroutine(Fire());
        }

        CapVelocity();
    }

    protected override IEnumerator Fire()
    {
        CanFire = false;

        LoadTorpedo();

        Transform torpedo = LoadedTorpedo.transform;
        torpedo.GetComponent<Torpedo>().Fire();
        torpedo.SetParent(null);
        LoadedTorpedo = null;

        yield return new WaitForSeconds(FireDelay);
        CanFire = true;
    }

    protected override IEnumerator EnemyDeath()
    {
        foreach (LaserTurret turret in Turrets)
            turret.Disable(true);

        foreach (Renderer renderer in GetComponentsInChildren<SpriteRenderer>())
            renderer.enabled = false;

        StartCoroutine(base.EnemyDeath());
        yield return null;
    }
}
