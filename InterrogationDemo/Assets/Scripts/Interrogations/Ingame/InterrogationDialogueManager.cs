using System;
using System.Collections;
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
        public static event Action OnTalkingStart;
        public static event Action OnTalkingEnd;

        [SerializeField] private bool testing;

        [SerializeField] private GameObject choicePanel;
        [SerializeField] private GameObject evidenceLocker;
        [SerializeField] private Button hintButton;
        [SerializeField] private GameObject hintListButtonObject;
        [SerializeField] private DialogueTextManager dialogueTextManager;
        [SerializeField] private Button saveDialogueButton;
        [SerializeField] private FlowchartManager flowchartManager;
        [SerializeField] private GameObject flowchartButtonObject;

        [SerializeField] private DialogueManagerInterrogationSO dialogueManager;
        private DialogueInterrogationSO currentDialogue;
        private Stack<DialogueInterrogationSO> pastDialogues;

        private string partnerName;
        private List<string> hints;

        private DialogueInterrogationSO savedDialogue;
        private GameObject savedDialogueObject;

        private void Awake()
        {
            //Debug.Log(GameManager.Instance.TransitionData);

            if(!testing)
            {
                dialogueManager = Resources.Load("InterrogationFiles/" + GameManager.Instance.TransitionData + "/" + GameManager.Instance.TransitionData) as DialogueManagerInterrogationSO;
            }

            //Instantiates dynamic stack
            pastDialogues = new Stack<DialogueInterrogationSO>();

            partnerName = dialogueManager.PartnerName;

            Debug.Log(partnerName);

            if(!string.IsNullOrEmpty(partnerName))
            {
                StageController.Instance.AddCharacter(partnerName, -1f);
                StageController.Instance.SetCharacterPosition(partnerName, "offscreenright");
            }

            hints = new List<string>();

            OpenPathsAndGetStartPoint();

            //Once first dialogue found, push it
            pastDialogues.Push(currentDialogue);

            InstantiateEvidence();

            dialogueTextManager.CheckForPriorityCommand(dialogueManager.IntroText);

            if(testing) StartInterrogation();
        }

        //Temporary initialization by screen fade in to start script after full
        private void OnEnable()
        {
            GameManager.OnSceneTransitionEnd += Intro;
            DialogueTextManager.TutorialActivation += ActivateObject;
        }

        private void OnDisable()
        {
            GameManager.OnSceneTransitionEnd -= Intro;
            DialogueTextManager.TutorialActivation -= ActivateObject;
        }

        #region Initializing Methods

        /*
        IEnumerator ParseBriefing()
        {
            if(!String.IsNullOrEmpty(dialogueManager.BriefingText))
            {
                string[] sentences = dialogueManager.BriefingText.Split(
                    new string[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                );

                StartCoroutine(dialogueTextManager.TypeText(sentences, characterAnimator));

                yield return new WaitUntil(() => dialogueTextManager.isDone);



                yield return new WaitUntil(() => dialogueTextManager.isDone);

                GameObject choiceButton = Resources.Load("Prefabs/Interrogation/Creation/ChoiceButton") as GameObject;

                GameObject button = Instantiate(choiceButton, choicePanel.transform);
                button.transform.GetChild(0).GetComponent<TMP_Text>().text = "Head in.";
                button.GetComponent<Button>().onClick.AddListener(() => StartInterrogation());

                button.GetComponent<RectTransform>().sizeDelta = new Vector2(button.transform.parent.GetComponent<RectTransform>().rect.width, button.GetComponent<RectTransform>().sizeDelta.y);
                float x = choicePanel.transform.GetComponent<RectTransform>().rect.width;
                //Gets new y by splitting height into sections based on number of buttons and placing each button into new sections at a time, and offsetting by half the height
                float y = choicePanel.GetComponent<RectTransform>().rect.height / 2;
                //Sets starting position of button before motion
                button.transform.localPosition = new Vector3(x, 0f, button.transform.localPosition.z);

                //Instantiates movement script and sets button to move in direction of motion
                button.GetComponent<ChoiceTween>().xScale = Mathf.Abs(x);
                button.GetComponent<ChoiceTween>().Move(-1, true);
            }
            else
            {
                StartInterrogation();
            }
        }
        */

        private void OpenPathsAndGetStartPoint()
        {
            //Combs through dialogue in scene and opens paths that don't have keys/are not locked
            foreach (DialogueInterrogationSO dialogue in dialogueManager.DialogueList)
            {
                foreach (DialogueChoiceData choiceData in dialogue.Choices)
                {
                    if (choiceData.Keys.Count == 0 || choiceData.Keys == null)
                    {
                        choiceData.Opened = true;
                    }
                }

                //If there is no previous dialogue, it is the start
                if (dialogue.PreviousDialogue == null)
                {
                    currentDialogue = dialogue;
                }
            }
        }

        public virtual void Intro()
        {
            StartInterrogation();
        }

        private IEnumerator WaitForIntroText()
        {

            if(!string.IsNullOrEmpty(dialogueManager.IntroText))
            {
                yield return StartCoroutine(dialogueTextManager.TypeText(dialogueManager.IntroText));
            }

            StartInterrogation();
        }

        public void StartInterrogation()
        {
            //Removes current choice buttons
            foreach (Transform child in choicePanel.transform)
            {
                child.GetComponent<ChoiceTween>().Move(-1, false);

                child.GetComponent<Button>().enabled = false;

                GameObject.Destroy(child.gameObject, 0.5f);
            }

            hintButton.interactable = true;

            StartCoroutine(ParseCurrentDialogue(-1, currentDialogue.Text));

            flowchartManager.Initialize(currentDialogue, this);

            flowchartManager.UpdateFlowchart(currentDialogue, true);
        }

        private void InstantiateEvidence()
        {
            GameObject evidencePrefab = Resources.Load("Prefabs/Interrogation/Creation/EvidenceEntry") as GameObject;

            foreach (EvidenceInterrogationSO evidence in dialogueManager.EvidenceList)
            {
                //Instantiate object under the locker's first child, being the list
                GameObject evidenceObject = Instantiate(evidencePrefab, evidenceLocker.transform.GetChild(0));

                InitializeEvidence(evidenceObject, evidence.Name, evidence.Text, evidence);
            }

            //Rebuild vertical layout group to fix possible issues with placement of evidence objects
            LayoutRebuilder.ForceRebuildLayoutImmediate(evidenceLocker.transform.GetChild(0).GetComponent<RectTransform>());
        }
        #endregion

        #region creation Methods
        public void CreateDialogueEvidence()
        {
            //If there is already a saved dialogue, delete the existing one to make way for the new one
            if(savedDialogueObject != null)
            {
                savedDialogue.Grabbed = false;

                if (currentDialogue == savedDialogue) saveDialogueButton.interactable = true;

                Destroy(savedDialogueObject);
            }

            GameObject dialoguePrefab = Resources.Load("Prefabs/Interrogation/Creation/DialogueEntry") as GameObject;

            //Splits sentences and grabs last sentence
            string[] sentences = dialogueTextManager.SplitIntoSentences(currentDialogue.Text);
            string sentence = sentences[sentences.Length - 1];

            //If speaker mentioned, exclude speaker's name to assume suspect ends all conversations
            if (sentence.Contains(":"))
            {
                sentence = sentence.Substring(sentence.IndexOf(":") + 2);
            }

            //Instantiate object under the locker's first child, being the list
            GameObject dialogueObject = Instantiate(dialoguePrefab, evidenceLocker.transform.GetChild(0));

            InitializeEvidence(dialogueObject, currentDialogue.Name, sentence, currentDialogue);

            savedDialogueObject = dialogueObject;

            currentDialogue.Grabbed = true;

            savedDialogue = currentDialogue;

            saveDialogueButton.interactable = false;

            LayoutRebuilder.ForceRebuildLayoutImmediate(evidenceLocker.transform.GetChild(0).GetComponent<RectTransform>());
        }

        private void InitializeEvidence(GameObject eObject, string name, string desc, ScriptableObject sObject)
        {
            //Changes text of second and third child to match evidence title and description
            eObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = name;
            eObject.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = desc;

            //Ties function to unity event of script
            eObject.GetComponent<EvidenceDragger>().onEvidenceAcccepted.AddListener(() => CheckContradiction(sObject));
        }

        private void CreateChoicesButtons(int dir)
        {
            //Creates list of buttons to populate
            List<GameObject> buttons = new List<GameObject>();
            //Gets choice button prefab to create new buttons out of
            GameObject choiceButton = Resources.Load("Prefabs/Interrogation/Creation/ChoiceButton") as GameObject;

            //For every choice avaliable, create one with its text
            foreach (DialogueChoiceData choiceData in currentDialogue.Choices)
            {
                //If a choice is open and points to another dialogue, make a choice button for it
                if (choiceData.Opened && choiceData.NextDialogue != null)
                {
                    //Instantiate button and replace its text with the choice text
                    GameObject button = Instantiate(choiceButton, choicePanel.transform);
                    button.transform.GetChild(0).GetComponent<TMP_Text>().text = choiceData.Text;

                    //Tie function with bytton click
                    button.GetComponent<Button>().onClick.AddListener(() => Decide(choiceData.NextDialogue));

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

                //Increase width of button to size of parent, leave height as is
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(button.transform.parent.GetComponent<RectTransform>().rect.width, button.GetComponent<RectTransform>().sizeDelta.y);
                //Gets new x with the width of parent and in the direction opposite of where the button slides
                float x = choicePanel.transform.GetComponent<RectTransform>().rect.width * -dir;
                //Gets new y by splitting height into sections based on number of buttons and placing each button into new sections at a time, and offsetting by half the height
                float y = ((-choicePanel.GetComponent<RectTransform>().rect.height / (buttons.Count + 1)) * ((i) + 1)) + (choicePanel.GetComponent<RectTransform>().rect.height / 2);
                //Sets starting position of button before motion
                button.transform.localPosition = new Vector3(x, y, button.transform.localPosition.z);

                //Instantiates movement script and sets button to move in direction of motion
                button.GetComponent<ChoiceTween>().xScale = Mathf.Abs(x);
                button.GetComponent<ChoiceTween>().Move(dir, true);
            }
        }
        #endregion

        #region Text Method
        IEnumerator ParseCurrentDialogue(int dir, string text, bool isProgressing = true)
        {
            //Calls event when characters speak
            OnTalkingStart?.Invoke();

            foreach(Button button in choicePanel.GetComponentsInChildren<Button>())
            {
                button.interactable = false;
            }

            //Disables save button so players cannot save unless seeing the full dialogue
            hintButton.interactable = false;
            saveDialogueButton.interactable = false;

            bool needToClickLastText = !isProgressing;

            StartCoroutine(dialogueTextManager.TypeText(text, needToClickLastText));

            yield return new WaitUntil(() => dialogueTextManager.isDone);

            OnTalkingEnd?.Invoke();

            foreach (Button button in choicePanel.GetComponentsInChildren<Button>())
            {
                button.interactable = true;
            }

            hintButton.interactable = true;
            saveDialogueButton.interactable = true;

            if (isProgressing)
            {
                //Goes onto create choice buttons once dialogue is finished
                CreateChoicesButtons(dir);
            }
            else
            {
                dialogueTextManager.StampLastSentence(currentDialogue.Text);
                
                if(!string.IsNullOrEmpty(partnerName))
                {
                    StageController.Instance.HintSlide(partnerName, false);
                }
            }
        }
        #endregion

        #region Choice Methods
        //Gets called by clicking choice buttons created in CreatedChoiceButtons()
        private void Decide(DialogueInterrogationSO nextDialogue)
        {
            AdvanceDialogue(nextDialogue, -1);

            pastDialogues.Push(currentDialogue);

            //Updates flowchart with new dialogue
            flowchartManager.UpdateFlowchart(currentDialogue, true);
        }

        //Gets called by clicking back button created in CreatedChoiceButtons()
        private void Back()
        {
            pastDialogues.Pop();

            //Updates flowchart with new dialogue
            flowchartManager.UpdateFlowchart(currentDialogue, false);

            AdvanceDialogue(currentDialogue.PreviousDialogue, 1);
        }

        //Gets called by clicking flowchart buttons, reference FlowchartNode script
        public void Jump(DialogueInterrogationSO dialogue)
        {
            //Automatically toggles view to get out of flowchart
            GetComponent<CameraController>().AlternateCameras();

            AdvanceDialogue(dialogue, -1);

            //Refills dialogues to get
            pastDialogues.Clear();

            WalkBackDialogueList(currentDialogue);

            //Recolors all flowchart nodes based on new dialogue list
            flowchartManager.RehighlightedNodes(pastDialogues);
        }

        private void WalkBackDialogueList(DialogueInterrogationSO dialogue)
        {
            //Starts at a specific dialogue and walks backwards through previous dialogues to add until none are left
            pastDialogues.Push(dialogue);

            if (dialogue.PreviousDialogue != null)
            {
                WalkBackDialogueList(dialogue.PreviousDialogue);
            }
        }

        private void AdvanceDialogue(DialogueInterrogationSO nextDialogue, int dir)
        {
            if (nextDialogue == null)
            {
                return;
            }

            //Removes current choice buttons
            foreach (Transform child in choicePanel.transform)
            {
                child.GetComponent<ChoiceTween>().Move(dir, false);

                child.GetComponent<Button>().enabled = false;

                GameObject.Destroy(child.gameObject, 0.5f);
            }

            //Marks current dialogue and advance to new one
            currentDialogue.Seen = true;

            currentDialogue = nextDialogue;

            //If the new dialogue is already in evidence, dont show save button
            if (currentDialogue.Grabbed) saveDialogueButton.interactable = false;
            else saveDialogueButton.interactable = true;

            //Makes sure all text coroutines are done before starting new ones
            StopAllCoroutines();
             
            //Start typing coroutine if a new dialogue yet to be seen
            if (!currentDialogue.Seen)
            {
                foreach(DialogueChoiceData choice in currentDialogue.Choices)
                {
                    if(!string.IsNullOrEmpty(choice.Hint))
                    {
                        hints.Add(choice.Hint);
                    }
                }

                StartCoroutine(ParseCurrentDialogue(dir, currentDialogue.Text));
            }
            //Or get the last sentence minus the possible speaker name and directly puts into text
            else
            {
                dialogueTextManager.StampLastSentence(currentDialogue.Text);

                CreateChoicesButtons(dir);
            }
        }
        #endregion

        #region Contradiction Method
        //Gets called by UnityEvent invoked evidence created in InstantiateEvidence() being accepted by evidence input 
        public void CheckContradiction(ScriptableObject sObject)
        {
            //Assuming drawer open, closes by calling move function
            evidenceLocker.GetComponent<HorizontalDrawerTween>().Move();

            //Instantiates bool to check if there is a key for a lock, and a choice that it may be tied to
            bool contradictionFound = false;
            DialogueChoiceData openedPath = new DialogueChoiceData();

            foreach (DialogueChoiceData choiceData in currentDialogue.Choices)
            {
                foreach (ScriptableObject key in choiceData.Keys)
                {
                    //If evidence or dialogue is a key to any choice in the current dialogue,
                    // we got a contradiction and save the choice it was a key to
                    if (sObject == key)
                    {
                        openedPath = choiceData;
                        contradictionFound = true;
                    }
                }
            }

            //When contradiction found, try to add a new music layer, open up the path that was locked, and continue onto the choice
            if (contradictionFound)
            {
                AudioManager.Instance.AddMusicLayer();

                openedPath.Opened = true;

                Decide(openedPath.NextDialogue);
            }

            //Dialogue when contradiction is found found to be added
            else
            {
                string distinctError = "";

                foreach(DialogueErrorData errorData in currentDialogue.Errors)
                {
                    foreach (ScriptableObject e in errorData.Evidence)
                    {
                        //If evidence or dialogue is a key to any choice in the current dialogue,
                        // we got a contradiction and save the choice it was a key to
                        if (sObject == e)
                        {
                            distinctError = errorData.Text;
                        }
                    }
                }

                if(!string.IsNullOrEmpty(distinctError))
                {
                    StartCoroutine(ParseCurrentDialogue(-1, distinctError, false));
                }
                else
                {
                    StartCoroutine(ParseCurrentDialogue(-1, dialogueManager.DefaultErrorResponse, false));
                }
            }
        }
        #endregion

        public void GetHint()
        {
            string hint;

            if(hints.Count > 0)
            {
                int rand = UnityEngine.Random.Range(0, hints.Count);

                hint = hints[rand];
                hints.RemoveAt(rand);
            }
            else
            {
                hint = dialogueManager.NoHintResponse;
            }

            if (!string.IsNullOrEmpty(partnerName))
            {
                StageController.Instance.HintSlide(partnerName, true);
            }

            StartCoroutine(ParseCurrentDialogue(-1, hint, false));
        }

        private void ActivateObject(string stringObject)
        {
            switch(stringObject)
            {
                case "locker":
                    evidenceLocker.GetComponent<CanvasGroupFadeInOut>().FadeIn();
                    break;

                case "save":
                    saveDialogueButton.gameObject.SetActive(true);
                    break;

                case "flowchart":
                    flowchartButtonObject.SetActive(true);
                    break;

                case "hint":
                    hintButton.gameObject.SetActive(true);
                    hintListButtonObject.SetActive(true);
                    break;
            }
        }
    }
}
