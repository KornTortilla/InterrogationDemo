using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Interrogation.Ingame
{
    using Data;
    using ScriptableObjects;
    using System.Collections;

    public class InterrogationDialogueManager : MonoBehaviour
    {
        public static event Action OnTalkingStart;
        public static event Action OnContradictionFound;
        public static event Action<String> OnMusicChange;

        [SerializeField] private GameObject choicePanel;
        [SerializeField] private GameObject evidenceLocker;
        [SerializeField] private DialogueTextManager dialogueTextManager;
        [SerializeField] private Animator characterAnimator;

        [SerializeField] private DialogueSOManager dialogueManager;
        private DialogueSO currentDialogue;
        private Stack<DialogueSO> pastDialogues;

        private void Awake()
        {
            //Instantiates dynamic stack
            pastDialogues = new Stack<DialogueSO>();

            OpenPathsAndGetStartPoint();

            pastDialogues.Push(currentDialogue);

            StartCoroutine(ParseCurrentDialogue(-1));
        }
        
        private void OpenPathsAndGetStartPoint()
        {
            //Combs through dialogue in scene and opens paths that don't have keys/are not locked
            foreach (DialogueSO dialogue in dialogueManager.DialogueList)
            {
                foreach (DialogueChoiceData choiceData in dialogue.Choices)
                {
                    if (choiceData.Keys.Count == 0 || choiceData.Keys == null)
                    {
                        choiceData.Opened = true;
                    }
                }

                //If there is no previous dialogue for one, it is the start
                if (dialogue.PreviousDialogue == null)
                {
                    currentDialogue = dialogue;
                }
            }
        }

        private void InstantiateEvidence()
        {
            /*
            foreach (EvidenceSO evidence in dialogueManager.EvidenceList)
            {
                GameObject evidenceObject = Instantiate(evidenceEntry, evidenceLocker.transform.GetChild(0));
                evidenceObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = evidence.Name;
                evidenceObject.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = evidence.Text;

                evidenceObject.GetComponent<EvidencePassThrough>().sObject = evidence;
                evidenceObject.GetComponent<EvidencePassThrough>().interroDialogueManager = this.gameObject;

                evidenceObject.GetComponent<DragAndDrop>().evidenceInput = evidenceInput;
                evidenceObject.GetComponent<DragAndDrop>().startingPos = evidenceObject.transform.GetComponent<RectTransform>().anchoredPosition;

                LayoutRebuilder.ForceRebuildLayoutImmediate(evidenceObject.GetComponent<RectTransform>());
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(evidenceLocker.transform.GetChild(0).GetComponent<RectTransform>());
            */
        }

        IEnumerator ParseCurrentDialogue(int dir)
        {
            //Calls event when characters speak
            OnTalkingStart?.Invoke();

            //Split sentences of text into lines and stores each line
            string[] sentences = SplitCurrentSentences();

            for (int i = 0; i < sentences.Length; i++)
            {
                string sentence = sentences[i];

                /*
                If sentence begins with #, it is a tag and will sepearate the 
                 first and second words to invoke different events.
                */
                if (sentence.Substring(0, 1) == "#")
                {
                    string tag = sentence.Substring(1);

                    string prefix = tag.Split(' ')[0].ToLower();
                    string param = tag.Split(' ')[1];

                    switch (prefix)
                    {
                        case "anim":
                            characterAnimator.Play(param);
                            break;

                        case "music":
                            OnMusicChange?.Invoke(param);
                            break;
                    }

                    continue;
                }

                //If not tag, send sentence to dialogue to write out
                StartCoroutine(dialogueTextManager.TypeText(sentence, characterAnimator));

                //If the dialogue is done being typed out and the player left clicks, continue
                yield return new WaitUntil(() => { return (((Input.GetMouseButtonDown(0)) || i == sentences.Length - 1) && dialogueTextManager.isDone); });
            }

            CreateChoices(dir);
        }

        private void CreateChoices(int dir)
        {
            //Creates list of buttons to populate
            List<GameObject> buttons = new List<GameObject>();
            //Gets choice button prefab to create new buttons out of
            GameObject choiceButton = Resources.Load("Prefabs/ChoiceButton") as GameObject;

            //For every choice avaliable, create one with its text
            foreach (DialogueChoiceData choiceData in currentDialogue.Choices)
            {
                if (choiceData.Opened && choiceData.NextDialogue != null)
                {
                    GameObject button = Instantiate(choiceButton, choicePanel.transform);
                    button.transform.GetChild(0).GetComponent<TMP_Text>().text = choiceData.Text;

                    button.GetComponent<Button>().onClick.AddListener(() => Decide(choiceData));
                    
                    //If its been seen, color it darker
                    if (choiceData.NextDialogue.Seen)
                    {
                        button.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
                    }

                    buttons.Add(button);
                }
            }

            //If there is a dialogue to go back to, create a back button
            if (currentDialogue.PreviousDialogue != null)
            {
                GameObject backButton = Instantiate(choiceButton, choicePanel.transform);
                backButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Back";

                backButton.GetComponent<Button>().onClick.AddListener(() => Back());

                buttons.Add(backButton);
            }

            //Once created, reposition all buttons so they can move in from the correct direction
            for (int i = 0; i < buttons.Count; i++)
            {
                GameObject button = buttons[i];

                button.GetComponent<RectTransform>().sizeDelta = new Vector2(button.transform.parent.GetComponent<RectTransform>().rect.width, button.GetComponent<RectTransform>().sizeDelta.y);
                float x = choicePanel.transform.GetComponent<RectTransform>().rect.width * -dir;
                float y = ((-choicePanel.GetComponent<RectTransform>().rect.height / (buttons.Count + 1)) * ((i) + 1)) + (choicePanel.GetComponent<RectTransform>().rect.height / 2);
                button.transform.localPosition = new Vector3(x, y, button.transform.localPosition.z);

                button.GetComponent<ChoiceTween>().xScale = Mathf.Abs(x);

                button.GetComponent<ChoiceTween>().Move(dir, true);
            }
        }

        private void Decide(DialogueChoiceData choiceData)
        {
            AdvanceDialogue(choiceData.NextDialogue, -1);

            pastDialogues.Push(currentDialogue);
        }

        private void Back()
        {
            pastDialogues.Pop();

            AdvanceDialogue(currentDialogue.PreviousDialogue, 1);
        }

        private void AdvanceDialogue(DialogueSO nextDialogue, int dir)
        {
            if (nextDialogue == null)
            {
                return;
            }

            foreach (Transform child in choicePanel.transform)
            {
                child.GetComponent<ChoiceTween>().Move(dir, false);

                child.GetComponent<Button>().enabled = false;

                GameObject.Destroy(child.gameObject, 0.5f);
            }

            currentDialogue.Seen = true;

            currentDialogue = nextDialogue;

            StopAllCoroutines();

            if (!currentDialogue.Seen)
            {
                StartCoroutine(ParseCurrentDialogue(dir));
            }
            else
            {
                string[] sentences = SplitCurrentSentences();

                string sentence = sentences[sentences.Length - 1];

                if (sentence.IndexOf(":") != -1)
                {
                    int colon = currentDialogue.Text.IndexOf(":");
                    sentence = sentence.Substring(colon + 2);
                }

                dialogueTextManager.ShowText(sentence);

                CreateChoices(dir);
            }
        }

        private string[] SplitCurrentSentences()
        {
            string[] sentences = currentDialogue.Text.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

            return sentences;
        }
    }
}
