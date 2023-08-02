using UnityEngine;

public class VerticalDrawerTween : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private AnimationCurve closeCurve;

    private RectTransform rectTransform;
    private float topY;
    private float bottomY;
    private bool open;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        //Get left side by anchored position assuming object is set to be anchored to the left of canvas
        topY = rectTransform.anchoredPosition.y;
        //Gets right side by adding to left side with width of evidence list
        bottomY = 0;
        //Is closed by default
        open = false;
    }

    public void Move()
    {
        //Moves based on opening or closing drawer
        if (!open)
        {
            LeanTween.moveY(rectTransform, bottomY, time).setEase(openCurve);
        }
        else
        {
            LeanTween.moveY(rectTransform, topY, time).setEase(closeCurve);
        }

        open = !open;
    }
}
