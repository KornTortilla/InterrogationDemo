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

