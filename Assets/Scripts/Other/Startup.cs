using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Startup : MonoBehaviour
{
    public UnityEvent OnStartup;

    // Start is called before the first frame update
    void Start()
    {
        OnStartup.Invoke();
    }
}
