using System;
using UnityEngine;

public class DialogueInjector : MonoBehaviour
{
    [SerializeField] private DialogueTextManager dialogueTextManager;
    [SerializeField] private DialogueSO dialogueSO;

    private void OnEnable()
    {
        GameManager.OnSceneTransitionEnd += Inject;
    }

    private void OnDisable()
    {
        GameManager.OnSceneTransitionEnd -= Inject;
    }

    public void Inject()
    {
        string[] sentences = dialogueSO.Text.Split(
                new string[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );

        StartCoroutine(dialogueTextManager.TypeText(sentences));
    }
}
