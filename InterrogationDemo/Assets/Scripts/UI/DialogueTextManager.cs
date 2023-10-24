using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueTextManager : MonoBehaviour
{
    public static Action<string, string> SentenceFinished;
    public static Action<string> TutorialActivation;

    public GameObject nameText;
    public TextMeshProUGUI dialogueText;
    public GameObject ctcObject;
    public Button ctcButton;

    private string currentLine;

    public float textSpeed;
    private float pauseTime;

    private FMODUnity.StudioEventEmitter voiceEmitter;

    private AudioClip blip;
    private bool odd;

    private bool clickProcessed;
    private bool interruptTyping;
    private bool stageReady = true;

    private bool odd;

    private void Awake()
    {
        voiceEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    private void OnEnable()
    {
        StageController.OnActionStart += StageUnready;
        StageController.OnActionComplete += StageReady;
    }

    private void OnDisable()
    {
        StageController.OnActionStart -= StageUnready;
        StageController.OnActionComplete -= StageReady;
    }

    private void StageUnready()
    {
        stageReady = false;
    }

    private void StageReady()
    {
        stageReady = true;
    }

    public void CheckForPriorityCommand(string text)
    {
        string[] lines = SplitIntoSentences(text);

        if (lines[0].Substring(0, 1) == "!")
        {
            SeperateAndExecute(lines[0]);
        }
    }

    public IEnumerator TypeText(string text, bool needToClickLastText = false)
    {
        string[] sentences = SplitIntoSentences(text);

        for (int i = 0; i < sentences.Length; i++)
        {
            pauseTime = 0;

            clickProcessed = false;

            yield return new WaitUntil(() => stageReady);

            currentLine = sentences[i];

            if(currentLine.Substring(0, 1) == "!")
            {
                continue;
            }

            /*
            If sentence begins with #, it is a tag and will sepearate the 
             first and second words to invoke different events.
            */
            if (currentLine.Substring(0, 1) == "#")
            {
                SeperateAndExecute(currentLine);

                //Skips to next sentence so that tag isn't displayed in text
                continue;
            }

            //Initializing beginning to simulate talking
            dialogueText.text = "";
            //By default, sets nameBox to be hidden unless utilized
            nameText.SetActive(false);
            ctcObject.SetActive(false);

            //If colon detected, name is given
            string name = "";
            if (currentLine.Contains(":"))
            {
                //Separates name and dialogue
                string[] sentenceArray = SeparateSentenceAndShowName(currentLine);
                name = sentenceArray[0];
                currentLine = sentenceArray[1];
            }

            char[] letterArray = currentLine.ToCharArray();
            for (int letterPos = 0; letterPos < letterArray.Length; letterPos++)
            {
                ShowTextbox();

                if (pauseTime > 0)
                {
                    yield return new WaitForSeconds(pauseTime);

                    pauseTime = 0;
                }

                char letter = letterArray[letterPos];

                if (interruptTyping)
                {
                    interruptTyping = false;

                    dialogueText.text = SearchForRemainingTagsAndRemove(currentLine, letterPos, true);

                    break;
                }

                float time = textSpeed;

                StageController.Instance.SetCurrentSpeakerTalking(true);

                odd = !odd;

                //If using punctuation, increase time delay and stop animation to simulate actual speech better
                if (letter == '.' || letter == '?' || letter == '!')
                {
                    time *= 10;
                    StageController.Instance.SetCurrentSpeakerTalking(false);
                }
                else if (letter == ',')
                {
                    time *= 5f;
                    StageController.Instance.SetCurrentSpeakerTalking(false);
                }
                else if (letter == '[')
                {
                    string parenthesisString = FindParenthesisString(currentLine.Substring(letterPos));

                    StripParenthesisAndExecute(parenthesisString);

                    letterPos += parenthesisString.Length - 1;

                    continue;
                }
                else if(letter != ' ' || letter != '(' || letter != ')')
                {
                    if (odd) AudioManager.Instance.PlayVoice();
                }

                dialogueText.text += letter;

                yield return StartCoroutine(Countdown(time));
            }

            StageController.Instance.SetCurrentSpeakerTalking(false);

            ctcObject.SetActive(true);

            clickProcessed = false;

            //If the dialogue is done being typed out and the player clicks on button, continue
            yield return new WaitUntil(() => { return ((clickProcessed || (i == sentences.Length - 1 && !needToClickLastText))); });

            SentenceFinished?.Invoke(name, TagsRemoved(currentLine));
        }

        ctcObject.SetActive(false);
    }

    private void SeperateAndExecute(string line)
    {
        //Splits each word of a tag into an array
        string[] commands = line.Substring(1).Split(',');

        for(int i = 0; i < commands.Length; i++)
        {
            string command = commands[i];
            //Due to space being after commas, gets rid of space as first char
            if(command.Substring(0, 1) == " ")
            {
                command = command.Substring(1);
            }

            ExecuteTag(command);
        }
    }

    private string SearchForRemainingTagsAndRemove(string sentence, int startingPosition, bool execute)
    {
        if(!sentence.Contains('['))
        {
            return sentence;
        }

        List<string> stringsToRemove = new List<string>();

        string remainingSentence = sentence;

        while(remainingSentence.Contains('['))
        {
            string parenthesisString = FindParenthesisString(remainingSentence);

            if(execute && remainingSentence.IndexOf('[') > startingPosition)
            {
                StripParenthesisAndExecute(parenthesisString);
            }

            stringsToRemove.Add(parenthesisString);

            remainingSentence = remainingSentence.Substring(remainingSentence.IndexOf(']') + 1);
        } 

        string replacingSentence = sentence;
        foreach(string stringToRemove in stringsToRemove)
        {
            replacingSentence = replacingSentence.Replace(stringToRemove, "");
        }

        return replacingSentence;
    }

    private string FindParenthesisString(string text)
    {
        int startOfParenth = text.IndexOf('[');
        int endOfParenth = text.IndexOf(']');

        string parenthesisString = text.Substring(startOfParenth, endOfParenth - startOfParenth + 1);

        return parenthesisString;
    }
    
    private void StripParenthesisAndExecute(string text)
    {
        string command = text.Substring(1, text.Length - 2);

        ExecuteTag(command);
    }

    private void ExecuteTag(string command)
    {
        //Splits each word of a tag into an array
        string[] terms = command.Split(' ');
        //Checks the first 
        switch (terms[0])
        {
            case "p" or "pause":
                pauseTime = float.Parse(terms[1]);
                break;

            case "background":
                StageController.Instance.SwitchBackground(terms[1], float.Parse(terms[2]));
                break;

            case "curtain":
                if (terms[1] == "fall") StageController.Instance.DropCurtain(float.Parse(terms[2]));
                else if (terms[1] == "rise") StageController.Instance.PullCurtain(float.Parse(terms[2]));
                break;

            case "add":
                //Adds character to stage based on only parameter
                StageController.Instance.AddCharacter(terms[1]);
                break;

            case "place":
                //Sets character to new position with two params, first being name and then position
                StageController.Instance.SetCharacterPosition(terms[1], terms[2]);
                break;

            case "move":
                //Slides character to new position with two params, first being name and then position
                StageController.Instance.MoveCharacter(terms[1], terms[2], float.Parse(terms[3]), bool.Parse(terms[4]));
                break;

            case "show":
                if (terms[1] == "text") ShowTextbox();
                else StageController.Instance.ShowCharacter(terms[1]);
                break;

            case "hide":
                if (terms[1] == "text") HideTextbox();
                else StageController.Instance.HideCharacter(terms[1]);
                break;

            case "anim":
                //If the anim prefix is used, two params are expected, first name and then animation
                StageController.Instance.PlayAnim(terms[1], terms[2]);
                break;

            case "remove":
                StageController.Instance.RemoveCharacter(terms[1]);
                break;

            case "music":
                if (terms[1] == "stop") AudioManager.Instance.StopMusic();
                else AudioManager.Instance.StartNewMusic(terms[1]);
                break;

            case "scene":
                StartCoroutine(GameManager.Instance.SceneTransition(terms[1], bool.Parse(terms[2]), float.Parse(terms[3]), terms[4]));
                break;

            case "tutorial":
                TutorialActivation?.Invoke(terms[1]);
                break;
        }
    }

    private void HideTextbox()
    {
        GetComponent<CanvasGroup>().alpha = 0f;
    }

    private void ShowTextbox()
    {
        GetComponent<CanvasGroup>().alpha = 1f;
    }

    private IEnumerator Countdown(float time)
    {
        for (float i = time; i >= 0; i -= Time.deltaTime)
        {
            if (clickProcessed)
            {
                interruptTyping = true;

                yield break;
            }

            yield return null;
        }
    }

    public void StampLastSentence(string text)
    {
        string[] sentences = SplitIntoSentences(text);

        string sentence = sentences[sentences.Length - 1];

        if (sentence.Contains(':'))
        {
            sentence = SeparateSentenceAndShowName(sentence)[1];
        }

        dialogueText.text = SearchForRemainingTagsAndRemove(sentence, 0, false);
    }

    private string[] SeparateSentenceAndShowName(string sentence)
    {
        int colon = sentence.IndexOf(":");
        string name = sentence.Substring(0, colon);
        sentence = sentence.Substring(colon + 2);

        if (name.Contains("{")) {
            string realSpeaker = name.Split('{', '}')[1];

            StageController.Instance.ChooseSpeaker(realSpeaker);

            name = name.Substring(0, name.IndexOf("{"));
        }
        else
        {
            StageController.Instance.ChooseSpeaker(name);
        }

        nameText.SetActive(true);
        nameText.GetComponent<TMP_Text>().text = name;

        return new string[] { name, sentence };
    }

    //Called when ctcButton is clicked
    public void ProcessClick()
    {
        clickProcessed = true;
    }

    private string TagsRemoved(string text)
    {
        string[] sentences = SplitIntoSentences(text);

        List<string> newSentences = new List<string>();

        foreach(string sentence in sentences)
        {
            if(sentence.Substring(0, 1) == "#")
            {
                continue;
            }

            string newSentence = SearchForRemainingTagsAndRemove(sentence, 0, false);
            newSentences.Add(newSentence);
        }

        string newText = "";
        foreach(string newSentence in newSentences)
        {
            newText += newSentence + "\n";
        }

        return newText;
    }

    public string[] SplitIntoSentences(string text)
    {
        string[] sentences = text.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

        return sentences;
    }
}
