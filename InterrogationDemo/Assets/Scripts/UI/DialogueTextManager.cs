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

    [HideInInspector]public bool isDone = true;

    private void Awake()
    {
        blip = Resources.Load("Audio/BlipMale") as AudioClip;
    }

    public IEnumerator TypeText(string[] sentences, Animator animator)
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

                string tag = sentence.Substring(1);

                string prefix = tag.Split(' ')[0].ToLower();
                string param = tag.Split(' ')[1];

                switch (prefix)
                {
                    case "anim":
                        animator.Play(param);
                        break;

                    case "music":
                        if (param == "stop") AudioManager.Instance.StopMusic();
                        else AudioManager.Instance.PlayNewTrack(param);
                        break;

                    case "scene":
                        SceneLoader.Load(SceneLoader.Scene.Seven);
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

                //If name given matches the suspect, they can talk but can't otherwise
                if (name != animator.GetLayerName(0))
                {
                    animator.SetBool("isSpeaking", false);
                }
                else
                {
                    animator.SetBool("isSpeaking", true);
                }
            }
            else
            {
                animator.SetBool("isSpeaking", true);
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

                animator.SetBool("talking", true);

                odd = !odd;

                //If using punctuation, increase time delay and stop animation to simulate actual speech better
                if (letter == '.' || letter == '?' || letter == '!')
                {
                    time *= 10;
                    animator.SetBool("talking", false);

                }
                else if (letter == ',')
                {
                    time *= 5f;
                    animator.SetBool("talking", false);
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

            animator.SetBool("talking", false);

            ctcObject.SetActive(true);

            clickProcessed = false;

            //If the dialogue is done being typed out and the player clicks on button, continue
            yield return new WaitUntil(() => { return ((clickProcessed || i == sentences.Length - 1)); });
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
