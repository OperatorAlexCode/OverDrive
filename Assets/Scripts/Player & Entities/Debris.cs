using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Debris : MonoBehaviour
{
    [SerializeField] int CollisionDamage = 1;
    [SerializeField] float MinCollisionSpeed = 2f;
    [SerializeField] LayerMask CollisionLayers;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if ((CollisionLayers & (1 << collision.gameObject.layer)) != 0)
            if (collision.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude >= MinCollisionSpeed ||
                gameObject.GetComponent<Rigidbody2D>().velocity.magnitude >= MinCollisionSpeed)
            {
                switch (collision.gameObject.tag)
                {
                    case "Player":
                        collision.gameObject.GetComponent<PlayerController>().Hurt(CollisionDamage);
                        break;
                    case "Enemy":
                        collision.gameObject.GetComponent<Drone>().Hurt(CollisionDamage);
                        break;
                }

                Destroy(gameObject);
            }
    }
}
