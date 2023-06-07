using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    public class InterrogationGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public string NoHintResponse { get; set; }
        [field: SerializeField] public string DefaultErrorResponse { get; set; }
        [field: SerializeField] public List<InterrogationEvidenceSaveData> Evidence { get; set; }
        [field: SerializeField] public List<InterrogationDialogueNodeSaveData> DialogueNodes { get; set; }
        [field: SerializeField] public List<InterrogationRepNodeSaveData> RepNodes { get; set; }
        [field: SerializeField] public List<string> OldDialogueNames { get; set; }
        [field: SerializeField] public List<string> OldEvidenceNames { get; set; }

        public void Intialize(string fileName)
        {
            FileName = fileName;
            Evidence = new List<InterrogationEvidenceSaveData>();
            DialogueNodes = new List<InterrogationDialogueNodeSaveData>();
            RepNodes = new List<InterrogationRepNodeSaveData>();
        }
    }
}
