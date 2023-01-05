using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    using Enumerations;

    [Serializable]
    public class InterrogationChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
        [field: SerializeField] public List<string> KeyIDs { get; set; }
        [field: SerializeField] public OutputType Type { get; set; }
    }
}
