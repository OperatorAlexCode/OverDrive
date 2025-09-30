using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : Drone
{
    // GameObject
    [SerializeField] protected GameObject TorpedoBay;
    protected GameObject LoadedTorpedo;
    // Other
    [SerializeField] protected Warhead Warhead;
    [SerializeField] protected float StopSpeed;

    protected override void EnemyInitialization()
    {
        base.EnemyInitialization();
        //Type = EnemyType.Destroyer;
        Warhead = new Warhead();
        Warhead.Set(Damage, "Player");
        LoadTorpedo();
        CanFire = true;
    }

    public override void EnemyLogic()
    {
        if (LoadedTorpedo != null)
        {
            LoadedTorpedo.transform.position = TorpedoBay.transform.position;
            LoadedTorpedo.transform.rotation = TorpedoBay.transform.rotation;
        }

        if (!Stunned)
        {
            SeekPlayer();

            if (Vector3.Distance(transform.position, PlayerPos) <= Range && !Player.GetComponent<PlayerController>().IsDead)
            {
                if (Rb.velocity.magnitude > 0)
                {
                    if (Rb.velocity.magnitude <= StopSpeed)
                    {
                        Rb.velocity = Vector2.zero;
                    }
                    else
                    {
                        Rb.AddForce(-(Rb.velocity.normalized * Acceleration * Time.deltaTime), ForceMode2D.Force);
                    }

                    //EnableDisableEngineExhaust(false);
                    EngineExhaust.Stop();
                }

                //Debug.Log(Vector2.Angle(transform.up, PlayerPos - transform.position));

                if (CanFire && Rb.velocity.magnitude == 0 && Vector2.Angle(transform.up, PlayerPos - transform.position) <= MinimumFiringAngle)
                    StartCoroutine(Fire());
            }

            //else
                //EnableDisableEngineExhaust(false);
                //EngineExhaust.Stop();

            CapVelocity();
        }
    }

    /// <summary>
    /// Fires torpedo in torpedo bay then loads another one after a delay
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Fire()
    {
        CanFire = false;

        if (LoadedTorpedo != null)
        {
            Transform torpedo = LoadedTorpedo.transform;
            torpedo.GetComponent<Torpedo>().Fire();
            torpedo.SetParent(null);
            LoadedTorpedo = null;
        }

        yield return new WaitForSeconds(FireDelay);
        LoadTorpedo();
        CanFire = true;
    }

    protected void LoadTorpedo()
    {
        GameObject newTorpedo = Instantiate(Projectile, TorpedoBay.transform);

        newTorpedo.transform.position = TorpedoBay.transform.position;
        newTorpedo.transform.rotation = TorpedoBay.transform.rotation;
        //newTorpedo.transform.SetParent(TorpedoBay.transform);
        newTorpedo.tag = "EnemyProjectile";
        newTorpedo.layer = LayerMask.NameToLayer("Interactible Projectile");

        Torpedo torpedo = newTorpedo.GetComponent<Torpedo>();

        torpedo.Set(Damage, ProjectileLaunchSpeed, ProjectileHitMask);
        torpedo.SetColor(GameObject.Find(GameObjectNames.Managers).GetComponent<GameManager>().EnemyProjectilesColor);
        torpedo.SetStats(8f, ProjectileLaunchSpeed);

        if (Warhead.GetType().ToString() == "Missile")
            torpedo.LoadWarhead<Missile>(Warhead);
        else if (Warhead.GetType().ToString() == "Flak")
            torpedo.LoadWarhead<Flak>(Warhead);
        else
            torpedo.LoadWarhead<Warhead>(Warhead);

        LoadedTorpedo = newTorpedo;
    }

    protected override IEnumerator EnemyDeath()
    {
        if (LoadedTorpedo != null)
        {
            Destroy(LoadedTorpedo.gameObject);
        }

        StartCoroutine(base.EnemyDeath());
        yield return null;
    }
}
