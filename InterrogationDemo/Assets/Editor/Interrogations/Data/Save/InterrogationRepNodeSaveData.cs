using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    using Enumerations;

    [Serializable]
    public class InterrogationRepNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string ParentID { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
        [field: SerializeField] public NodeType NodeType { get; set; }
    }
}

