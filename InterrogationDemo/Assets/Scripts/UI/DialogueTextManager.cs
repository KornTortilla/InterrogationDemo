using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueTextManager : MonoBehaviour
{
    public static event Action OnGameEnd;

    public GameObject nameBox;
    public TextMeshProUGUI dialogueText;
    public GameObject ctcObject;
    public Button ctcButton;

    public float textSpeed;

    private AudioClip blip;

    private bool clickProcessed;
    private bool interruptTyping;
    private bool odd = false;

    [HideInInspector] public bool isDone = true;

    private void Awake()
    {
        blip = Resources.Load("Audio/BlipMale") as AudioClip;
    }

    public IEnumerator TypeText(string[] sentences, bool needToClickLastText = false)
    {
        isDone = false;

        for (int i = 0; i < sentences.Length; i++)
        {
            clickProcessed = false;

            string sentence = sentences[i];

            /*
            If sentence begins with #, it is a tag and will sepearate the 
             first and second words to invoke different events.
            */
            if (sentence.Substring(0, 1) == "#")
            {
                //Removes first character
                string tag = sentence.Substring(1);
                //Splits each word of a tag into an array
                string[] terms = tag.Split(' ');
                //Checks the first 
                switch (terms[0])
                {
                    case "add":
                        //Adds character to stage based on only parameter
                        StageController.Instance.AddCharacter(terms[1]);
                        break;

                    case "move":
                        //Sets character to new position with two params, first name and then position
                        StageController.Instance.SetCharacterPosition(terms[1], terms[2]);
                        break;

                    case "anim":
                        //If the anim prefix is used, two params are expected, first name and then animation
                        StageController.Instance.PlayAnim(terms[1], terms[2]);
                        break;

                    case "music":
                        if (terms[1] == "stop") AudioManager.Instance.StopMusic();
                        else AudioManager.Instance.PlayNewTrack(terms[1]);
                        break;

                    case "scene":
                        GameManager.Instance.TransitionScenes(terms[1], terms[2]);
                        break;

                    case "game":
                        OnGameEnd?.Invoke();
                        break;

                    case "fadein":

                        break;
                }

                //Skips to next sentence so that tag isn't displayed in text
                continue;
            }

            //Initializing beginning to simulate talking
            dialogueText.text = "";
            //By default, sets nameBox to be hidden unless utilized
            nameBox.SetActive(false);
            ctcObject.SetActive(false);

            //If colon detected, name is given
            if (sentence.Contains(":"))
            {
                //Separates name and dialogue
                int colon = sentence.IndexOf(":");
                string name = sentence.Substring(0, colon);
                sentence = sentence.Substring(colon + 2);

                nameBox.SetActive(true);
                nameBox.GetComponentInChildren<TMP_Text>().text = name;

                StageController.Instance.ChooseSpeaker(name);
            }

            foreach (char letter in sentence.ToCharArray())
            {
                if(interruptTyping)
                {
                    interruptTyping = false;

                    ShowText(sentence);

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
        }

        ctcObject.SetActive(false);
        isDone = true;
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
    
    public void ShowName(string name)
    {
        nameBox.GetComponentInChildren<TMP_Text>().text = name;
    }

    //Show all text at once
    public void ShowText(string sentence)
    {
        dialogueText.text = sentence;
    }

    //Called when ctcButton is clicked
    public void ProcessClick()
    {
        clickProcessed = true;
    }
}
