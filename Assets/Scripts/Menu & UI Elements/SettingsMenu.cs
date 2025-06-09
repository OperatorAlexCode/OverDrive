using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SettingsMenu : Menu
{
    public UnityEvent<float> OnMasterVolumeChange;
    public UnityEvent<float> OnMusicVolumeChange;
    public UnityEvent<float> OnSfxVolumeChange;
    public UnityEvent<float> OnRotationSpeedChange;
    public UnityEvent<bool> OnBlinkingEnabledChange;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey(PlayerPrefkeys.MasterVolumeKey))
            transform.Find("Options").Find(GameObjectNames.MasterVolumeSlider).Find("Slider").GetComponent<Slider>().value = PlayerPrefs.GetFloat(PlayerPrefkeys.MasterVolumeKey);

        if (PlayerPrefs.HasKey(PlayerPrefkeys.RotationalSpeedKey))
            transform.Find("Options").Find(GameObjectNames.RotateSpeedSlider).Find("Slider").GetComponent<Slider>().value = PlayerPrefs.GetFloat(PlayerPrefkeys.RotationalSpeedKey);

        if (PlayerPrefs.HasKey(PlayerPrefkeys.BlinkingEnabledKey))
            transform.Find("Options").Find(GameObjectNames.BlinkingEnableToggle).GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt(PlayerPrefkeys.BlinkingEnabledKey));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MasterVolumeChange(float newValue)
    {
        PlayerPrefs.SetFloat(PlayerPrefkeys.MasterVolumeKey, newValue);
        PlayerPrefs.Save();

        OnMasterVolumeChange.Invoke(newValue);
    }

    public void MusicVolumeChange(float newValue)
    {
        PlayerPrefs.SetFloat(PlayerPrefkeys.MusicVolumeKey, newValue);
        PlayerPrefs.Save();

        OnMusicVolumeChange.Invoke(newValue);
    }

    public void SfxVolumeChange(float newValue)
    {
        PlayerPrefs.SetFloat(PlayerPrefkeys.SfxVolumeKey, newValue);
        PlayerPrefs.Save();

        OnSfxVolumeChange.Invoke(newValue);
    }

    public void RoationSpeedChange(float newValue)
    {
        PlayerPrefs.SetFloat(PlayerPrefkeys.RotationalSpeedKey, newValue);
        PlayerPrefs.Save();

        OnRotationSpeedChange.Invoke(newValue);
    }

    public void BlinkingEnabledChange(bool newValue)
    {
        PlayerPrefs.SetInt(PlayerPrefkeys.BlinkingEnabledKey, Convert.ToInt32(newValue));
        PlayerPrefs.Save();

        OnBlinkingEnabledChange.Invoke(newValue);
    }

    public void SaveSettings()
    {
        //PlayerPrefs.SetFloat(PlayerPrefkeys.MasterVolumeKey, Player.RotationalSpeed);
        //PlayerPrefs.SetFloat(PlayerPrefkeys.MusicVolumeKey, Player.RotationalSpeed);
        //PlayerPrefs.SetFloat(PlayerPrefkeys.SfxVolumeKey, Player.RotationalSpeed);
        //PlayerPrefs.SetFloat(PlayerPrefkeys.RotationalSpeedKey, Player.RotationalSpeed);
        PlayerPrefs.Save();
    }
}
