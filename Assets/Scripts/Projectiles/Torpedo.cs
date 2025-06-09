using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Torpedo : Projectile
{
    Warhead Payload;
    protected bool ToBeDestroyed;
    [SerializeField] ParticleSystem Trail;
    [SerializeField] AudioSource EngineSfx;

    private void Start()
    {
    }

    public override void ProjectileLogic()
    {
        Payload.WarheadLogic();

        if (ToBeDestroyed)
        {
            ToBeDestroyed = false;
            ProjectileDeath();
        }

        GameObjectHelper.AudioPauseCheck(EngineSfx);
    }

    public override void TargetHit(GameObject target)
    {
        //Damage = Payload.Damage;
        //base.TargetHit(target);
        Payload.OnHit(target);
        ToBeDestroyed = true;
    }

    public override void ProjectileDeath(bool silentDestuction = false)
    {
        if (silentDestuction)
        {
            SoundManager soundManager = GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>();
            soundManager.RemoveAudioSource(EngineSfx, SourceType.Sfx);
            soundManager.RemoveAudioSource(HitSfx, SourceType.Sfx);
            Destroy(gameObject);
        }
        else
        {
            transform.Find("Engine").GetComponent<ParticleSystem>().Stop(true, ParticleSystemStopBehavior.StopEmitting);
            transform.Find("Engine").GetComponent<ParticleSystem>().gameObject.SetActive(false);
            transform.Find("Coloration").gameObject.SetActive(false);
            transform.Find("Body").gameObject.SetActive(false);
            transform.GetComponent<BoxCollider2D>().enabled = false;
            EngineSfx.Stop();
            HitSfx.Play();
            Explosion.Play();
        }
    }

    public void SetStats(float lifeTime, float launchSpeed)
    {
        LifeTime = lifeTime;
        FireVelocity = launchSpeed;
    }

    public void Set(float launchSpeed)
    {
        FireVelocity = launchSpeed;
    }
    public void SetToDestroy()
    {
        ToBeDestroyed = true;
    }

    public void SetColor(Color color)
    {
        transform.Find("Coloration").GetComponent<SpriteRenderer>().color = color;
    }

    public void LoadWarhead<subclass>(Warhead warhead) where subclass : Warhead, new()
    {
        subclass newWarehad = new subclass();
        newWarehad.Set((subclass)warhead);
        newWarehad.Setparent(gameObject);
        Payload = newWarehad;
    }

    public override void Fire()
    {
        if (Payload.GetType() == typeof(Missile))
        {
            float v = FireVelocity / GetComponent<Rigidbody2D>().mass;
            Missile warhead = Payload as Missile;
            warhead.MaxVelocity = v;
        }

        Trail.Play();
        EngineSfx.Play();

        base.Fire();
    }

    private void OnDestroy()
    {
        SoundManager soundManager = GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>();
        soundManager.RemoveAudioSource(EngineSfx, SourceType.Sfx);
        soundManager.RemoveAudioSource(HitSfx, SourceType.Sfx);
    }
}

public class Warhead
{
    protected Rigidbody2D TorpedoRb;
    protected Torpedo Torpedo;
    public int Damage = 10;
    protected string TargetTag = "Enemy";

    public virtual void WarheadLogic()
    {

    }

    public void Setparent(GameObject parent)
    {
        Torpedo = parent.GetComponent<Torpedo>();
        TorpedoRb = parent.GetComponent<Rigidbody2D>();
    }

    public virtual void Set(Warhead warheadStats)
    {
        Damage = warheadStats.Damage;
        TargetTag = warheadStats.TargetTag;
    }

    public void Set(int damage, string targetTag)
    {
        Damage = damage;
        TargetTag = targetTag;
    }

    public virtual void OnHit(GameObject target)
    {
        switch (target.tag)
        {
            case "Player":
                target.GetComponent<PlayerController>().Hurt(Damage);
                break;
            case "Enemy":
                target.GetComponent<Drone>().Hurt(Damage);
                break;
            case "PlayerProjectile":
                target.GetComponent<Projectile>().ProjectileDeath();
                break;
            case "EnemyProjectile":
                target.GetComponent<Projectile>().ProjectileDeath();
                break;
            default:
                break;
        }
    }
}

