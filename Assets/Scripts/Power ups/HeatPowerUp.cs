using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatPowerUp : PowerUp
{
    [SerializeField] float HeatChange;
    public override void Use(GameObject pickupEntity)
    {
        if (pickupEntity.tag == "Player")
        {
            pickupEntity.GetComponent<PlayerController>().ChangeHeat(HeatChange);
            PickupSfx.Play();
            Destroy(gameObject);
        }
    }
}
