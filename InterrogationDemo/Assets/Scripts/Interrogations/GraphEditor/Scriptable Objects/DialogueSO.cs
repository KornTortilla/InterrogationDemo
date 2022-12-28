using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.ScriptableObjects
{
    using Data;

    public class DialogueSO : ScriptableObject
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public List<string> Speakers { get; set; }
        [field: SerializeField] [field: TextArea] public string Text { get; set; }
        [field: SerializeField] public List<DialogueChoiceData> Choices { get; set; }
        [field: SerializeField] public DialogueSO PreviousDialogue { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: NonSerialized] public bool Grabbed { get; set; }
        [field: NonSerialized] public bool Seen { get; set; }

        public void Initialize(string dialogueName, string text, List<DialogueChoiceData> choices, Vector2 pos)
        {
            Name = dialogueName;
            Text = text;
            Choices = choices;
            Position = pos;
        }
    }
}
