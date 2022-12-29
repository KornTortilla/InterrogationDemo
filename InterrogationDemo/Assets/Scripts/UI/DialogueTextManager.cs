using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueTextManager : MonoBehaviour
{
    public GameObject nameBox;
    public TextMeshProUGUI dialogueText;
    public GameObject ctcObject;

    public float textSpeed;

    [HideInInspector]public bool isDone = true;

    public IEnumerator TypeText(string sentence, Animator animator)
    {
        dialogueText.text = "";
        isDone = false;
        nameBox.SetActive(false);
        ctcObject.SetActive(false);

        if (sentence.Contains(":"))
        {
            int colon = sentence.IndexOf(":");
            string name = sentence.Substring(0, colon);

            nameBox.SetActive(true);
            nameBox.GetComponentInChildren<TMP_Text>().text = name;

            if (name != animator.GetLayerName(0))
            {
                animator.SetBool("isSpeaking", false);
            }
            else
            {
                animator.SetBool("isSpeaking", true);
            }

            sentence = sentence.Substring(colon + 2);
        }
        else
        {
            Debug.Log("Speaking");
            animator.SetBool("isSpeaking", true);
        }

        foreach (char letter in sentence.ToCharArray())
        {
            float time = textSpeed;

            animator.SetBool("talking", true);

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

    public void ShowText(string sentence)
    {
        dialogueText.text = sentence;
    }
}
