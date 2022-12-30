using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Interrogation.Ingame;

public class DialogueTextManager : MonoBehaviour
{
    public GameObject nameBox;
    public TextMeshProUGUI dialogueText;
    public GameObject ctcObject;

    public float textSpeed;

    [HideInInspector]public bool isDone = true;

    public IEnumerator TypeText(string sentence, Animator animator)
    {
        //Initializing beginning to simulate talking
        dialogueText.text = "";
        isDone = false;
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
            float time = textSpeed;

            animator.SetBool("talking", true);

            //If using punctuation, increase time delay and stop animation to simulate actual speech better
            if (letter == '.' || letter == '?')
            {
                time *= 10;
                animator.SetBool("talking", false);

            }
            if (letter == ',')
            {
                time *= 5f;
                animator.SetBool("talking", false);
            }

            dialogueText.text += letter;

            yield return new WaitForSeconds(time);
        }

        ctcObject.SetActive(true);

        isDone = true;
    }

    //Show all text at once
    public void ShowText(string sentence)
    {
        dialogueText.text = sentence;
    }

    //Once dialogue has been deemed ended, the ctc object will be disabled
    private void OnEnable()
    {
        InterrogationDialogueManager.OnTalkingEnd += EndDialgogue;
    }

    private void OnDisable()
    {
        InterrogationDialogueManager.OnTalkingEnd -= EndDialgogue;
    }

    private void EndDialgogue()
    {
        ctcObject.SetActive(false);
    }
}