public class Missile : Warhead
{
    // Float2
    public float TurnSpeed;
    public float Acceleration;
    public float MaxVelocity;
    public float LockOnDistance;
    /// <summary>
    /// The cone of vision, in degrees,that the missile can see targets in.
    /// </summary>
    public float SeekingCone;

    // Other
    LayerMask TargetLayer = 1 << 6;
    GameObject LatestTarget;
    int TrackingLasers = 9;

    public override void WarheadLogic()
    {
        GameObject target = AquireTarget();

        if (LatestTarget != null)
        {
            if (GameObjectHelper.IsObjectWithinDistance(Torpedo.gameObject, LatestTarget, LockOnDistance))
                target = LatestTarget;
        }

        if (target != null)
        {
            //PointTowards(target);
            //TorpedoRb.AddForce(Torpedo.gameObject.transform.up * Acceleration * Time.deltaTime, ForceMode2D.Force);

            //if (GameObjectHelper.IsObjectWithinDistance(Torpedo.gameObject, target, LockOnDistance))
            //{
            //    //Vector3 targetVector = target.transform.position - Torpedo.gameObject.transform.position;
            //    //Debug.DrawRay(Torpedo.gameObject.transform.position, targetVector, Color.yellow);
            //    //TorpedoRb.AddForce(targetVector.normalized * Acceleration * 2f * Time.deltaTime, ForceMode2D.Force);
            //    //PointTowards(targetVector);

            //    if (/*Mathf.Acos(Torpedo.gameObject.transform.up.magnitude / TorpedoRb.velocity.magnitude) < 2f && */TorpedoRb.velocity.magnitude > MaxVelocity * 0.6f)
            //        TorpedoRb.AddForce(-(TorpedoRb.velocity.normalized * Acceleration * 2f * Time.deltaTime), ForceMode2D.Force);
            //}
            //else
            //{
            //    PointTowards(target);
            //    TorpedoRb.AddForce(Torpedo.gameObject.transform.up * Acceleration * Time.deltaTime, ForceMode2D.Force);
            //}

            //PointTowards(target);
            //TorpedoRb.AddForce(Torpedo.gameObject.transform.up * Acceleration * Time.deltaTime, ForceMode2D.Impulse);
            //Debug.Log(Vector2.Distance(Torpedo.transform.position, target.transform.position));

            if (GameObjectHelper.IsObjectWithinDistance(Torpedo.gameObject, target, LockOnDistance))
            {
                RaycastHit2D hit = Physics2D.Raycast(Torpedo.gameObject.transform.position, TorpedoRb.velocity, TorpedoRb.velocity.magnitude, TargetLayer);

                if (hit.collider != null)
                    if (hit.collider.gameObject.Equals(target))
                        PointTowards(TorpedoRb.velocity);
            }

            else
                PointTowards(target);

            TorpedoRb.AddForce(Torpedo.gameObject.transform.up * Acceleration * Time.deltaTime, ForceMode2D.Impulse);

            //Vector3 targetVector = target.transform.position - Torpedo.gameObject.transform.position;
            //TorpedoRb.AddForce(targetVector.normalized * Acceleration * Time.deltaTime,ForceMode2D.Force);
            //Debug.DrawRay(Torpedo.gameObject.transform.position, targetVector, Color.yellow);

            CapVelocity();
            LatestTarget = target;
        }
        else
            TorpedoRb.angularVelocity = 0;

        Debug.DrawRay(Torpedo.gameObject.transform.position, TorpedoRb.velocity, Color.blue);
        Debug.DrawRay(Torpedo.gameObject.transform.position, Torpedo.gameObject.transform.up * 20f, Color.red);
    }

    public override void Set(Warhead warheadStats)
    {
        Missile stats = (Missile)warheadStats;
        base.Set(stats.Damage, stats.TargetTag);
        TurnSpeed = stats.TurnSpeed;
        Acceleration = stats.Acceleration;
        LockOnDistance = stats.LockOnDistance;
        SeekingCone = stats.SeekingCone;
    }

