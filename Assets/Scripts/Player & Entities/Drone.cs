using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public class Drone : MonoBehaviour
{
    // Float | Ship
    [SerializeField] protected float Acceleration;
    [SerializeField] protected float RotationalSpeed;
    [SerializeField] protected float MaxVelocity;

    // Float | Weapon
    [SerializeField] protected float FireDelay;
    [SerializeField] protected float Range;
    [SerializeField] protected float ProjectileLaunchSpeed;
    [SerializeField] protected float MinimumFiringAngle;

    // Float | Other
    [SerializeField] protected float DespawnDistance;
    [SerializeField] protected float NextWaypointDistance;
    [SerializeField] protected float StunDuration;

    // Int
    [SerializeField] protected int Health;
    [SerializeField] protected int Damage;
    protected int CurrentWaypoint;

    // GameObject
    [SerializeField] protected GameObject Projectile;
    protected GameObject Player;
    protected Vector3 PlayerPos;

    // Particle System
    [SerializeField] protected ParticleSystem[] EngineExhaust;
    [SerializeField] protected ParticleSystem Explosion;

    // bool
    protected bool IsDead;
    protected bool CanFire = true;
    protected bool Stunned;
    [SerializeField] protected bool CanBeStunned;

    // Audio Source
    [SerializeField] protected AudioSource HitSfx;
    [SerializeField] protected AudioSource DeathSfx;
    [SerializeField] protected AudioSource FireSfx;
    [SerializeField] protected AudioSource EnginesSfx;

    // Other
    protected Rigidbody2D Rb;
    protected Seeker Seeker;
    protected Path Path;
    [SerializeField] protected EnemyType Type;
    [SerializeField] protected LayerMask ProjectileHitMask;


    // Start is called before the first frame update
    protected void Start()
    {
        EnemyInitialization();

        SoundManager soundManager = GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>();

        soundManager.AddAudioSource(FireSfx, SourceType.Sfx);
        soundManager.AddAudioSource(DeathSfx, SourceType.Sfx);
        soundManager.AddAudioSource(EnginesSfx, SourceType.Sfx);

        //Type = EnemyType.Drone;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player != null)
            PlayerPos = Player.transform.position;

        if (Health <= 0 && !IsDead)
            StartCoroutine(EnemyDeath());

        else if (!IsDead)
        {
            if (Vector2.Distance(PlayerPos, transform.position) >= DespawnDistance)
                Despawn();

            EnemyLogic();
        }

        GameObjectHelper.AudioPauseCheck(EnginesSfx);
        GameObjectHelper.AudioPauseCheck(FireSfx);
        GameObjectHelper.AudioPauseCheck(DeathSfx);
        //GameObjectHelper.AudioPauseCheck(HitSfx);
    }

    public virtual void EnemyLogic()
    {
        if (!Stunned)
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
    }

    protected virtual void EnemyInitialization()
    {
        Rb = GetComponent<Rigidbody2D>();
        Player = GameObject.Find("Player");
        Seeker = GetComponent<Seeker>();

        foreach (ParticleSystem exhaust in EngineExhaust)
            exhaust.Play();

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    protected void UpdatePath()
    {
        if (Player != null)
            if (Seeker.IsDone())
                Seeker.StartPath(transform.position, Player.transform.position, OnPathComplete);
    }

    protected void OnPathComplete(Path path)
    {
        if (!path.error)
        {
            Path = path;
            CurrentWaypoint = 0;
        }
    }

    protected void SeekPlayer()
    {
        if (Path == null)
            return;

        if (CurrentWaypoint >= Path.vectorPath.Count)
        {
            return;
        }

        RotateTowards((Vector2)Path.vectorPath[CurrentWaypoint] - (Vector2)transform.position);

        Rb.AddForce(transform.up * Acceleration * Time.deltaTime, ForceMode2D.Force);

        EnableEngines(true);

        if (Vector2.Distance(transform.position, Path.vectorPath[CurrentWaypoint]) < NextWaypointDistance)
            CurrentWaypoint++;
    }

    protected virtual IEnumerator Fire()
    {
        CanFire = false;
        GameObject newLaser = Instantiate(Projectile);

        newLaser.transform.position = transform.position;
        newLaser.transform.rotation = transform.rotation;
        newLaser.tag = "EnemyProjectile";
        //newLaser.layer = SortingLayer.NameToID("Enemy Projectile");

        Laser laser = newLaser.GetComponent<Laser>();

        laser.Set(Damage, ProjectileLaunchSpeed, ProjectileHitMask);
        laser.SetColor(GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>().EnemyProjectilesColor);
        laser.Fire();
        FireSfx.Play();

        yield return new WaitForSeconds(FireDelay);
        CanFire = true;
    }

    public virtual void Hurt(int damage)
    {
        Health -= damage;

        if (!IsDead && CanBeStunned)
        {
            //Hit.Play();
            if (Mathf.RoundToInt(Random.value) == 1)
                Rb.AddTorque(-damage * 2f, ForceMode2D.Impulse);
            else
                Rb.AddTorque(-damage * 2f, ForceMode2D.Impulse);

            EnableEngines(false);
            Stunned = true;
            StartCoroutine(StunDelay());
        }
    }

    protected virtual IEnumerator EnemyDeath()
    {
        DeathSfx.Play();
        Rb.velocity = Vector3.zero;
        Rb.angularVelocity = 0;

        IsDead = true;
        EnableDisableEngineExhaust(false);
        GameObject.Find(GameObjectNames.Managers).GetComponent<EnemySpawner>().EnemiesLeft[Type]--;

        //float t = 0;

        //// Despawn an object after 10 seconds, unless the damage taken is twice the enemy health
        //while (t < 10f)
        //{
        //    if (Health >= Health * 2)
        //        break;

        //    t += Time.deltaTime;
        //    yield return null;
        //}

        //Destroy(gameObject);

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<PolygonCollider2D>().enabled = false;

        foreach (ParticleSystem exhaust in EngineExhaust)
        {
            exhaust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            exhaust.gameObject.SetActive(false);
        }

        Explosion.Play();
        yield return new WaitForSeconds(3f);
    }

    protected virtual IEnumerator StunDelay()
    {
        yield return new WaitForSeconds(StunDuration);
        Stunned = false;
        EnableDisableEngineExhaust(true);
    }

    protected void Despawn()
    {
        Destroy(gameObject);
    }

    protected void CapVelocity()
    {
        Rb.velocity = Vector2.ClampMagnitude(Rb.velocity, MaxVelocity);
    }

    /// <summary>
    /// Calculates and adds tourque to rigidbody to point towards a target gameobject
    /// </summary>
    protected void RotateTowards(GameObject target)
    {
        Vector3 playerVector = target.transform.position - transform.position;
        Vector3 cross = Vector3.Cross(transform.up, playerVector);

        Rb.AddTorque(RotationalSpeed * cross.z * Time.deltaTime, ForceMode2D.Force);
    }

    /// <summary>
    /// Calculates and adds tourque to rigidbody to point towards a target vector
    /// </summary>
    protected void RotateTowards(Vector2 targetVector)
    {
        Vector3 cross = Vector3.Cross(transform.up, targetVector);

        Rb.AddTorque(RotationalSpeed * cross.z * Time.deltaTime, ForceMode2D.Force);
    }

    protected void EnableEngines(bool enable)
    {
        if (enable && !EnginesSfx.isPlaying)
            EnginesSfx.Play();
        else if (!enable)
            EnginesSfx.Stop();

        EnableDisableEngineExhaust(enable);
    }

    protected void EnableDisableEngineExhaust(bool enable)
    {
        if (EngineExhaust.Any(e => !e.emission.enabled))
            foreach (ParticleSystem exhaust in EngineExhaust)
            {
                var emission = exhaust.emission;
                emission.enabled = enable;
            }
    }

    public int GetCurrentHealth()
    {
        return Health;
    }

    protected void OnDestroy()
    {
        SoundManager soundManager = GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>();

        soundManager.RemoveAudioSource(FireSfx, SourceType.Sfx);
        soundManager.RemoveAudioSource(DeathSfx, SourceType.Sfx);
        soundManager.RemoveAudioSource(EnginesSfx, SourceType.Sfx);
    }
}