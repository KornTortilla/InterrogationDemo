using UnityEngine;
using UnityEngine.UI;

public class VerticalDrawerTween : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private AnimationCurve closeCurve;

    private RectTransform rectTransform;
    private float topY;
    private float bottomY;
    private bool open;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        Canvas parentCanvas = transform.parent.GetComponent<Canvas>();
        float parentHeight = parentCanvas.pixelRect.height / parentCanvas.scaleFactor;
        Debug.Log("Parent Height: " + parentHeight);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, parentHeight);
        
        //Get left side by anchored position assuming object is set to be anchored to the left of canvas
        topY = rectTransform.anchoredPosition.y;
        //Gets right side by adding to left side with width of evidence list
        bottomY = rectTransform.anchoredPosition.y - rectTransform.rect.height;
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
