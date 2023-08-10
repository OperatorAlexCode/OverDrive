using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Drone
{
    [SerializeField]
    GameObject DetonatorTrigger;
    bool LockedOn;

    //protected override void EnemyInitialization()
    //{
    //    base.EnemyInitialization();
    //    //Type = EnemyType.Charger;
    //    //DetonatorTrigger.AddComponent<ChargerDetonator>();
    //}

    public override void EnemyLogic()
    {
        SeekPlayer();

        if (Vector3.Distance(transform.position, PlayerPos) <= Range && !Player.GetComponent<PlayerController>().IsDead)
        {
            if (!LockedOn)
            {
                MaxVelocity *= 2f;
                LockedOn = true;
            }
        }
        else if (LockedOn)
        {
            MaxVelocity /= 2f;
            LockedOn = false;
        }

        CapVelocity();
    }

    public override void Hurt(int damage)
    {
        Health -= damage;

        if (!IsDead)
        {
            if (Mathf.RoundToInt(Random.value) == 1)
                Rb.AddTorque(-damage * 2f, ForceMode2D.Impulse);
            else
                Rb.AddTorque(-damage * 2f, ForceMode2D.Impulse);
        }
    }

    public void Detonate(bool doDamage)
    {
        if (doDamage)
        Player.GetComponent<PlayerController>().Hurt(Damage);

        StartCoroutine(EnemyDeath());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
            Detonate(true);
        else
            Detonate(false);
    }
}
