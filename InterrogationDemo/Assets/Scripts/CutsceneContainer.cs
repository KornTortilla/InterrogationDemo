using System;
using UnityEngine;

public class CutsceneContainer : MonoBehaviour
{
    [field: TextArea] public string text;

    [SerializeField] private DialogueTextManager dialogueTextManager;

    public void Start()
    {
        string[] sentences = text.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

        StartCoroutine(dialogueTextManager.TypeText(sentences));
    }
}
