using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Interrogation.Ingame
{
    using ScriptableObjects;

    public class EvidenceDragger : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        //Events to signal to the evidence input to highlight or not
        public static event Action OnEvidenceDragBegin;
        public static event Action OnEvidenceDragEnd;
        //Unity event so the interrogation manager can tie it to its own contradiction check
        public UnityEvent onEvidenceAcccepted;

        private Vector2 startingPos;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        //Called when evidence input is able to take the game object
        public void PassEvidence()
        {
            onEvidenceAcccepted?.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Signals that the draggin begins
            OnEvidenceDragBegin?.Invoke();

            //Gets starting pos so that it can return when done dragging
            startingPos = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0.6f;
            //Disables collision so that evidence input can't detect while dragging
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Movement
            rectTransform.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Signals the end of dragging
            OnEvidenceDragEnd?.Invoke();

            canvasGroup.alpha = 1;
            //Allows collision at the end so that evidence input can detect
            canvasGroup.blocksRaycasts = true;
            //Resets to origin
            rectTransform.anchoredPosition = startingPos;
        }
    }
}
