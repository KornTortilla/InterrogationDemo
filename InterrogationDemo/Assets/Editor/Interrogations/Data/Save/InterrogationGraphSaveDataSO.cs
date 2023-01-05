using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    public class InterrogationGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField] public string FileName { get; set; }
        [field: SerializeField] public List<InterrogationNodeSaveData> Nodes { get; set; }
        [field: SerializeField] public List<string> OldDialogueNames { get; set; }
        [field: SerializeField] public List<string> OldEvidenceNames { get; set; }

        public void Intialize(string fileName)
        {
            FileName = fileName;

            Nodes = new List<InterrogationNodeSaveData>();
        }
    }
}
