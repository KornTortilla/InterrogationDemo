using UnityEngine;

public class HorizontalDrawerTween : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private AnimationCurve closeCurve;

    private RectTransform rectTransform;
    private float leftX;
    private float rightX;
    private bool open;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        leftX = rectTransform.anchoredPosition.x;
        rightX = leftX + transform.GetChild(0).GetComponent<RectTransform>().rect.width;
        open = false;
    }

    public void Move()
    {
        if (!open)
        {
            LeanTween.moveX(rectTransform, rightX, time).setEase(openCurve);
        }
        else
        {
            LeanTween.moveX(rectTransform, leftX, time).setEase(closeCurve);
        }

        open = !open;
    }
}
