using UnityEngine;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;

    public class DialogueRepNode : RepNode
    {
        public DialogueNode DialogueNode { get; set; }

        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);

            NodeType = NodeType.DialogueRep;
        }

        public void InitializeParent(DialogueNode dialogueNode)
        {
            DialogueNode = dialogueNode;
        }
    }
}


