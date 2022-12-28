using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interrogation.Ingame
{
    using Data;
    using ScriptableObjects;

    public class InterrogationDialogueManager : MonoBehaviour
    {
        public static event Action OnContradictionFound;
        public static event Action OnAnimationChange;
        public static event Action OnMusicChange;

        [SerializeField] private DialogueSOManager dialogueManager;
        private DialogueSO currentDialogue;
        public Stack<DialogueSO> pastDialogues;

        [SerializeField] private float textSpeed;

        private void Awake()
        {
            pastDialogues = new Stack<DialogueSO>();

            OpenPathsAndGetStart();
        }
        
        private void OpenPathsAndGetStart()
        {
            foreach (DialogueSO dialogue in dialogueManager.DialogueList)
            {
                foreach (DialogueChoiceData choiceData in dialogue.Choices)
                {
                    if (choiceData.Keys.Count == 0 || choiceData.Keys == null)
                    {
                        choiceData.Opened = true;
                    }
                }

                if (dialogue.PreviousDialogue == null)
                {
                    currentDialogue = dialogue;
                }
            }
        }
    }
}
