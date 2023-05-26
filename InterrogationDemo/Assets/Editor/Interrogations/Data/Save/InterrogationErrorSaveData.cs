using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    using Enumerations;

    [Serializable]
    public class InterrogationErrorSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<string> EvidenceIDs { get; set; }

        [field: SerializeField] public OutputType Type = OutputType.Error;
    }
}