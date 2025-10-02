using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    Vector2 StartPos;
    [SerializeField] GameObject Camera;
    [SerializeField] float ParallaxEffect;

    // Start is called before the first frame update
    void Start()
    {
        StartPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        float distanceX = Camera.transform.position.x * Mathf.Clamp(ParallaxEffect, 0, 1);
        float distanceY = Camera.transform.position.y * Mathf.Clamp(ParallaxEffect, 0, 1);

        transform.position = new(StartPos.x + distanceX, StartPos.y + distanceY, transform.position.z);
    }
}
