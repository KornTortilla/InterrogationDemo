using UnityEngine;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;

    public class EvidenceRepNode : RepNode
    {
        public EvidenceContainer EvidenceContainer { get; set; }

        public override void Initialize(InterrogationGraphView interroGraphView, Vector2 pos, string nodeName)
        {
            base.Initialize(interroGraphView, pos, nodeName);

            NodeType = NodeType.EvidenceRep;
        }

        public void InitializeParent(EvidenceContainer evidenceContainer)
        {
            EvidenceContainer = evidenceContainer;

            EvidenceContainer.EvidenceRepNodes.Add(this);
        }
    }
}