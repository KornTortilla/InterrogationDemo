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

    public GameObject nameBox;
    public TextMeshProUGUI dialogueText;
    public GameObject ctcObject;
    public Button ctcButton;

    public float textSpeed;

    private AudioClip blip;

    private bool clickProcessed;
    private bool interruptTyping;
    private bool stageReady = true;

    [HideInInspector] public bool isDone = true;

    private void Awake()
    {
        blip = Resources.Load("Audio/BlipMale") as AudioClip;
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
        string[] sentences = SplitIntoSentences(text);

        if (sentences[0].Substring(0, 1) == "!")
        {
            SeperateAndExecute(sentences[0]);
        }
    }

    public IEnumerator TypeText(string text, bool needToClickLastText = false)
    {
        isDone = false;

        string[] sentences = SplitIntoSentences(text);

        for (int i = 0; i < sentences.Length; i++)
        {
            clickProcessed = false;

            string sentence = sentences[i];

            if(sentence.Substring(0, 1) == "!")
            {
                continue;
            }

            /*
            If sentence begins with #, it is a tag and will sepearate the 
             first and second words to invoke different events.
            */
            if (sentence.Substring(0, 1) == "#")
            {
                SeperateAndExecute(sentence);

                //Skips to next sentence so that tag isn't displayed in text
                continue;
            }

            yield return new WaitUntil(() => stageReady);

            //Initializing beginning to simulate talking
            dialogueText.text = "";
            //By default, sets nameBox to be hidden unless utilized
            nameBox.SetActive(false);
            ctcObject.SetActive(false);

            //If colon detected, name is given
            string name = "";
            if (sentence.Contains(":"))
            {
                //Separates name and dialogue
                string[] sentenceArray = SeparateSentenceAndShowName(sentence);
                name = sentenceArray[0];
                sentence = sentenceArray[1];

                StageController.Instance.ChooseSpeaker(name);
            }

            char[] letterArray = sentence.ToCharArray();
            for (int letterPos = 0; letterPos < letterArray.Length; letterPos++)
            {
                char letter = letterArray[letterPos];

                if (interruptTyping)
                {
                    interruptTyping = false;

                    dialogueText.text = SearchForRemainingTagsAndRemove(sentence, letterPos, true);

                    break;
                }

                float time = textSpeed;

                StageController.Instance.SetCurrentSpeakerTalking(true);

                //odd = !odd;

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
                else if (letter == '(')
                {
                    string parenthesisString = FindParenthesisString(sentence.Substring(letterPos));

                    StripParenthesisAndExecute(parenthesisString);

                    letterPos += parenthesisString.Length - 1;

                    continue;
                }
                /*
                else if(letter != ' ')
                {
                    if(odd) AudioManager.Instance.PlayEffect(blip);
                }
                */

                dialogueText.text += letter;

                yield return StartCoroutine(Countdown(time));
            }

            StageController.Instance.SetCurrentSpeakerTalking(false);

            ctcObject.SetActive(true);

            clickProcessed = false;

            //If the dialogue is done being typed out and the player clicks on button, continue
            yield return new WaitUntil(() => { return ((clickProcessed || (i == sentences.Length - 1 && !needToClickLastText))); });

            SentenceFinished?.Invoke(name, TagsRemoved(sentence));
        }

        ctcObject.SetActive(false);
        isDone = true;
    }

    private void SeperateAndExecute(string sentence)
    {
        //Removes first character
        string line = sentence.Substring(1);
        //Splits each word of a tag into an array
        string[] commands = line.Split(',');

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
        if(!sentence.Contains('('))
        {
            return sentence;
        }

        List<string> stringsToRemove = new List<string>();

        string remainingSentence = sentence.Substring(startingPosition);

        while(remainingSentence.Contains('('))
        {
            string parenthesisString = FindParenthesisString(remainingSentence);

            if(execute)
            {
                StripParenthesisAndExecute(parenthesisString);
            }

            stringsToRemove.Add(parenthesisString);

            remainingSentence = remainingSentence.Substring(remainingSentence.IndexOf(')') + 1);
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
        int startOfParenth = text.IndexOf('(');
        int endOfParenth = text.IndexOf(')');

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
            case "background":
                StageController.Instance.ChangeBackground(terms[1]);
                break;

            case "add":
                //Adds character to stage based on only parameter
                StageController.Instance.AddCharacter(terms[1]);
                break;

            case "move":
                //Sets character to new position with two params, first name and then position
                StageController.Instance.SetCharacterPosition(terms[1], terms[2]);
                break;

            case "show":
                StageController.Instance.ShowCharacter(terms[1]);
                break;

            case "hide":
                StageController.Instance.HideCharacter(terms[1]);
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
                else AudioManager.Instance.PlayNewTrack(terms[1]);
                break;

            case "scene":
                GameManager.Instance.TransitionScenes(terms[1], bool.Parse(terms[2]), float.Parse(terms[3]), terms[4]);
                break;

            case "tutorial":
                TutorialActivation?.Invoke(terms[1]);
                break;
        }
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

        nameBox.SetActive(true);
        nameBox.GetComponentInChildren<TMP_Text>().text = name;

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
