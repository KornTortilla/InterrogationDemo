using UnityEngine;

public class CanvasGroupFadeInOut : MonoBehaviour
{
    [SerializeField] private bool beginOpaque;
    [SerializeField] private float alpha;
    [SerializeField] private float time;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        //If not set to begin opaque, set alpha to 0 instantly
        if (!beginOpaque) LeanTween.alphaCanvas(canvasGroup, 0, 0);
    }

    public void FadeIn()
    {
        LeanTween.alphaCanvas(canvasGroup, alpha, time);
        canvasGroup.interactable = true;
    }

    public void FadeOut()
    {
        LeanTween.alphaCanvas(canvasGroup, 0, time);
    }
}

