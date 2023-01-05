using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource effectSource;
    [SerializeField] private GameObject musicObject;

    private List<AudioSource> musicSources;
    private int musicLayerIndex;

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

        musicSources = new List<AudioSource>();
    }

    public void PlayEffect(AudioClip clip)
    {
        effectSource.PlayOneShot(clip);
    }

    public void PlayNewTrack(string name)
    {
        StopAllCoroutines();

        //Empties current musicSources to replace with new onces
        foreach(AudioSource musicSource in musicSources)
        {
            Destroy(musicSource);
        }
        musicSources.Clear();

        //Gets music scriptable object of name, returns if cannot find
        MusicSO musicSO = Resources.Load("Audio/" + name) as MusicSO;
        if(musicSO == null)
        {
            return;
        }

        //For each clip in SO, add an audio source and start it
        foreach(AudioClip track in musicSO.MusicList)
        {
            AudioSource musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.clip = track;
            musicSource.loop = true;
            musicSource.volume = 0f;

            musicSource.Play();
            musicSources.Add(musicSource);
        }

        //Resets index of music layer
        musicLayerIndex = 0;

        //Adds first music layer
        AddMusicLayer();
    }

    public void AddMusicLayer()
    {
        //If there are still music sources at the index, start transitioning to max volume
        if(musicLayerIndex < musicSources.Count)
        {
            StartCoroutine(TransitionMusic(musicSources[musicLayerIndex], 1f));
        }

        //Increase index
        musicLayerIndex++;
    }

    public void StopMusic()
    {
        //For each music source, transitioning to no volume
        foreach(AudioSource musicSource in musicSources)
        {
            StartCoroutine(TransitionMusic(musicSource, 0f));
        }
    }

    private IEnumerator TransitionMusic(AudioSource source, float value)
    {
        //Temporary implementation of transition
        if(source != null)
        {
            if (source.volume < value)
            {
                while (source.volume < value)
                {
                    source.volume += Time.deltaTime / 2;

                    yield return null;
                }
            }
            else
            {
                while (source.volume > value)
                {
                    source.volume -= Time.deltaTime / 2;

                    yield return null;
                }
            }
            source.volume = value;
        }

        yield return null;
    }
}