    public void Set(int damage, string targetTag, float turnSpeed, float speed, float lockOnDistance, float seekingCone)
    {
        base.Set(damage, targetTag);
        TurnSpeed = turnSpeed;
        Acceleration = speed;
        LockOnDistance = lockOnDistance;
        SeekingCone = seekingCone;
    }

    public void Set(LayerMask target)
    {
        TargetLayer = target;
    }

    GameObject AquireTarget()
    {
        List<Vector2> raycastDirections = VectorHelper.CreateVectorSpread(Torpedo.transform.up, SeekingCone, TrackingLasers);

        List<RaycastHit2D> raycastHits = new();

        foreach (Vector2 direction in raycastDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(Torpedo.gameObject.transform.position, direction, float.PositiveInfinity, TargetLayer);

            //Debug.DrawRay(Torpedo.gameObject.transform.position, direction * 100f, Color.red);

            if (hit.collider != null)
                raycastHits.Add(hit);
        }

        // if any of the raycasts hit a target then return the closest target
        if (raycastHits.Count > 0)
            return GetClosestTarget(raycastHits);

        return null;
    }

    GameObject GetClosestTarget(List<RaycastHit2D> targets)
    {
        return targets.OrderBy(h => h.distance).ToList()[0].collider.gameObject;
    }

    void PointTowards(GameObject target)
    {
        Vector3 targetVector = Torpedo.gameObject.transform.position - target.transform.position;
        Vector3 cross = Vector3.Cross(Torpedo.gameObject.transform.up, targetVector);

        //TorpedoRb.angularVelocity = -cross.z * TurnSpeed * Time.deltaTime;

        TorpedoRb.AddTorque(-cross.z * TurnSpeed * Time.deltaTime, ForceMode2D.Force);
    }

    void PointTowards(Vector3 direction)
    {
        Vector3 cross = Vector3.Cross(Torpedo.gameObject.transform.up, direction);

        //TorpedoRb.angularVelocity = -cross.z * TurnSpeed * Time.deltaTime;
        TorpedoRb.AddTorque(cross.z * TurnSpeed * Time.deltaTime, ForceMode2D.Force);
    }

    void CapVelocity()
    {
        TorpedoRb.velocity = Vector2.ClampMagnitude(TorpedoRb.velocity, MaxVelocity);
    }
}

public class Flak : Warhead
{
    public float DamageRange;
    public float DetonationDistance;
    public float Fuse = 0.05f;
    bool Detonated;
    bool FuseActivated;

    public override void WarheadLogic()
    {
        if (!Detonated)
            if (GameObject.FindGameObjectsWithTag(TargetTag).Length > 0 && Torpedo != null)
            {
                if (FuseActivated)
                {
                    Fuse -= Time.deltaTime;
                    
                    if (Fuse <= 0)
                        Detonate();
                }

                if (GameObjectHelper.AnyObjectsWithinDistance(Torpedo.gameObject, DetonationDistance, TargetTag))
                    FuseActivated = true;
            }
    }

    public override void OnHit(GameObject target)
    {
        Detonate();
    }

    public override void Set(Warhead warheadStats)
    {
        Flak stats = (Flak)warheadStats;
        Set(stats.Damage, stats.TargetTag);
        DetonationDistance = stats.DetonationDistance;
        //DamageRange = stats.DamageRange;
        DamageRange = stats.DetonationDistance * 2;
    }

    public void Set(int damage, string targetTag, float detonationDistance, float damageRange)
    {
        Set(damage, targetTag);
        DetonationDistance = detonationDistance;
        //DamageRange = damageRange;
        DamageRange = detonationDistance * 2;
    }

    void Detonate()
    {
        List<GameObject> inRange = GameObjectHelper.GetGameObjectsInRange(Torpedo.gameObject, TargetTag, DamageRange);

        foreach (GameObject enemy in inRange)
            enemy.GetComponent<Drone>().Hurt(Damage);

        Detonated = true;
        Torpedo.SetToDestroy();
    }
}
