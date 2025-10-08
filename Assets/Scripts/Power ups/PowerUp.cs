using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField] float LifeTime;
    public AudioSource PickupSfx;
    // Start is called before the first frame update
    void Start()
    {
        if (LifeTime > 0)
            StartCoroutine(SelfDestroy());

        PickupSfx = GetComponent<AudioSource>();
    }

    public virtual void Use(GameObject pickupEntity)
    {
        PickupSfx.PlayOneShot(PickupSfx.clip);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Use(collision.gameObject);
    }

    IEnumerator SelfDestroy()
    {
        yield return new WaitForSeconds(LifeTime);
        Destroy(gameObject);
    }
}
