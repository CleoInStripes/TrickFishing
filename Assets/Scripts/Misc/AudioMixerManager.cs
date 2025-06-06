using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerManager : SingletonMonoBehaviour<AudioMixerManager>
{
    public AudioMixer masterMixer;

    public void SetMaster(float soundLevel)
    {
        Debug.Log(soundLevel);
        masterMixer.SetFloat("MasterVol", soundLevel);
    }

    public void SetMusic(float soundLevel)
    {
        masterMixer.SetFloat("MusicVol", soundLevel);
    }

    public void SetSound(float soundLevel)
    {
        masterMixer.SetFloat("SFXVol", soundLevel);
    }
}