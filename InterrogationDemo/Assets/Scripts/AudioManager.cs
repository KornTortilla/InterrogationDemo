using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private EventInstance musicInstance;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    [SerializeField]
    private EventReference voiceReference;
    private EventInstance voiceInstance;

    void Awake()
    {
        //Singleton Patter setup
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        voiceInstance = RuntimeManager.CreateInstance(voiceReference);
    }

    #region Volume Methods
    public void SetMasterVolume(float volume)
    {
        masterBus.setVolume(volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicBus.setVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxBus.setVolume(volume);
    }

    private float DecibelToLinear(float db)
    {
        float linear = Mathf.Pow(10.0f, db / 20f);
        return linear;
    }
    #endregion

    public void PlayVoice()
    {
        voiceInstance.start();
    }

    public void SetVoicePitch(float pitch)
    {
        voiceInstance.setParameterByName("VoicePitch", pitch);
    }

    public void StartNewMusic(string name)
    {
        musicInstance = RuntimeManager.CreateInstance("event:" + name);
        musicInstance.start();
    }

    public void AddMusicLayer()
    {
        
    }

    public void StopMusic()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
