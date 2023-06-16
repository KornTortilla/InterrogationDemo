using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private RectTransform blackScreenRect;
    [SerializeField] private float fadeTime = 0.5f;

    public static event Action OnSceneTransitionBegin;
    public static event Action OnSceneTransitionAdd;
    public static event Action OnSceneTransitionEnd;

    [SerializeField] public string TransitionData { get; private set; }

    public bool starting;

    private string nextScene;

    private void Awake()
    {
        //Singleton Pattern setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if(starting)
        {
            SceneManager.LoadSceneAsync("Title", LoadSceneMode.Additive);

            AudioManager.Instance.PlayNewTrack("Title");
        }
    }

    public void StartGame(string scene)
    {
        TransitionScenes(scene, true);

        AudioManager.Instance.StopMusic();
    }

    public void TransitionScenes(string scene, bool blackScreenTransition, string arg = null)
    {
        blackScreenRect.gameObject.SetActive(true);

        nextScene = scene;

        TransitionData = arg;

        OnSceneTransitionBegin?.Invoke();

        Crossfade(scene, blackScreenTransition);
    }

    private void Crossfade(string scene, bool blackScreenTransition)
    {
        float a;
        if (blackScreenTransition) a = 1f;
        else a = 0f;

        LeanTween.alpha(blackScreenRect, a, fadeTime).setOnComplete(Transition);
    }

    private void Transition()
    {
        //StageController.Instance.ClearStage();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name is not "Background")
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);

        OnSceneTransitionAdd?.Invoke();

        LeanTween.alpha(blackScreenRect, 0f, fadeTime).setOnComplete(SignalTransitionEnd);
    }

    private void SignalTransitionEnd()
    {
        OnSceneTransitionEnd?.Invoke();

        blackScreenRect.gameObject.SetActive(false);
    }
}
