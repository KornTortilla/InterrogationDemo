using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data
{
    using ScriptableObjects;

    [Serializable]
    public class DialogueErrorData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<ScriptableObject> Evidence { get; set; }
    }
}

