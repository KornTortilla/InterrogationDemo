using System;
using UnityEngine;

public class UISceneFading : MonoBehaviour
{
    public static event Action OnScreenAwake;

    public RectTransform blackScreenRect;
    public float fadeTime = 1f;

    //Current scene starter, initializes interrogation manager through event. Temporary.
    private void Awake()
    {
        LeanTween.alpha(blackScreenRect, 0f, fadeTime).setOnComplete(EventInvoke);
    }

    private void EventInvoke()
    {
        OnScreenAwake?.Invoke();
    }

    private void OnEnable()
    {
        //DialogueTextManager.OnGameEnd += FadeOut;
    }

    private void OnDisable()
    {
        //DialogueTextManager.OnGameEnd -= FadeOut;
    }

    //Fades in black screen once interrogation manager signals the end
    private void FadeOut()
    {
        LeanTween.alpha(blackScreenRect, 1f, fadeTime);
    }
}
