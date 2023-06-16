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
        [field: SerializeField] public DialogueInterrogationSO NextDialogue { get; set; }
        [field: SerializeField] public List<ScriptableObject> Keys { get; set; }
        [field: SerializeField] public string Hint { get; set; }
        [field: NonSerialized] public bool Opened { get; set; }
    }
}
