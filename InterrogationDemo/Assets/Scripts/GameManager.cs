using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static event Action<float> OnSceneTransitionBegin;
    public static event Action OnSceneTransitionEnd;

    public string TransitionData { get; private set; }
    public bool testing;

    [HideInInspector]
    public float fadeTime = 0f;

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

        if(!testing)
        {
            SceneManager.LoadSceneAsync("Title", LoadSceneMode.Additive);
        }
    }

    public void StartGame()
    {
        StartCoroutine(SceneTransition("Dialogue", true, 2f));

        AudioManager.Instance.StopMusic();
    }

    public IEnumerator SceneTransition(string newScene, bool isBlackScreenTransition, float time = 1f, string arg = null)
    {
        testing = false;

        OnSceneTransitionBegin?.Invoke(time);

        fadeTime = time;

        nextScene = newScene;

        TransitionData = arg;

        StageController.Instance.DropCurtain(time);

        yield return new WaitForSeconds(time);

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name is not "Background")
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }

        SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);

        StageController.Instance.PullCurtain(time);

        yield return new WaitForSeconds(time);

        OnSceneTransitionEnd?.Invoke();
    }
}