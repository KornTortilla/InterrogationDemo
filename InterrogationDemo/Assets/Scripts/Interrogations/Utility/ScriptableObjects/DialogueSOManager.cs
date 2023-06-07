using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.ScriptableObjects
{
    using Data;

    public class DialogueSOManager : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<DialogueSO> DialogueList { get; set; }
        [field: SerializeField] public List<EvidenceSO> EvidenceList { get; set; }
        [field: SerializeField] [field: TextArea] public string NoHintResponse { get; set; }
        [field: SerializeField] [field: TextArea] public string DefaultErrorResponse { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueList = new List<DialogueSO>();
            EvidenceList = new List<EvidenceSO>();
        }
    }
}


