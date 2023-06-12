using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParent : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        LeanTween.alphaCanvas(canvasGroup, 1f, 1f);
    }

    private void OnEnable()
    {
        GameManager.OnSceneTransitionBegin += FadeOut;
        GameManager.OnSceneTransitionAdd += FadeIn;
    }

    private void OnDisable()
    {
        GameManager.OnSceneTransitionBegin -= FadeOut;
        GameManager.OnSceneTransitionAdd -= FadeIn;
    }

    private void FadeOut()
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, 1f);
    }

    private void FadeIn()
    {
        Debug.Log("Fadin'");

        LeanTween.alphaCanvas(canvasGroup, 1f, 1f);
    }
}
