using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data
{
    using ScriptableObjects;

    [Serializable]
    public class DialogueChoiceData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public DialogueSO NextDialogue { get; set; }
        [field: SerializeField] public List<ScriptableObject> Keys { get; set; }
        [field: NonSerialized] public bool Opened { get; set; }
    }
}
