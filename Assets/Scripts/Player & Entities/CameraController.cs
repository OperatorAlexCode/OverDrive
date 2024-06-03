using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Int
    [SerializeField] float MaxProjectionSize;
    float StartProjectionSize;

    // Other
    [SerializeField] GameObject FollowObject;
    [SerializeField] bool AccelerationZoomOut;
    Camera GameCamera;
    Rect Bounds;
    [SerializeField] Transform LookAhead;

    // Start is called before the first frame update
    void Start()
    {
        GameCamera = GetComponent<Camera>();
        StartProjectionSize = GameCamera.orthographicSize;
        Bounds = GameObject.Find("Managers").GetComponent<GameManager>().PlayArea;
    }

    // Update is called once per frame
    void Update()
    {
        if (FollowObject != null)
        {
            transform.position = new Vector3(FollowObject.transform.position.x, FollowObject.transform.position.y, transform.position.z);

            if (FollowObject.name == "Player")
            {
                // Zooms the camera out and in depending on the players velocity
                if (AccelerationZoomOut)
                {
                    PlayerController player = FollowObject.GetComponent<PlayerController>();

                    float t = Mathf.InverseLerp(0, player.GetMaxVelocity(), player.GetVelocity());
                    GameCamera.orthographicSize = Mathf.SmoothStep(StartProjectionSize, MaxProjectionSize, t);
                }

                else if (GameCamera.orthographicSize != StartProjectionSize)
                    GameCamera.orthographicSize = StartProjectionSize + (MaxProjectionSize - StartProjectionSize) / 2;
            }

            // Keeps the camera inside the play area
            float halfHeight = GameCamera.orthographicSize;
            float halfWidth = halfHeight * Screen.width / Screen.height;

            float xBounded = Mathf.Clamp(transform.position.x, Bounds.xMin + halfWidth, Bounds.xMax - halfWidth);
            float yBounded = Mathf.Clamp(transform.position.y, Bounds.yMin + halfHeight, Bounds.yMax - halfHeight);

            transform.position = new Vector3(xBounded, yBounded, transform.position.z);
        }
    }

    /// <summary>
    /// Get
    /// </summary>
    /// <returns>Rectangle coresponding to </returns>
    public Rect GetCameraBounds()
    {
        float halfHeight = GameCamera.orthographicSize;
        float halfWidth = halfHeight * Screen.width / Screen.height;

        return new Rect(transform.position.x - halfWidth, transform.position.y - halfHeight, halfWidth * 2f, halfHeight * 2f);
    }
}
