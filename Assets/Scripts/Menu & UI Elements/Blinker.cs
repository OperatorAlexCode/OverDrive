using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Blinker : MonoBehaviour
{
    [SerializeField] float Value;
    [SerializeField] List<Phase> Phases;
    [SerializeField] Blinker SynchronizeWith;
    public Phase CurrentPhase;
    //[SerializeField] PlayerController Player;
    Image Icon;
    public bool IsOn;
    public bool Blinking;

    // Start is called before the first frame update
    void Start()
    {
        //Player = GameObject.Find("Player").GetComponent<PlayerController>();
        Icon = GetComponent<Image>();
        CurrentPhase = Phases[0];
        //Value = Player.Heat;

        if (PlayerPrefs.HasKey(PlayerPrefkeys.BlinkingEnabledKey))
            Blinking = Convert.ToBoolean(PlayerPrefs.GetInt(PlayerPrefkeys.BlinkingEnabledKey));
    }

    // Update is called once per frame
    void Update()
    {
        if (SynchronizeWith != null)
        {
            IsOn = SynchronizeWith.IsOn;
            Icon.sprite = SynchronizeWith.CurrentPhase.Icon;
            Icon.color = SynchronizeWith.CurrentPhase.Clr;
        }

        /*else if (Player != null)
            if (Player.Heat != Value)
            {
                if (!CurrentPhase.InRange(Value / Player.MaxHeat))
                {
                    StopAllCoroutines();
                    CurrentPhase = Phases.First(p => p.InRange(Value / Player.MaxHeat));

                    Icon.sprite = CurrentPhase.Icon;
                    Icon.color = CurrentPhase.Clr;
                    IsOn = true;

                    if (!SolidColor)
                        StartCoroutine(Flicker());
                }
            }

        Value = Player.Heat;
        Icon.enabled = IsOn;*/

        else if (!CurrentPhase.InRange(Value))
        {
            StopAllCoroutines();
            CurrentPhase = Phases.First(p => p.InRange(Value));

            Icon.sprite = CurrentPhase.Icon;
            Icon.color = CurrentPhase.Clr;
            IsOn = true;

            if (Blinking)
                StartCoroutine(Flicker());
        }

        Icon.enabled = IsOn;
    }

    public void UpdateValue(float newValue)
    {
        Value = newValue;
    }

    public void UpdateValue(bool newValue)
    {
        Blinking = newValue;
    }

    IEnumerator Flicker()
    {
        yield return new WaitForSeconds(CurrentPhase.FlickerDuration);

        if (Blinking)
            IsOn = !IsOn;

        StartCoroutine(Flicker());
    }

    [Serializable]
    public class Phase
    {
        [SerializeField] public Sprite Icon;
        [SerializeField] public Color Clr;
        [SerializeField] public float FlickerDuration;
        [SerializeField] float RangeMin;
        [SerializeField] float RangeMax;
        //[SerializeField] bool Percentage;
        [SerializeField] bool MinEnclusive;
        [SerializeField] bool MaxEnclusive;

        public bool InRange(float value)
        {
            bool aboveMin = false;
            bool bellowMax = false;

            //if ((value >= RangeMin && MinEnclusive) || (value > RangeMin && !MinEnclusive))
            //    aboveMin = true;

            //if ((value <= RangeMax && MaxEnclusive) || (value < RangeMax && !MaxEnclusive))
            //    bellowMax = true;
            
            aboveMin = (value >= RangeMin && MinEnclusive) || (value > RangeMin && !MinEnclusive);
            bellowMax = (value <= RangeMax && MaxEnclusive) || (value < RangeMax && !MaxEnclusive);

            return aboveMin && bellowMax;
        }
    }
}
