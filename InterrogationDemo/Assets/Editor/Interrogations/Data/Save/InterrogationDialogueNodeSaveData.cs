using System;
using System.Collections.Generic;
using UnityEngine;

namespace Interrogation.Data.Save
{
    using Enumerations;

    [Serializable]
    public class InterrogationDialogueNodeSaveData
    {
        [field: SerializeField] public string ID { get; set; }
        [field: SerializeField] public string Name { get; set; }
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public List<InterrogationChoiceSaveData> Choices { get; set; }
        [field: SerializeField] public List<InterrogationErrorSaveData> Errors { get; set; }
        [field: SerializeField] public Vector2 Position { get; set; }
    }
}