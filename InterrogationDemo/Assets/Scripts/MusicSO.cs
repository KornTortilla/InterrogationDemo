using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicScriptableObject", menuName = "ScriptableObjects/Music")]
public class MusicSO : ScriptableObject
{
    [field: SerializeField] public List<AudioClip> MusicList { get; set; }
}
