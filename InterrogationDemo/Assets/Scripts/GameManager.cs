using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private RectTransform blackScreenRect;
    private float fadeTime = 1f;

    public static event Action<float> OnSceneTransitionBegin;
    public static event Action<float> OnSceneTransitionAdd;
    public static event Action OnSceneTransitionEnd;

    public string TransitionData { get; private set; }

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
        TransitionScenes(scene, true, 2f);

        AudioManager.Instance.StopMusic();
    }

    public void TransitionScenes(string scene, bool blackScreenTransition, float time = 1f, string arg = null)
    {
        fadeTime = time;

        blackScreenRect.gameObject.SetActive(true);

        nextScene = scene;

        TransitionData = arg;

        OnSceneTransitionBegin?.Invoke(time);

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

        OnSceneTransitionAdd?.Invoke(fadeTime);

        LeanTween.alpha(blackScreenRect, 0f, fadeTime).setOnComplete(SignalTransitionEnd);
    }

    private void SignalTransitionEnd()
    {
        OnSceneTransitionEnd?.Invoke();

        blackScreenRect.gameObject.SetActive(false);
    }
}
