using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private RectTransform blackScreenRect;
    [SerializeField] private float fadeTime = 1f;

    public static event Action OnSceneTransitionBegin;
    public static event Action OnSceneTransitionAdd;

    public string TransitionData { get; set; }

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
        }
    }

    public void StartGame(string scene)
    {
        TransitionScenes(scene);
    }

    public void TransitionScenes(string scene, string arg = null)
    {
        nextScene = scene;

        TransitionData = arg;

        OnSceneTransitionBegin?.Invoke();

        Debug.Log("Fading");

        Crossfade(scene);
    }

    private void Crossfade(string scene)
    {
        LeanTween.alpha(blackScreenRect, 0f, fadeTime).setOnComplete(Transition);
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

        LeanTween.alpha(blackScreenRect, 0f, fadeTime);
    }
}
