using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] List<SourceList> AudioSourcesInScene;

    /*[Range(0, 1)] */public float MasterVolume { get; set; } = 0.75f;
    /*[Range(0, 1)] */public float MusicVolume { get; set; } = 0.75f;
    /*[Range(0, 1)] */public float SfxVolume { get; set; } = 0.75f;

    // Start is called before the first frame update
    void Start()
    {
        // Sets the master, music and sfx volume variables from playerprefs if they exist
        if (PlayerPrefs.HasKey(PlayerPrefkeys.MasterVolumeKey))
            MasterVolume = PlayerPrefs.GetFloat(PlayerPrefkeys.MasterVolumeKey);

        if (PlayerPrefs.HasKey(PlayerPrefkeys.MusicVolumeKey))
            MusicVolume = PlayerPrefs.GetFloat(PlayerPrefkeys.MusicVolumeKey);

        if (PlayerPrefs.HasKey(PlayerPrefkeys.SfxVolumeKey))
            SfxVolume = PlayerPrefs.GetFloat(PlayerPrefkeys.SfxVolumeKey);

        //UpdateVolume(SourceType.Music);
        UpdateVolume(SourceType.Sfx);
    }

    // Update is called once per frame
    void Update()
    {
        //if (AudioSourcesInScene.Any(t => t.Any(s => s.volume == MasterVolume*)))
        //{

        //}
    }

    public void UpdateVolumeAll()
    {
        foreach (int type in Enum.GetValues(typeof(SourceType)))
            UpdateVolume(type);
    }

    public void UpdateVolume(SourceType type)
    {
        foreach (AudioSource audioSource in AudioSourcesInScene[(int)type].Sources)
            switch (type)
            {
                case SourceType.Music:
                    audioSource.volume = MasterVolume * MusicVolume;
                    break;
                case SourceType.Sfx:
                    audioSource.volume = MasterVolume * SfxVolume;
                    break;
            }

    }

    public void UpdateVolume(int type)
    {
        UpdateVolume((SourceType)type);
    }

    public void AddAudioSource(AudioSource audioSource, SourceType type)
    {
        AudioSourcesInScene[(int)type].Sources.Add(audioSource);

        switch (type)
        {
            case SourceType.Music:
                audioSource.volume = MasterVolume * MusicVolume;
                break;
            case SourceType.Sfx:
                audioSource.volume = MasterVolume * SfxVolume;
                break;
        }
    }

    public void RemoveAudioSource(AudioSource audioSource, SourceType type)
    {
        AudioSourcesInScene[(int)type].Sources.Remove(audioSource);
    }
}

[System.Serializable]
public class SourceList
{
    public List<AudioSource> Sources;
}

public enum SourceType
{
    Music,
    Sfx
}
