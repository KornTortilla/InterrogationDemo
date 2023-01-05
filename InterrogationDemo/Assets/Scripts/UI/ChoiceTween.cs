using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceTween : MonoBehaviour
{
    [SerializeField] private float inTime;
    [SerializeField] private float outTime;
    [SerializeField] private AnimationCurve animCurve;

    [HideInInspector] public float xScale;

    private RectTransform rectTransform;
    private float time;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    //Gets called by interrogation manager, dir of -1 is left, 1 is right
    public void Move(int dir, bool isIn)
    {
        CheckDirection(isIn);
        //Moves on x-axis and is controlled by animCurve
        LeanTween.moveX(rectTransform, rectTransform.localPosition.x + (xScale * dir), time).setEase(animCurve);
    }

    //Checks whether the object is sliding into the scene or out of to be gone
    private void CheckDirection(bool isIn)
    {
        if (isIn) time = inTime;
        else time = outTime;
    }
}
