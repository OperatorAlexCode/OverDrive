using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField]
    Image Background;
    [SerializeField]
    Image Icon;
    [SerializeField]
    Image Backing;
    [SerializeField]
    Image Overlay;

    /// <summary>
    /// Sets how much the timer is filled in so 1f would fill it completely, 0.5f would fill it half and 0f would not fill anything
    /// </summary>
    public void Set(float t)
    {
        //Background.fillAmount = t;
        //Icon.fillAmount = t;

        Backing.fillAmount = 1f - t;
    }

    public void SetIcon(Color newColor)
    {
        Icon.color = newColor;
    }

    public void SetBackground(Color newColor)
    {
        Background.color = newColor;
    }

    public void SetBacking(Color newColor)
    {
        Backing.color = newColor;
    }

    public void SetOverLay(Color newColor)
    {
        Overlay.color = newColor;
    }

    public void SetOverLay(bool isActive)
    {
        Overlay.gameObject.SetActive(isActive);
    }
}
