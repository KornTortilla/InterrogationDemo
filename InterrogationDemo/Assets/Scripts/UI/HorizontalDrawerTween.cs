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
        //Get left side by anchored position assuming object is set to be anchored to the left of canvas
        leftX = rectTransform.anchoredPosition.x;
        //Gets right side by adding to left side with width of evidence list
        rightX = leftX + transform.GetChild(0).GetComponent<RectTransform>().rect.width;
        //Is closed by default
        open = false;
    }

    public void Move()
    {
        //Moves based on opening or closing drawer
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
