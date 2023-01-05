using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    [SerializeField] private bool beginOpaque;
    [SerializeField] private float alpha;
    [SerializeField] private float time;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        //If not set to begin opaque, set alpha to 0 instantly
        if (!beginOpaque) LeanTween.alpha(rectTransform, 0, 0);
    }

    public void FadeIn()
    {
        LeanTween.alpha(rectTransform, alpha, time);
    }

    public void FadeOut()
    {
        LeanTween.alpha(rectTransform, 0, time);
    }
}

