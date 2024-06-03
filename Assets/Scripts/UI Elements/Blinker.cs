using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;

public class Blinker : MonoBehaviour
{
    [SerializeField] float Value;
    [SerializeField] List<Phase> Phases;
    [SerializeField] Blinker SynchronizeWith;
    public Phase CurrentPhase;
    PlayerController Player;
    Image Icon;
    public bool IsOn;

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Player").GetComponent<PlayerController>();
        Icon = GetComponent<Image>();
        CurrentPhase = Phases[0];
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

        else if (Player != null)
            if (Player.Heat != Value)
                if (!CurrentPhase.InRange(Value / Player.MaxHeat))
                {
                    StopAllCoroutines();
                    CurrentPhase = Phases.First(p => p.InRange(Value / Player.MaxHeat));

                    Icon.sprite = CurrentPhase.Icon;
                    Icon.color = CurrentPhase.Clr;
                    IsOn = true;
                    
                    StartCoroutine(Flicker());
                }

        Icon.enabled = IsOn;
        Value = Player.Heat;
    }


    IEnumerator Flicker()
    {
        yield return new WaitForSeconds(CurrentPhase.FlickerDuration);

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
        [SerializeField] bool Procentage;
        [SerializeField] bool MinEnclusive;
        [SerializeField] bool MaxEnclusive;

        public bool InRange(float value)
        {
            bool aboveMin = false;
            bool bellowMax = false;

            if ((value >= RangeMin && MinEnclusive) || (value > RangeMin && !MinEnclusive))
                aboveMin = true;

            if ((value <= RangeMax && MaxEnclusive) || (value < RangeMax && !MaxEnclusive))
                bellowMax = true;

            return aboveMin && bellowMax;
        }
    }
}
