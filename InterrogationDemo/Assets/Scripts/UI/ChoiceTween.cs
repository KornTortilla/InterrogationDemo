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

    public void Move(int dir, bool isIn)
    {
        CheckDirection(isIn);
        LeanTween.moveX(rectTransform, rectTransform.localPosition.x + (xScale * dir), time).setEase(animCurve);
    }

    private void CheckDirection(bool isIn)
    {
        if (isIn) time = inTime;
        else time = outTime;
    }
}
