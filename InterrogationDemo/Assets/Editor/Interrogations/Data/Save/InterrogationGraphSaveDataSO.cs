using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    public class InterrogationGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public string DefaultErrorResponse { get; set; }
        [field: SerializeField] public List<InterrogationEvidenceSaveData> Evidence { get; set; }
        [field: SerializeField] public List<InterrogationNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldDialogueNames { get; set; }
        [field: SerializeField] public List<string> OldEvidenceNames { get; set; }

        public void Intialize(string fileName)
        {
            FileName = fileName;
            Evidence = new List<InterrogationEvidenceSaveData>();
            Nodes = new List<InterrogationNodeSaveData>();
        }
    }
}
