using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.ScriptableObjects
{
    using Data;

    public class DialogueManagerInterrogationSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<DialogueInterrogationSO> DialogueList { get; set; }
        [field: SerializeField] public List<EvidenceInterrogationSO> EvidenceList { get; set; }
        [field: SerializeField] public string PartnerName { get; set; }
        [field: SerializeField] public string IntroText { get; set; }
        [field: SerializeField] [field: TextArea] public string NoHintResponse { get; set; }
        [field: SerializeField] [field: TextArea] public string DefaultErrorResponse { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;
            DialogueList = new List<DialogueInterrogationSO>();
            EvidenceList = new List<EvidenceInterrogationSO>();
        }
    }
}


