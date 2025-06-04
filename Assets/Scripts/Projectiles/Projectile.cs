using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // Float
    [SerializeField] protected float FireVelocity;
    [SerializeField] protected float LifeTime;
    [SerializeField] protected float Age;

    // Other
    [SerializeField] protected int Damage;
    [SerializeField] protected LayerMask HitMask;
    [SerializeField] protected ParticleSystem Explosion;
    [SerializeField] protected AudioSource HitSfx;
    bool Fired;

    private void Start()
    {
        var main = Explosion.main;
        main.startColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if (Fired)
        {
            Age += Time.deltaTime;
            if (Age > LifeTime)
                ProjectileDeath(true);

            ProjectileLogic();
        }

        GameObjectHelper.AudioPauseCheck(HitSfx);
    }

    public virtual void ProjectileLogic()
    {

    }

    public virtual void TargetHit(GameObject target)
    {
        if (Fired)
            switch (target.tag)
            {
                case "Player":
                    target.GetComponent<PlayerController>().Hurt(Damage);
                    ProjectileDeath();
                    break;
                case "Enemy":
                    target.GetComponent<Drone>().Hurt(Damage);
                    ProjectileDeath();
                    break;
                case "PlayerProjectile":
                    target.GetComponent<Projectile>().ProjectileDeath();
                    ProjectileDeath(true);
                    break;
                case "EnemyProjectile":
                    target.GetComponent<Projectile>().ProjectileDeath();
                    ProjectileDeath(true);
                    break;
                default:
                    ProjectileDeath();
                    break;
            }
    }

    public virtual void Fire()
    {
        GetComponent<Rigidbody2D>().AddForce(transform.up * FireVelocity, ForceMode2D.Impulse);
        Fired = true;
    }

    /// <summary>
    /// Set Projectile Damage
    /// </summary>
    public void Set(int newDamage, float launchSpeed, LayerMask hitMask)
    {
        Damage = newDamage;
        FireVelocity = launchSpeed;
        HitMask = hitMask;
    }

    public virtual void ProjectileDeath(bool silentDestuction = false)
    {
        if (Fired)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            if (silentDestuction)
                Destroy(gameObject);
            else
            {
                GetComponent<SpriteRenderer>().enabled = false;
                HitSfx.Play();
                Explosion.Play();
            }
        }
    }

    /// <summary>
    /// Logic for when OnTriggerEnter2D is fired
    /// </summary>
    protected virtual void ColliderTriggerLogic(Collider2D collision)
    {
        if (HitMask == (HitMask | (1 << collision.gameObject.layer)))
            TargetHit(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ColliderTriggerLogic(collision);
    }

    private void OnDestroy()
    {
        GameObject.Find(GameObjectNames.Managers).GetComponent<SoundManager>().RemoveAudioSource(HitSfx,SourceType.Sfx);
    }
}
