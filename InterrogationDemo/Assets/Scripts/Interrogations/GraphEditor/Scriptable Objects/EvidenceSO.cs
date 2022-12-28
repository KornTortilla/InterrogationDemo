using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.ScriptableObjects
{
    [Serializable]
    public class EvidenceSO : ScriptableObject
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }

        public void Initialize(string name, string text)
        {
            Name = name;
            Text = text;
        }
    }
}