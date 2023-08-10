using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LaserTurret : MonoBehaviour
{
    // Float
    [SerializeField] float ProjectilefireSpeed;
    [SerializeField] float Cooldown;
    [SerializeField] float Range;
    [SerializeField] float TurnSpeed;
    [SerializeField] float MinimumFiringAngle;

    // Bool
    bool CanFire = true;
    [SerializeField] bool InstantTurnspeed;
    bool Disabled = true;

    // GameObject
    [SerializeField]GameObject Projectile;
    GameObject CurrentTarget;
    [SerializeField] GameObject ProjectileStart;

    // String
    [SerializeField] string ProjectileTag;
    [SerializeField] string TargetTag;

    // Other
    [SerializeField] Color ProjectileColor;
    [SerializeField] int Damage;
    [SerializeField] LayerMask ProjectileHitMask;
    //[SerializeField] LayerMask TargetLayers;
    [SerializeField] AudioSource ShootSfx;

    void Start()
    {
    }

    void Update()
    {
        if (!Disabled)
        {
            // Aquires the closest target if one is in range
            if (CurrentTarget == null)
            {
                List<GameObject> inRange = GameObjectHelper.GetGameObjectsInRange(gameObject, TargetTag, Range);

                if (inRange.Count > 0)
                    CurrentTarget = inRange.OrderBy(o => Vector2.Distance(transform.position, o.transform.position)).First();

            }

            if (CurrentTarget != null)
            {
                if (Vector2.Distance(transform.position, CurrentTarget.transform.position) > Range)
                    CurrentTarget = null;

                else
                {
                    Vector2 targetVector = CurrentTarget.transform.position - transform.position;

                    if (InstantTurnspeed)
                        transform.up = targetVector;

                    else
                    {
                        float rotationDifference = Vector2.SignedAngle(transform.up, (CurrentTarget.transform.position - transform.position).normalized);
                        transform.up = VectorHelper.AngleVector(transform.up, Mathf.Clamp(rotationDifference, -TurnSpeed, TurnSpeed) * Time.deltaTime);
                    }

                    if (CanFire && Vector2.Angle(transform.up, (CurrentTarget.transform.position - transform.position).normalized) <= MinimumFiringAngle)
                        StartCoroutine(Fire());
                }
            }
        }
    }

    protected virtual IEnumerator Fire()
    {
        CanFire = false;
        GameObject newLaser = Instantiate(Projectile, ProjectileStart.transform);

        newLaser.transform.position = ProjectileStart.transform.position;
        newLaser.transform.rotation = ProjectileStart.transform.rotation;
        newLaser.tag = ProjectileTag;
        newLaser.transform.SetParent(null);

        Laser laser = newLaser.GetComponent<Laser>();

        laser.Set(Damage, ProjectilefireSpeed, ProjectileHitMask);
        laser.SetColor(ProjectileColor);
        laser.Fire();
        ShootSfx.Play();

        yield return new WaitForSeconds(Cooldown);
        CanFire = true;
    }

    public void Set(int damage, float fireSpeed, float cooldown, float range, float turnSpeed, float minimumFiringAngle, bool instantTurnspeed = false)
    {
        Damage = damage;
        ProjectilefireSpeed = fireSpeed;
        Cooldown = cooldown;
        Range = range;
        TurnSpeed = turnSpeed;
        MinimumFiringAngle = minimumFiringAngle;
        InstantTurnspeed = instantTurnspeed;
    }

    public void SetTags(string projectileTag, string targetTag)
    {
        ProjectileTag = projectileTag;
        TargetTag = targetTag;
    }

    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    public void SetProjectileColor(Color newColor)
    {
        ProjectileColor = newColor;
    }

    public void SetLayerMasks(LayerMask projetileHitMask, LayerMask targetLayers)
    {
        ProjectileHitMask = projetileHitMask;
        //TargetLayers = targetLayers;
    }

    public void Disable(bool toDisable)
    {
        Disabled = toDisable;
    }
}
