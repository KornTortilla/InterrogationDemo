using System;
using UnityEngine;

namespace Interrogation.Data.Save
{
    [Serializable]
    public class InterrogationEvidenceSaveData
    {
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Text { get; set; }

    }
}

