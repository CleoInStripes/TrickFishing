using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SoundEffectsManager : SingletonMonoBehaviour<SoundEffectsManager>
{
    public AudioSource audioSource;

    public List<AudioClip> soundEffects;

    private readonly Dictionary<string, AudioClip> soundEffectsDict = new Dictionary<string, AudioClip>();
    private float originalVolume = 1f;

    new void Awake()
    {
        base.Awake();

        soundEffectsDict.Clear();
        foreach (var clip in soundEffects)
        {
            soundEffectsDict[clip.name] = clip;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        originalVolume = audioSource.volume;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Play(string key, float volumeMultiplier = 1f)
    {
        if (soundEffectsDict.ContainsKey(key))
        {
            Play(soundEffectsDict[key], volumeMultiplier);
        }
    }

    public void PlayAt(string key, Vector3 position, float volumeMultiplier = 1f)
    {
        if (soundEffectsDict.ContainsKey(key))
        {
            PlayAt(soundEffectsDict[key], position);
        }
    }


    public void Play(AudioClip audioClip, float volumeMultiplier = 1f)
    {
        if (audioClip == null)
        {
            return;
        }

        audioSource.spatialBlend = 0f;
        audioSource.PlayOneShot(audioClip, volumeMultiplier);
    }

    public async void PlayAt(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (audioClip == null)
        {
            return;
        }

        var newAudioSourceObj = new GameObject("Temp_3D_AudioSource");
        var newAudioSource = newAudioSourceObj.AddComponent<AudioSource>();

        newAudioSource.spatialBlend = 1f;
        newAudioSource.transform.position = position;
        newAudioSource.volume = originalVolume;
        newAudioSource.PlayOneShot(audioClip, volumeMultiplier);
        
        await Task.Delay((int)(audioClip.length * 1000));
        Destroy(newAudioSourceObj);
    }

    public AudioClip GetClip(string key)
    {
        if (soundEffectsDict.ContainsKey(key))
        {
            return soundEffectsDict[key];
        }

        return null;
    }
}