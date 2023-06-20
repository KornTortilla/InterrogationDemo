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

    private void Awake()
    {
        dialogueTextManager.CheckForPriorityCommand(dialogueSO.Text);
    }

    public void Inject()
    {
        StartCoroutine(dialogueTextManager.TypeText(dialogueSO.Text));
    }
}
