using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueScriptableObject", menuName = "ScriptableObjects/Dialogue")]
public class DialogueSO : ScriptableObject
{
    [field: SerializeField] [field: TextArea(10, 100)] public string Text { get; set; }
}
