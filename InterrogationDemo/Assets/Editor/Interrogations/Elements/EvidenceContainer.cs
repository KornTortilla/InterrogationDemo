using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Interrogation.Elements
{
    using Windows;
    using Enumerations;
    using Utilities;
    using Data.Save;
    using System.Linq;

    public class EvidenceContainer : VisualElement
    {
        public InterrogationEvidenceSaveData EvidenceData { get; set; }
        public List<EvidenceNode> EvidenceNodes { get; set; }

        public EvidenceContainer(InterrogationGraphView interroGraphView, ProfileBlackboard board, InterrogationEvidenceSaveData evidenceData)
        {
            EvidenceData = evidenceData;

            EvidenceNodes = new List<EvidenceNode>();

            this.style.width = board.style.width;

            VisualElement textContainer = new VisualElement();
            VisualElement creationContianer = new VisualElement();
            VisualElement movementContainer = new VisualElement();

            foreach (string ID in EvidenceData.NodeIDs)
            {
                foreach (BaseNode node in interroGraphView.nodes)
                {
                    board.subTitle = "Yes";

                    if (node.NodeType == NodeType.Evidence)
                    {
                        EvidenceNode eNode = (EvidenceNode)node;

                        if (eNode.ID == ID)
                        {
                            EvidenceNodes.Add(eNode);
                        }
                    }
                }
            }

            TextField evidenceNameField = InterrogationElementUtility.CreateTextArea(evidenceData.Name, null, callback =>
            {
                evidenceData.Name = callback.newValue;

                foreach (EvidenceNode eNode in EvidenceNodes)
                {
                    eNode.nameField.text = callback.newValue;
                }
            });

            evidenceNameField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__filename-textfield",
                "interro-node__textfield__hidden"
            );

            Foldout evidenceFoldout = InterrogationElementUtility.CreateFoldout("Evidence Description");

            TextField evidenceTextField = InterrogationElementUtility.CreateTextArea(evidenceData.Text, null, callback =>
            {
                evidenceData.Text = callback.newValue;
            });

            evidenceTextField.AddStyleClasses(
                "interro-node__textfield",
                "interro-node__quote-textfield"
            );

            Button addEvidenceNodeButton = InterrogationElementUtility.CreateButton("+", () =>
            {
                EvidenceNode eNode = (EvidenceNode)interroGraphView.CreateNode(interroGraphView.contentViewContainer.WorldToLocal(Vector2.zero), NodeType.Evidence, evidenceData.Name);
                eNode.Evidence = evidenceData;

                EvidenceNodes.Add(eNode);

                interroGraphView.AddElement(eNode);
            });

            Button deleteEvidenceButton = InterrogationElementUtility.CreateButton("X", () =>
            {
                board.EvidenceContainers.Remove(this);

                board.Remove(this);

                foreach(EvidenceNode eNode in EvidenceNodes)
                {
                    eNode.DisconnectAllPorts();
                    interroGraphView.RemoveElement(eNode);
                }
            });

            Button moveUpButton = InterrogationElementUtility.CreateButton("Up", () =>
            {
                int position = board.IndexOf(this);

                if(position > 1)
                {
                    board.Insert(position - 1, this);

                    board.EvidenceContainers.Remove(this);
                    board.EvidenceContainers.Insert(position -2, this);
                }
            });

            Button moveDownButton = InterrogationElementUtility.CreateButton("Down", () =>
            {
                int position = board.IndexOf(this);

                board.Insert(position + 1, this);

                board.EvidenceContainers.Remove(this);
                board.EvidenceContainers.Insert(position, this);
            });

            addEvidenceNodeButton.AddToClassList("interro-node__button");
            deleteEvidenceButton.AddToClassList("interro-node__button");
            moveUpButton.AddToClassList("interro-node__button");
            moveDownButton.AddToClassList("interro-node__button");
            creationContianer.AddToClassList("interro-node__evidence-button");
            movementContainer.AddToClassList("interro-node__evidence-button");
            this.AddToClassList("interro-node__evidence-container");

            interroGraphView.Evidence.Add(evidenceData);

            evidenceFoldout.Add(evidenceTextField);

            textContainer.Add(evidenceNameField);
            textContainer.Add(evidenceFoldout);

            creationContianer.Add(addEvidenceNodeButton);
            creationContianer.Add(deleteEvidenceButton);

            movementContainer.Add(moveUpButton);
            movementContainer.Add(moveDownButton);

            this.Add(textContainer);
            this.Add(creationContianer);
            this.Add(movementContainer);

            board.contentContainer.Add(this);
        }
    }
}