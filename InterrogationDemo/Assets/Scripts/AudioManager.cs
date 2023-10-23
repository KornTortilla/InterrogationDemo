using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer mixer;

    private FMOD.Studio.EventInstance musicInstance;

    [SerializeField]
    private FMODUnity.EventReference voiceReference;
    private FMOD.Studio.EventInstance voiceInstance;

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

        voiceInstance = RuntimeManager.CreateInstance(voiceReference);
    }

    #region Volume Methods
    public void SetMasterVolume(float volume)
    {
        mixer.SetFloat("Master Volume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("Music Volume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFX Volume", volume);
    }
    #endregion

    public void PlayVoice()
    {
        voiceInstance.start();
    }

    public void ChangeVoicePitch(float pitch)
    {
        voiceInstance.setParameterByName("VoicePitch", pitch);
    }

    public void PlayNewTrack(string name)
    {
        musicInstance = RuntimeManager.CreateInstance("event:" + name);
        musicInstance.start();
    }

    public void AddMusicLayer()
    {
        //musicInstance.setParameterByID();
    }

    public void StopMusic()
    {
        Debug.Log("Stopping music");

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
