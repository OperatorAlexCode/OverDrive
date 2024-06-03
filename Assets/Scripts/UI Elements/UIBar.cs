using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    [SerializeField] Color[] GradientColors;
    [SerializeField] bool UseCustomGradient;
    [SerializeField] Gradient BarGradient;
    Slider Bar;
    Image BarFill;

    private void Start()
    {
        if (!UseCustomGradient)
        {
            var colorKey = new GradientColorKey[GradientColors.Count()];

            colorKey[0].color = GradientColors[0];
            colorKey[0].time = 0;

            for (int x = 1; x < GradientColors.Count(); x++)
            {
                colorKey[x].color = GradientColors[x];
                colorKey[x].time = (1f / GradientColors.Count()) * (float)x;
            }

            BarGradient.SetKeys(colorKey, new GradientAlphaKey[0]);
        }

        Bar = GetComponent<Slider>();
        BarFill = Bar.fillRect.GetComponent<Image>();
    }

    public void Update()
    {

    }

    public void Set(float value)
    {
        if (Bar.value != value)
        {
            Bar.value = value;
            BarFill.color = BarGradient.Evaluate((float)value / (float)Bar.maxValue);
        }
    }

    public void SetMin(float newMinValue)
    {
        if (Bar.minValue != newMinValue)
            Bar.minValue = newMinValue;
    }

    public void SetMax(float newMaxValue)
    {
        if (Bar.maxValue != newMaxValue)
            Bar.maxValue = newMaxValue;
    }
}
