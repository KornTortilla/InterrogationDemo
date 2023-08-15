using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        FadeIn(GameManager.Instance.fadeTime);
    }

    private void OnEnable()
    {
        GameManager.OnSceneTransitionBegin += FadeOut;
    }

    private void OnDisable()
    {
        GameManager.OnSceneTransitionBegin -= FadeOut;
    }

    private void FadeIn(float time)
    {
        Debug.Log("Time: " + time);

        LeanTween.alphaCanvas(canvasGroup, 1f, time);
    }

    private void FadeOut(float time)
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, time);
    }
}
