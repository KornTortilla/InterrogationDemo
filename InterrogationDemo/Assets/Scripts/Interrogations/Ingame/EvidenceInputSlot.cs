using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Interrogation.Ingame
{
    public class EvidenceInputSlot : MonoBehaviour, IDropHandler
    {
        private bool canAcceptEvidence = false;
        private FadeInOut fader;

        private void Awake()
        {
            fader = GetComponent<FadeInOut>();
            //fader.FadeOut();
        }

        //When evidence is dropped on object, check if it will accept to signal event through evidence
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag != null && canAcceptEvidence)
            {
                eventData.pointerDrag.GetComponent<EvidenceDragger>().PassEvidence();
            }
        }

        private void OnEnable()
        {
            //Listening to events so that evidence isn't accepted while subject is talking
            InterrogationDialogueManager.OnTalkingStart += DisableAcceptEvidence;
            InterrogationDialogueManager.OnTalkingEnd += EnableAcceptEvidence;

            //Listening to events so that image is able to show on evidence dragging
            EvidenceDragger.OnEvidenceDragBegin += EnableImage;
            EvidenceDragger.OnEvidenceDragEnd += DisableImage;
        }

        private void OnDisable()
        {
            InterrogationDialogueManager.OnTalkingStart -= DisableAcceptEvidence;
            InterrogationDialogueManager.OnTalkingEnd -= EnableAcceptEvidence;

            EvidenceDragger.OnEvidenceDragBegin -= EnableImage;
            EvidenceDragger.OnEvidenceDragEnd -= DisableImage;
        }

        private void DisableAcceptEvidence()
        {
            canAcceptEvidence = false;
        }

        private void EnableAcceptEvidence()
        {
            canAcceptEvidence = true;
        }

        private void EnableImage()
        {
            if(canAcceptEvidence)
            {
                fader.FadeIn();
            }
        }

        private void DisableImage()
        {
            fader.FadeOut();
        }
    }
}
