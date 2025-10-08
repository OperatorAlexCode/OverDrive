using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPowerUp : PowerUp
{
    [SerializeField] int HealthGain;
    public override void Use(GameObject pickupEntity)
    {
        if (pickupEntity.tag == "Player")
        {
            pickupEntity.GetComponent<PlayerController>().ChangeHealth(HealthGain);
            PickupSfx.Play();
            Destroy(gameObject);
        }
    }
}
