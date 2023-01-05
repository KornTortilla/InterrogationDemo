using System;
using UnityEngine;
using Interrogation.Ingame;

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
        InterrogationDialogueManager.OnInterrogationEnd += FadeOut;
    }

    private void OnDisable()
    {
        InterrogationDialogueManager.OnInterrogationEnd -= FadeOut;
    }

    //Fades in black screen once interrogation manager signals the end
    private void FadeOut()
    {
        Debug.Log("lmao");
        LeanTween.alpha(blackScreenRect, 255f, fadeTime);
    }
}
